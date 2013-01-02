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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using apophis.SharpIRC;
using apophis.SharpIRC.IrcFeatures;
using Huffelpuff;
using Huffelpuff.Plugins;
using Huffelpuff.Utils;
using Nntp;
using Plugin.Properties;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class NntpPlugin : AbstractPlugin
    {
        public NntpPlugin(IrcBot botInstance) : base(botInstance) { }

        private NntpConnection nntp;
        private Timer interval;
        private readonly Dictionary<string, Dictionary<string, Article>> nntpCache = new Dictionary<string, Dictionary<string, Article>>();
        private bool firstTime = true;
        private List<string> filterGroups;
        private List<string> filterWords;
        // TODO: look at this
        // private DccChat chat;


        public override void Init()
        {
            nntp = new NntpConnection();
            interval = new Timer(30000);
            interval.Elapsed += IntervalElapsed;

            filterGroups = NntpSettings.Default.FilteredGroups.Split(',').ToList();
            filterWords = NntpSettings.Default.FilteredWords.Split(',').ToList();
            base.Init();
        }


        private bool lockInterval;

        public void IntervalElapsed(object sender, ElapsedEventArgs e)
        {
            if (lockInterval)
                return;
            lockInterval = true;
            try
            {
                nntp.ConnectServer(NntpSettings.Default.NntpServer, NntpSettings.Default.NntpPort);
                var newMessages = new SortedDictionary<DateTime, KeyValuePair<Article, string>>();

                foreach (Newsgroup ng in nntp.GetGroupList())
                {
                    if (!nntpCache.ContainsKey(ng.Group))
                    {
                        nntpCache.Add(ng.Group, new Dictionary<string, Article>());
                    }

                    ArrayList list;
                    Newsgroup cng = nntp.ConnectGroup(ng.Group);

                    try
                    {
                        list = nntp.GetArticleList(cng.Low, cng.High);
                    }
                    catch (Exception) { list = new ArrayList(); }
                    // var r = new Random();
                    Newsgroup newsgroup = ng;
                    foreach (var a in from Article a in list where !nntpCache[newsgroup.Group].ContainsKey(a.MessageId) select a)
                    {
                        // new message
                        nntpCache[ng.Group].Add(a.MessageId, a);
                        if (!newMessages.ContainsKey(a.Header.Date))
                            newMessages.Add(a.Header.Date, new KeyValuePair<Article, string>(a, ng.Group));
                    }
                }

                if (firstTime)
                {
                    var t = new TimeSpan(0);
                    foreach (var kvp in newMessages)
                    {
                        t = (DateTime.Now - kvp.Key);
                    }
                    foreach (var channel in NntpSettings.Default.NntpChannels.Split(','))
                    {
                        BotMethods.SendMessage(SendType.Message, channel, "New FMS Posts will be reported: There are " + newMessages.Count + " Messages in the repository");
                        BotMethods.SendMessage(SendType.Message, channel, "Last Post on FMS was " + Math.Round(t.TotalMinutes) + " minutes ago.");
                    }
                    nntp.Disconnect();
                    firstTime = false;
                    lockInterval = false;
                    return;
                }

                foreach (var m in newMessages)
                {
                    foreach (var channel in NntpSettings.Default.NntpChannels.Split(','))
                    {
                        var t = (DateTime.Now - m.Value.Key.Header.Date);
                        if (t >= new TimeSpan(4, 0, 0)) continue;
                        if (WordFilter(m.Value.Key))
                        {
                            BotMethods.SendMessage(SendType.Message, channel, "New Post on FMS: " + IrcConstants.IrcBold + "*** Word Filtered ***" + IrcConstants.IrcBold + " by " + m.Value.Key.Header.From.Split(new[] { '@' })[0] + " in Board " + m.Value.Value + " (" + Math.Round(t.TotalMinutes) + " minutes ago)");
                        }
                        else if (GroupFilter(m.Value.Value))
                        {
                            BotMethods.SendMessage(SendType.Message, channel, "New Post on FMS: " + IrcConstants.IrcBold + "*** Group Filtered ***" + IrcConstants.IrcBold + " by " + m.Value.Key.Header.From.Split(new[] { '@' })[0] + " in Board " + m.Value.Value + " (" + Math.Round(t.TotalMinutes) + " minutes ago)");
                        }
                        else
                        {
                            BotMethods.SendMessage(SendType.Message, channel, "New Post on FMS: " + IrcConstants.IrcBold + m.Value.Key.Header.Subject + IrcConstants.IrcBold + " by " + m.Value.Key.Header.From.Split(new[] { '@' })[0] + " in Board " + m.Value.Value + " (" + Math.Round(t.TotalMinutes) + " minutes ago)");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Instance.Log(exception);
            }
            finally
            {
                lockInterval = false;
                nntp.Disconnect();
            }
        }

        private bool WordFilter(Article a)
        {
            return filterWords.Aggregate(false, (current, s) => current | a.Header.Subject.Contains(s));
        }

        private bool GroupFilter(string group)
        {
            return filterGroups.Aggregate(false, (current, s) => current | group.Contains(s));
        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!poster", "!poster <name> : Queries Stats about FMS User <name>", PosterCommand, this));
            BotMethods.AddCommand(new Commandlet("!group", "!group <name> : Queries Stats about FMS Group <name>", GroupCommand, this));
            BotMethods.AddCommand(new Commandlet("!24h", "!24h : Stats about the last 24 hours", LastDayCommand, this));
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

        private void PosterCommand(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length > 1)
            {
                PosterStats(e);
            }
        }
        private void GroupCommand(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length > 1)
            {
                GroupStats(e);
            }
            else
            {
                ListGroups(e);
            }
        }

        private void LastDayCommand(object sender, IrcEventArgs e)
        {
            var timeframe = new TimeSpan(25, 0, 0);
            Stats(e, timeframe);
        }

        private void GroupStats(IrcEventArgs e)
        {
            if (!nntpCache.ContainsKey(e.Data.MessageArray[1]))
            {
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " does not exist");
                return;
            }
            int posts = 0;
            Article lastArticle = null;

            foreach (KeyValuePair<string, Article> article in nntpCache[e.Data.MessageArray[1]])
            {
                posts++;
                if ((lastArticle == null) || (article.Value.Header.Date > lastArticle.Header.Date))
                {
                    lastArticle = article.Value;
                }
            }
            var t = new TimeSpan(0);
            if (lastArticle != null)
            {
                t = (DateTime.Now - lastArticle.Header.Date);
            }
            if (GroupFilter(e.Data.MessageArray[1]))
            {
                if (lastArticle == null)
                {
                    BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " has no posts.  (*** Group Filtered ***)");
                }
                else
                {
                    BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " : " + posts + " Posts, Last Post " + Math.Round(t.TotalHours) + " hours ago by " + lastArticle.Header.From.Split(new[] { '@' })[0] + " (*** Group Filtered ***)");
                }
            }
            else
            {
                if (lastArticle == null)
                {
                    BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " has no posts.");
                }
                else
                {
                    BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " : " + posts + " Posts, Last Post " + Math.Round(t.TotalHours) + " hours ago by " + lastArticle.Header.From.Split(new[] { '@' })[0] + " (" + lastArticle.Header.Subject + ")");
                }
            }
        }


        private void ListGroups(IrcEventArgs e)
        {
            foreach (var s in nntpCache.Keys.ToLines(350))
            {
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, s);
            }
        }


        private void PosterStats(IrcEventArgs e)
        {
            int posts = 0, lines = 0;
            var groups = new Dictionary<string, int>();
            Article lastArticle = null;
            string lastGroup = "no group";
            foreach (KeyValuePair<string, Dictionary<string, Article>> group in nntpCache)
            {
                foreach (var article in group.Value.Where(article => article.Value.Header.From.Split(new[] { '@' })[0] == e.Data.MessageArray[1]))
                {
                    posts++;
                    lines += article.Value.Header.LineCount;
                    if (!groups.ContainsKey(group.Key))
                    {
                        groups.Add(group.Key, 0);
                    }
                    groups[group.Key]++;
                    if (lastArticle == null || article.Value.Header.Date > lastArticle.Header.Date)
                    {
                        lastArticle = article.Value;
                        lastGroup = group.Key;
                    }
                }
            }
            if (posts > 0)
            {
                if (lastArticle != null)
                {
                    var t = (DateTime.Now - lastArticle.Header.Date);
                    BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Poster " + e.Data.MessageArray[1] + " has posted " + posts + " times, in " + groups.Count + " groups and wrote " + lines + " lines of text. Last post: " + IrcConstants.IrcBold + lastArticle.Header.Subject + IrcConstants.IrcBold + " in Board " + lastGroup + " (" + Math.Round(t.TotalHours) + " hours ago)");
                }
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Poster " + e.Data.MessageArray[1] + " not found.");
            }
        }


        private void Stats(IrcEventArgs e, TimeSpan timeframe)
        {
            var posts = 0;
            var groups = new Dictionary<string, int>();
            var posters = new Dictionary<string, int>();
            foreach (var @group in nntpCache)
            {
                foreach (var article in @group.Value.Where(article => article.Value.Header.Date > (DateTime.Now - timeframe)))
                {
                    posts++;
                    if (!groups.ContainsKey(@group.Key))
                    {
                        groups.Add(@group.Key, 0);
                    }
                    groups[@group.Key]++;
                    if (!posters.ContainsKey(article.Value.Header.From))
                    {
                        posters.Add(article.Value.Header.From, 0);
                    }
                    posters[article.Value.Header.From]++;
                }
            }
            BotMethods.SendMessage(SendType.Message, e.Data.Channel, "In the last 24hours there have been (at least) " + posts + " posts in " + groups.Count + " groups by " + posters.Count + " posters in FMS.");
        }
    }
}