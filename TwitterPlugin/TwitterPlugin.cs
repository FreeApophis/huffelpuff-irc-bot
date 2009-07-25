/*
 *  This is an example Plugin, you can use it as a base for your own plugins.
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Web;
using System.Xml;

using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// This is a very simple Plugin Example: The Echo Plugin
    /// </summary>
    public class TwitterPlugin : AbstractPlugin
    {
        private Timer checkInterval;
        
        private AccesTwitter twitter_fr = new AccesTwitter("http://twitter.com/", "http://twitter.com/");
        private AccesTwitter identica_fr = new AccesTwitter("http://identi.ca/api/", "http://identi.ca/");
        private AccesTwitter twitter_de = new AccesTwitter("http://twitter.com/", "http://twitter.com/");
        private AccesTwitter identica_de = new AccesTwitter("http://identi.ca/api/", "http://identi.ca/");
        private AccesTwitter twitter_it = new AccesTwitter("http://twitter.com/", "http://twitter.com/");
        private AccesTwitter identica_it = new AccesTwitter("http://identi.ca/api/", "http://identi.ca/");
        private AccesTwitter twitter_all = new AccesTwitter("http://twitter.com/", "http://twitter.com/");
        private AccesTwitter identica_all = new AccesTwitter("http://identi.ca/api/", "http://identi.ca/");
        
        private AccesTwitter twitter_old = new AccesTwitter("http://twitter.com/", "http://twitter.com/");
        private AccesTwitter identica_old = new AccesTwitter("http://identi.ca/api/", "http://identi.ca/");

        
        private Dictionary<long, TwitterMention> allMention = new Dictionary<long, TwitterMention>();

        public TwitterPlugin(IrcBot botInstance) : base(botInstance) {}
        
        public override void Init()
        {
            twitter_fr.User = PersistentMemory.Instance.GetValueOrTodo("twitter_user_fr");
            identica_fr.User = PersistentMemory.Instance.GetValueOrTodo("identica_user_fr");
            twitter_fr.Pass = PersistentMemory.Instance.GetValueOrTodo("twitter_pass_fr");
            identica_fr.Pass = PersistentMemory.Instance.GetValueOrTodo("identica_pass_fr");
            
            twitter_de.User = PersistentMemory.Instance.GetValueOrTodo("twitter_user_de");
            identica_de.User = PersistentMemory.Instance.GetValueOrTodo("identica_user_de");
            twitter_de.Pass = PersistentMemory.Instance.GetValueOrTodo("twitter_pass_de");
            identica_de.Pass = PersistentMemory.Instance.GetValueOrTodo("identica_pass_de");

            twitter_it.User = PersistentMemory.Instance.GetValueOrTodo("twitter_user_it");
            identica_it.User = PersistentMemory.Instance.GetValueOrTodo("identica_user_it");
            twitter_it.Pass = PersistentMemory.Instance.GetValueOrTodo("twitter_pass_it");
            identica_it.Pass = PersistentMemory.Instance.GetValueOrTodo("identica_pass_it");

            twitter_all.User = PersistentMemory.Instance.GetValueOrTodo("twitter_user_all");
            identica_all.User = PersistentMemory.Instance.GetValueOrTodo("identica_user_all");
            twitter_all.Pass = PersistentMemory.Instance.GetValueOrTodo("twitter_pass_all");
            identica_all.Pass = PersistentMemory.Instance.GetValueOrTodo("identica_pass_all");

            twitter_old.User = "piratenparteich";
            twitter_old.Pass = "9y2FU9FFIyljGxGgSvnjZQ8YYpEoNic3";
            identica_old.User = "piratenparteischweiz";
            identica_old.Pass = "Tqk7EhvfLGbz3i09abEwFO6aFL3BbGrz";
            
            checkInterval = new Timer();
            checkInterval.Interval = 1 * 60 * 1000; // 1 minute
            checkInterval.Elapsed += new ElapsedEventHandler(CheckInterval_Elapsed);
            
            BotMethods.SendMessage(SendType.Action, "#pps-twitter", " is in a new version: " + this.GetType().Assembly.FullName);
            base.Init();
        }

        private AccesTwitter GetTwitterByLang(string lang) {
            switch(lang) {
                    case "fr": return twitter_fr;
                    case "de": return twitter_de;
                    case "it": return twitter_it;
                    default: return null;
            }
        }
        
        private AccesTwitter GetIdenticaByLang(string lang) {
            switch(lang) {
                    case "fr": return identica_fr;
                    case "de": return identica_de;
                    case "it": return identica_it;
                    default: return null;
            }
        }
        
        private Regex whiteSpaceMatch = new Regex(@"\s+");

        private string SafeString(string str)  {
            return whiteSpaceMatch.Replace(str, " ");
        }
        
        private void CheckInterval_Elapsed(object sender, ElapsedEventArgs e)
        {
            IEnumerable<TwitterMention> mentions;
            long newid;
            
            //twitter fr
            mentions = twitter_fr.GetNewMentions();
            newid = SendMentions(mentions, PersistentMemory.Instance.GetValue("twitter_maxid_fr"), "fr");
            PersistentMemory.Instance.ReplaceValue("twitter_maxid_fr", newid.ToString());
            
            //identi.ca fr
            mentions = identica_fr.GetNewMentions();
            newid = SendMentions(mentions, PersistentMemory.Instance.GetValue("identica_maxid_fr"), "fr");
            PersistentMemory.Instance.ReplaceValue("identica_maxid_fr", newid.ToString());
            
            //twitter de
            mentions = twitter_de.GetNewMentions();
            newid = SendMentions(mentions, PersistentMemory.Instance.GetValue("twitter_maxid_de"), "de");
            PersistentMemory.Instance.ReplaceValue("twitter_maxid_de", newid.ToString());
            
            //identi.ca de
            mentions = identica_de.GetNewMentions();
            newid = SendMentions(mentions, PersistentMemory.Instance.GetValue("identica_maxid_de"), "de");
            PersistentMemory.Instance.ReplaceValue("identica_maxid_de", newid.ToString());
            
            //search tags
            foreach(string tag in PersistentMemory.Instance.GetValues("twitter_search_tag")) {
                string maxid = PersistentMemory.Instance.GetValue("maxid_tag_" + tag);
                long mid = 0, nid = 0;
                Console.WriteLine(tag);
                if(!string.IsNullOrEmpty(maxid)) {
                    mid =  long.Parse(maxid);
                }
                
                foreach(TwitterSearch search in twitter_de.GetNewSearch(tag)) {
                    nid = Math.Max(search.Id, nid);
                    if (maxid == null) {
                        continue;
                    }
                    if (search.Id > mid) {
                        BotMethods.SendMessage(SendType.Message, "#pps-twitter", "[" + IrcConstants.IrcBold + IrcConstants.IrcColor + "" + (int)IrcColors.LightRed + search.Tag + IrcConstants.IrcNormal + "] " + SafeString(search.Text) + " [ " + search.Author +" @ " + MessageTime(search.Created) + " " + search.Feed + " ]");
                    }
                }
                PersistentMemory.Instance.ReplaceValue("maxid_tag_" + tag, nid.ToString());
            }
        }
        
        private long SendMentions(IEnumerable<TwitterMention> mentions, string maxid, string lang)
        {
            long mid, nid = 0;
            if(string.IsNullOrEmpty(maxid)) {
                mid = 0;
            } else {
                mid = long.Parse(maxid);
                nid = mid;
            }
            
            foreach (TwitterMention mention in mentions) {
                if (!allMention.ContainsKey(mention.Id)) {
                    allMention.Add(mention.Id, mention);
                }
                if (mention.Id > mid) {

                    BotMethods.SendMessage(SendType.Message, "#pps-twitter", "[" + IrcConstants.IrcBold+ IrcConstants.IrcColor + "" + (int)IrcColors.LightRed + "mention" + IrcConstants.IrcNormal + "] " + Colorize(HttpUtility.HtmlDecode(SafeString(mention.Text))) + " (by " + mention.Feed + mention.User.Nick + " [" + IrcConstants.IrcColor + "" + (int)IrcColors.Brown+ mention.User.Name + IrcConstants.IrcColor + "|" + mention.User.Statuses + "|" + mention.User.Followers + "|" + mention.User.Friends + "] posted " +MessageTime(mention.Created) + ") !reply-" + lang + " " + mention.Id + " @" + mention.User.Nick + ")");
                    nid = Math.Max(nid, mention.Id);
                }
                
            }
            return nid;
        }
        
        List<string> tocolorize = new List<string>();
        
        private string Colorize(string text) {
            foreach(string word in tocolorize) {
                text = text.Replace(word, "" + IrcConstants.IrcBold+ IrcConstants.IrcColor + "" + (int)IrcColors.Blue + word + IrcConstants.IrcNormal + IrcConstants.IrcColor);
            }
            return text;
        }
        
        public override string Name {
            get {
                return Assembly.GetExecutingAssembly().FullName;
            }
        }
        
        public override void Activate() {
            BotMethods.AddCommand(new Commandlet("!tweet-fr", "The command !tweet <your text>, will post a twitter message to the channel feed", twitterHandler, this, CommandScope.Public, "twitter_access"));
            BotMethods.AddCommand(new Commandlet("!tweet-de", "The command !tweet <your text>, will post a twitter message to the channel feed", twitterHandler, this, CommandScope.Public, "twitter_access"));
            BotMethods.AddCommand(new Commandlet("!reply-fr", "The command !reply <twitter_message_id> <your text>, will post a twitter message to the channel feed as a reply to a twitter message", twitterHandler, this, CommandScope.Public, "twitter_access"));
            BotMethods.AddCommand(new Commandlet("!reply-de", "The command !reply <twitter_message_id> <your text>, will post a twitter message to the channel feed as a reply to a twitter message", twitterHandler, this, CommandScope.Public, "twitter_access"));
            BotMethods.AddCommand(new Commandlet("!+tag", "", tagHandler, this, CommandScope.Public, "twitter_access"));
            BotMethods.AddCommand(new Commandlet("!-tag", "", tagHandler, this, CommandScope.Public, "twitter_access"));
            BotMethods.AddCommand(new Commandlet("!tags", "", tagHandler, this, CommandScope.Public));
            BotMethods.AddCommand(new Commandlet("!tweetstats", "", tweetStatsHandler, this, CommandScope.Public));
            
            checkInterval.Enabled = true;
            base.Activate();
        }

        
        public override void Deactivate() {
            BotMethods.RemoveCommand("!tweet-fr");
            BotMethods.RemoveCommand("!tweet-de");
            BotMethods.RemoveCommand("!reply-fr");
            BotMethods.RemoveCommand("!reply-de");
            BotMethods.RemoveCommand("!+tag");
            BotMethods.RemoveCommand("!-tag");
            BotMethods.RemoveCommand("!tags");

            checkInterval.Enabled = false;
            base.Deactivate();
        }
        
        public override string AboutHelp() {
            return "The Twitter Plugin will post a twitter message to the channel feed, try !help twitter.";
        }

        private  string MessageTime(DateTime time) {
            return time.ToString("HH:mm K", new CultureInfo("DE-ch", true));
        }
        
        private string Ago(TimeSpan ago) {
            if (ago.Days > 0) {
                return ago.Days + ((ago.Days==1)?" day":" days") + " ago";
            } else if (ago.Hours > 0) {
                return ago.Hours + ((ago.Days==1)?" hour":" hours") + " ago";
            } else if (ago.Minutes > 0) {
                return ago.Minutes + ((ago.Days==1)?" minute":" minutes") + " ago";
            } else {
                return ago.Seconds + ((ago.Days==1)?" second":" seconds") + " ago";
            }
        }

        
        private void tagHandler(object sender, IrcEventArgs e) {
            if (e.Data.MessageArray[0].ToLower() == "!+tag") {
                PersistentMemory.Instance.SetValue("twitter_search_tag", e.Data.MessageArray[1]);
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Automatic Search activated: " + "http://search.twitter.com/search?q=" + HttpUtility.UrlEncode(e.Data.MessageArray[1]) + " or " + "http://search.twitter.com/search?q=" + HttpUtility.UrlEncode(e.Data.MessageArray[1]));
            }
            if (e.Data.MessageArray[0].ToLower() == "!-tag") {
                PersistentMemory.Instance.RemoveValue("twitter_search_tag", e.Data.MessageArray[1]);
                
            }
            if (e.Data.MessageArray[0].ToLower() == "!tags") {
                foreach(string line in BotMethods.ListToLines(PersistentMemory.Instance.GetValues("twitter_search_tag"), 350)) {
                    BotMethods.SendMessage(SendType.Message, e.Data.Channel, line);
                }
            }
        }
        
        private void tweetStatsHandler(object sender, IrcEventArgs e) {
            BotMethods.SendMessage(SendType.Message, e.Data.Channel, "http://twitter.com/users/show/ppsde.xml http://twitter.com/users/show/ppsfr.xml");
        }
        
        private void twitterHandler(object sender, IrcEventArgs e) {
            
            string lang = e.Data.MessageArray[0].Substring(e.Data.MessageArray[0].Length-2,2);
            AccesTwitter identica = GetIdenticaByLang(lang);
            AccesTwitter twitter = GetTwitterByLang(lang);
            
            try {
                if(e.Data.MessageArray[0].ToLower().StartsWith("!reply")) {
                    long id = long.Parse(e.Data.MessageArray[1]);
                    // fucking bad heuristic
                    if (id > 1000000000) {
                        twitter.StatusUpdate(e.Data.Message.Substring(11 + e.Data.MessageArray[1].Length), id);
                    } else {
                        identica.StatusUpdate(e.Data.Message.Substring(11 + e.Data.MessageArray[1].Length), id);
                    }
                } else {
                    identica.StatusUpdate(e.Data.Message.Substring(10));
                    //twitter.StatusUpdate(e.Data.Message.Substring(9));
                }
                
                BotMethods.SendMessage(SendType.Notice, e.Data.Nick, "twittered your message!" );
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, "New message on twitter-feed: " + identica.FeedUrl + " and " + twitter.FeedUrl);
            } catch (Exception ex) {
                Console.WriteLine();
                BotMethods.SendMessage(SendType.Notice, e.Data.Nick, "Twitter Error: " + ex.Message);
            }
        }
        
        private void mentionsHandler(object sender, IrcEventArgs e) {
            IEnumerable<TwitterMention> query = allMention.Values.OrderByDescending(t => t.Created).Take(10);
            
        }
    }
}
