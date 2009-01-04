/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 12.11.2008
 * Zeit: 18:49
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Timers;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Huffelpuff;
using Huffelpuff.SimplePlugins;

using Nntp;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class NntpPlugin : IPlugin
    {
        
        private NntpConnection nntp;
        private IrcBot bot;
        private Timer interval;
        private Dictionary<string, Dictionary<string, Article>> nntpCache = new Dictionary<string, Dictionary<string, Article>>();
        private bool firstTime = true;
        private List<string> filterGroups;
        private List<string> filterWords;
        private DccChat chat;
        
        
        public string Name {
            get {
                return Assembly.GetExecutingAssembly().FullName;
            }
        }
        
        private bool ready = false;
        public bool Ready {
            get {
                return ready;
            }
        }
        
        private bool active;
        public bool Active {
            get {
                return active;
            }
        }
        
        
        public void Init(IrcBot botInstance)
        {
            bot = botInstance;
            nntp = new NntpConnection();
            interval = new System.Timers.Timer(30000);
            interval.Elapsed += new ElapsedEventHandler(interval_Elapsed);
            
            filterGroups = PersistentMemory.GetValues("nntp", "GroupFilter");
            filterWords = PersistentMemory.GetValues("nntp", "WordFilter");
            ready = true;
        }
        
        private bool lock_interval = false;
        
        public void interval_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (lock_interval)
                return;
            lock_interval = true;
            try{
                nntp.ConnectServer(PersistentMemory.GetValue("nntpServer"), int.Parse(PersistentMemory.GetValue("nntpPort")));
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
                    } catch(Exception ex) { list = new ArrayList(); }
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
                    foreach(string channel in PersistentMemory.GetValues("nntpChannel")) {
                        bot.SendMessage(SendType.Message, channel, "New FMS Posts will be reported: There are "+newMessages.Count+" Messages in the repository");
                        bot.SendMessage(SendType.Message, channel, "Last Post on FMS was " + Math.Round(t.TotalMinutes) + " minutes ago.");
                    }
                    nntp.Disconnect();
                    firstTime = false;
                    lock_interval = false;
                    return;
                }
                
                foreach(KeyValuePair<DateTime, KeyValuePair<Article, string>>m in newMessages)
                {
                    foreach(string channel in PersistentMemory.GetValues("nntpChannel")) {
                        TimeSpan t = (DateTime.Now - m.Value.Key.Header.Date);
                        if (t < new TimeSpan(4,0,0)) {
                            if(WordFilter(m.Value.Key))
                            {
                                bot.SendMessage(SendType.Message, channel, "New Post on FMS: " + IrcConstants.IrcBold + "*** Word Filtered ***" + IrcConstants.IrcBold + " by " + m.Value.Key.Header.From.Split(new char[] {'@'})[0] + " in Board " + m.Value.Value + " (" + Math.Round(t.TotalMinutes) + " minutes ago)");
                            } else if (GroupFilter(m.Value.Value)) {
                                bot.SendMessage(SendType.Message, channel, "New Post on FMS: " + IrcConstants.IrcBold + "*** Group Filtered ***" + IrcConstants.IrcBold + " by " + m.Value.Key.Header.From.Split(new char[] {'@'})[0] + " in Board " + m.Value.Value + " (" + Math.Round(t.TotalMinutes) + " minutes ago)");
                            } else {
                                bot.SendMessage(SendType.Message, channel, "New Post on FMS: " + IrcConstants.IrcBold + m.Value.Key.Header.Subject + IrcConstants.IrcBold + " by " + m.Value.Key.Header.From.Split(new char[] {'@'})[0] + " in Board " + m.Value.Value + " (" + Math.Round(t.TotalMinutes) + " minutes ago)");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
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

        public void Activate()
        {
            bot.AddPublicCommand(new Commandlet("!poster", "!poster <name> : Queries Stats about FMS User <name>", PosterCommand, this));
            bot.AddPublicCommand(new Commandlet("!group", "!group <name> : Queries Stats about FMS Group <name>", GroupCommand, this));
            bot.AddPublicCommand(new Commandlet("!24h", "!24h : Stats about the last 24 hours", lastDayCommand, this));
            //bot.AddPublicCommand(new Commandlet("!lastmsg", "!poster <name> : Queries Stats about FMS User <name>", LastMessageCommand, this));
                                 
            interval.Enabled = true;
            active = true;
        }
        
        public void Deactivate()
        {
            bot.RemovePublicCommand("!poster");
            bot.RemovePublicCommand("!group");
            bot.RemovePublicCommand("!24h");
            //bot.RemovePublicCommand("!lastmsg");
            interval.Enabled = false;
            active = false;
        }
        
        public string AboutHelp()
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
                bot.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " does not exist");
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
                    bot.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " has no posts.  (*** Group Filtered ***)");
                }
                else {
                    bot.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " : " + posts + " Posts, Last Post " + Math.Round(t.TotalHours) + " hours ago by " + LastArticle.Header.From.Split(new char[] { '@' })[0] + " (*** Group Filtered ***)");
                }
            }
            else {
                if (LastArticle == null) {
                    bot.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " has no posts.");
                }
                else {
                    bot.SendMessage(SendType.Message, e.Data.Channel, "Group " + e.Data.MessageArray[1] + " : " + posts + " Posts, Last Post " + Math.Round(t.TotalHours) + " hours ago by " + LastArticle.Header.From.Split(new char[] { '@' })[0] + " (" + LastArticle.Header.Subject + ")");
                }
            }
        }


        private void ListGroups(IrcEventArgs e)
        {
            foreach (string s in bot.ListToLines(nntpCache.Keys, 350)) {
                bot.SendMessage(SendType.Message, e.Data.Channel, s);
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
                bot.SendMessage(SendType.Message, e.Data.Channel, "Poster " + e.Data.MessageArray[1] + " has posted " + posts + " times, in " + groups.Count + " groups and wrote " + lines + " lines of text. Last post: " + IrcConstants.IrcBold + LastArticle.Header.Subject + IrcConstants.IrcBold + " in Board " + LastGroup + " (" + Math.Round(t.TotalHours) + " hours ago)");
            }
            else {
                bot.SendMessage(SendType.Message, e.Data.Channel, "Poster " + e.Data.MessageArray[1] + " not found.");
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
            bot.SendMessage(SendType.Message, e.Data.Channel, "In the last 24hours there have been (at least) " + posts + " posts in " + groups.Count + " groups by " + posters.Count + " posters in FMS.");
        }
    }
}