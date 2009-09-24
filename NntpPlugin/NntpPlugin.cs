/*
 *  The NNTP Plugin can provide information about postings to a Usenet Server
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>. 
 */
 
using System;
using System.Timers;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Huffelpuff;
using Huffelpuff.Plugins;
using Huffelpuff.Utils;

using Nntp;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class NntpPlugin : AbstractPlugin
    {
        public NntpPlugin(IrcBot botInstance) : base(botInstance) {}
        
        private NntpConnection nntp;
        private Timer interval;
        private Dictionary<string, Dictionary<string, Article>> nntpCache = new Dictionary<string, Dictionary<string, Article>>();
        private bool firstTime = true;
        private List<string> filterGroups;
        private List<string> filterWords;
        // TODO: look at this
        // private DccChat chat;
        
        
        public override void Init()
        {
            nntp = new NntpConnection();
            interval = new System.Timers.Timer(30000);
            interval.Elapsed += new ElapsedEventHandler(interval_Elapsed);
            
            filterGroups = PersistentMemory.Instance.GetValues("nntp", "GroupFilter");
            filterWords = PersistentMemory.Instance.GetValues("nntp", "WordFilter");
            base.Init();
        }
        
        
        private bool lock_interval = false;
        
        public void interval_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (lock_interval)
                return;
            lock_interval = true;
            try{
                nntp.ConnectServer(PersistentMemory.Instance.GetValueOrTodo("nntpServer"), int.Parse(PersistentMemory.Instance.GetValueOrTodo("nntpPort")));
                SortedDictionary<DateTime, KeyValuePair<Article, string>> newMessages = new SortedDictionary<DateTime, KeyValuePair<Article, string>>();
                
                foreach(Newsgroup ng in nntp.GetGroupList())
                {
                    if (!nntpCache.ContainsKey(ng.Group)) {
                        nntpCache.Add(ng.Group, new Dictionary<string, Article>());
                    }
                    
                    ArrayList list;
                    Newsgroup cng = nntp.ConnectGroup(ng.Group);
                    
                    try {
                        list = nntp.GetArticleList(cng.Low, cng.High);
                    } catch(Exception) { list = new ArrayList(); }
                    Random r = new Random();
                    foreach(Article a in list)
                    {
                        if(!nntpCache[ng.Group].ContainsKey(a.MessageId))
                        {
                            // new message
                            nntpCache[ng.Group].Add(a.MessageId, a);
                            if (!newMessages.ContainsKey(a.Header.Date))
                                newMessages.Add(a.Header.Date, new KeyValuePair<Article, string>(a, ng.Group));
                        }
                    }
                }
                
                if (firstTime) {
                    TimeSpan t = new TimeSpan(0);
                    foreach(KeyValuePair<DateTime, KeyValuePair<Article, string>>kvp in newMessages)
                    {
                        t = (DateTime.Now - kvp.Key);
                    }
                    foreach(string channel in PersistentMemory.Instance.GetValues("nntpChannel")) {
                        BotMethods.SendMessage(SendType.Message, channel, "New FMS Posts will be reported: There are "+newMessages.Count+" Messages in the repository");
                        BotMethods.SendMessage(SendType.Message, channel, "Last Post on FMS was " + Math.Round(t.TotalMinutes) + " minutes ago.");
                    }
                    nntp.Disconnect();
                    firstTime = false;
                    lock_interval = false;
                    return;
                }
                
                foreach(KeyValuePair<DateTime, KeyValuePair<Article, string>>m in newMessages)
                {
                    foreach(string channel in PersistentMemory.Instance.GetValues("nntpChannel")) {
                        TimeSpan t = (DateTime.Now - m.Value.Key.Header.Date);
                        if (t < new TimeSpan(4,0,0)) {
                            if(WordFilter(m.Value.Key))
                            {
                                BotMethods.SendMessage(SendType.Message, channel, "New Post on FMS: " + IrcConstants.IrcBold + "*** Word Filtered ***" + IrcConstants.IrcBold + " by " + m.Value.Key.Header.From.Split(new char[] {'@'})[0] + " in Board " + m.Value.Value + " (" + Math.Round(t.TotalMinutes) + " minutes ago)");
                            } else if (GroupFilter(m.Value.Value)) {
                                BotMethods.SendMessage(SendType.Message, channel, "New Post on FMS: " + IrcConstants.IrcBold + "*** Group Filtered ***" + IrcConstants.IrcBold + " by " + m.Value.Key.Header.From.Split(new char[] {'@'})[0] + " in Board " + m.Value.Value + " (" + Math.Round(t.TotalMinutes) + " minutes ago)");
                            } else {
                                BotMethods.SendMessage(SendType.Message, channel, "New Post on FMS: " + IrcConstants.IrcBold + m.Value.Key.Header.Subject + IrcConstants.IrcBold + " by " + m.Value.Key.Header.From.Split(new char[] {'@'})[0] + " in Board " + m.Value.Value + " (" + Math.Round(t.TotalMinutes) + " minutes ago)");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                Log.Instance.Log(ex.Message, Level.Error);
                Log.Instance.Log(ex.StackTrace, Level.Error);
            } finally {
                lock_interval = false;
                nntp.Disconnect();
            }
        }
        
        private bool WordFilter(Article a)
        {
            bool filter = false;
            foreach(string s in filterWords)
            {
                filter |= a.Header.Subject.Contains(s);
            }
            return filter;
        }

        private bool GroupFilter(string group)
        {
            bool filter = false;
            foreach(string s in filterGroups)
            {
                filter |= group.Contains(s);
            }
            return filter;
        }
        
        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!poster", "!poster <name> : Queries Stats about FMS User <name>", PosterCommand, this));
            BotMethods.AddCommand(new Commandlet("!group", "!group <name> : Queries Stats about FMS Group <name>", GroupCommand, this));
            BotMethods.AddCommand(new Commandlet("!24h", "!24h : Stats about the last 24 hours", lastDayCommand, this));
            //bot.AddPublicCommand(new Commandlet("!lastmsg", "!poster <name> : Queries Stats about FMS User <name>", LastMessageCommand, this));
                                 
            interval.Enabled = true;
            base.Activate();
        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!poster");
            BotMethods.RemoveCommand("!group");
            BotMethods.RemoveCommand("!24h");
            //bot.RemovePublicCommand("!lastmsg");
            interval.Enabled = false;
            base.Deactivate();
        }
        
        public override string AboutHelp()
        {
            return "The FMS Module uses the public nntp Gateway to the FMS groups.";
        }
                
        private void PosterCommand(object sender, IrcEventArgs e) {
            if(e.Data.MessageArray.Length > 1) {
                PosterStats(e);
            } else {
                /* How many posters? */
            }
        }
        private void GroupCommand(object sender, IrcEventArgs e) {
            if(e.Data.MessageArray.Length > 1) {
                GroupStats(e);
            } else {
                ListGroups(e);
            }
        }
        
        private void lastDayCommand(object sender, IrcEventArgs e) {
            TimeSpan timeframe = new TimeSpan(25,0,0);
            Stats(e, timeframe);
        }
        
        private void LastMessageCommand(object sender, IrcEventArgs e) {
        }

        private void GroupStats(IrcEventArgs e)
        {
            if (!nntpCache.ContainsKey(e.Data.MessageArray[1])) {
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " does not exist");
                return;
            }
            int posts = 0;
            Article LastArticle = null;

            foreach (KeyValuePair<string, Article> article in nntpCache[e.Data.MessageArray[1]]) {
                posts++;
                if ((LastArticle == null) || (article.Value.Header.Date > LastArticle.Header.Date)) {
                    LastArticle = article.Value;
                }
            }
            TimeSpan t = new TimeSpan(0);
            if (LastArticle != null) {
                t = (DateTime.Now - LastArticle.Header.Date);
            }
            if (GroupFilter(e.Data.MessageArray[1])) {
                if (LastArticle == null) {
                    BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " has no posts.  (*** Group Filtered ***)");
                }
                else {
                    BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " : " + posts + " Posts, Last Post " + Math.Round(t.TotalHours) + " hours ago by " + LastArticle.Header.From.Split(new char[] { '@' })[0] + " (*** Group Filtered ***)");
                }
            }
            else {
                if (LastArticle == null) {
                    BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " has no posts.");
                }
                else {
                    BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " : " + posts + " Posts, Last Post " + Math.Round(t.TotalHours) + " hours ago by " + LastArticle.Header.From.Split(new char[] { '@' })[0] + " (" + LastArticle.Header.Subject + ")");
                }
            }
        }


        private void ListGroups(IrcEventArgs e)
        {
            foreach (string s in nntpCache.Keys.ToLines(350)) {
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, s);
            }
        }


        private void PosterStats(IrcEventArgs e)
        {
            int posts = 0, lines = 0;
            Dictionary<string, int> groups = new Dictionary<string, int>();
            Article LastArticle = null;
            string LastGroup = "no group";
            foreach (KeyValuePair<string, Dictionary<string, Article>> group in nntpCache) {
                foreach (KeyValuePair<string, Article> article in group.Value) {
                    if (article.Value.Header.From.Split(new char[] { '@' })[0] == e.Data.MessageArray[1]) {
                        posts++;
                        lines += article.Value.Header.LineCount;
                        if (!groups.ContainsKey(group.Key)) {
                            groups.Add(group.Key, 0);
                        }
                        groups[group.Key]++;
                        if (LastArticle == null || article.Value.Header.Date > LastArticle.Header.Date) {
                            LastArticle = article.Value;
                            LastGroup = group.Key;
                        }
                    }
                }
            }
            if (posts > 0) {
                TimeSpan t = (DateTime.Now - LastArticle.Header.Date);
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Poster " + e.Data.MessageArray[1] + " has posted " + posts + " times, in " + groups.Count + " groups and wrote " + lines + " lines of text. Last post: " + IrcConstants.IrcBold + LastArticle.Header.Subject + IrcConstants.IrcBold + " in Board " + LastGroup + " (" + Math.Round(t.TotalHours) + " hours ago)");
            }
            else {
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Poster " + e.Data.MessageArray[1] + " not found.");
            }
        }


        private void Stats(IrcEventArgs e, TimeSpan timeframe)
        {
            int posts = 0;
            Dictionary<string, int> groups = new Dictionary<string, int>();
            Dictionary<string, int> posters = new Dictionary<string, int>();
            foreach (KeyValuePair<string, Dictionary<string, Article>> @group in nntpCache) {
                foreach (KeyValuePair<string, Article> article in @group.Value) {
                    if (article.Value.Header.Date > (DateTime.Now-timeframe)) {
                        posts++;
                        if (!groups.ContainsKey(@group.Key)) {
                            groups.Add(@group.Key, 0);
                        }
                        groups[@group.Key]++;
                        if (!posters.ContainsKey(article.Value.Header.From)) {
                            posters.Add(article.Value.Header.From, 0);
                        }
                        posters[article.Value.Header.From]++;
                    }
                }
            }
            BotMethods.SendMessage(SendType.Message, e.Data.Channel, "In the last 24hours there have been (at least) " + posts + " posts in " + groups.Count + " groups by " + posters.Count + " posters in FMS.");
        }
    }
}