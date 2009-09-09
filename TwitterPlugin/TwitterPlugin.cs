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

using Dimebrain.TweetSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Timers;
using System.Web;
using Dimebrain.TweetSharp.Extensions;
using Dimebrain.TweetSharp.Fluent;
using Dimebrain.TweetSharp.Model;
using Huffelpuff;
using Huffelpuff.Plugins;
using Huffelpuff.Tools;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// This is a very simple Plugin Example: The Echo Plugin
    /// </summary>
    public class TwitterPlugin : AbstractPlugin
    {
        private Timer checkInterval;
        
        public TwitterPlugin(IrcBot botInstance) : base(botInstance) {}
        
        private Dictionary<string, TwitterWrapper> twitterAccounts = new Dictionary<string, TwitterWrapper>();
        
        public const string twitteraccountconst = "twitter_account";
        public static TwitterClientInfo ClientInfo = new TwitterClientInfo() {
            ClientName = "Huffelpuff IRC Bot - Twitter Plugin",
            ClientUrl = "http://huffelpuff-irc-bot.origo.ethz.ch/",
            ClientVersion = "1.0",
        };
        
        
        public override void Init()
        {

            foreach(string account in PersistentMemory.Instance.GetValues(twitteraccountconst))
            {
                TwitterWrapper twitterInfo = new TwitterWrapper(account);
                if (twitterInfo.FriendlyName != PersistentMemory.todoValue) {
                    twitterAccounts.Add(twitterInfo.FriendlyName, twitterInfo);
                }
            }
            
            checkInterval = new Timer(90000);
            
            checkInterval.Elapsed += checkInterval_Elapsed;
            
            base.Init();
            
        }

        void checkInterval_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach(var twitteraccount in twitterAccounts) {
                twitteraccount.Value.GetMentions();
            }
        }

        private Regex whiteSpaceMatch = new Regex(@"\s+");

        private string SafeString(string str)  {
            return whiteSpaceMatch.Replace(str, " ");
        }
        
        List<KeyValuePair<string, int>> tocolorize = new List<KeyValuePair<string, int>>();
        
        private string Colorize(string text) {
            foreach(KeyValuePair<string, int> wordAndColor in tocolorize) {
                text = text.Replace(wordAndColor.Key, "" + IrcConstants.IrcBold + IrcConstants.IrcColor + "" + wordAndColor.Value + wordAndColor.Key + IrcConstants.IrcNormal + IrcConstants.IrcColor);
            }
            return text;
        }
        
        public override string Name {
            get {
                return Assembly.GetExecutingAssembly().FullName;
            }
        }
        
        public override void Activate() {
            BotMethods.AddCommand(new Commandlet("!tweet", "The !tweet <account> <text> command tweets a message to the account. If there is only 1 account, the account name can be omitted.", tweetHandler, this, CommandScope.Public, "twitter_access"));
            BotMethods.AddCommand(new Commandlet("!+tweet", "With the command !+tweet <friendlyname> [<username> <password>] you can add a tweets.", adminTweet, this, CommandScope.Private, "twitter_admin"));
            BotMethods.AddCommand(new Commandlet("!-tweet", "With the command !-tweet <friendlyname> you can remove a tweets.", adminTweet, this, CommandScope.Private, "twitter_admin"));
            BotMethods.AddCommand(new Commandlet("!+tag", "With the command !+tag you can add a search tag.", tagHandler, this, CommandScope.Both, "twitter_admin"));
            BotMethods.AddCommand(new Commandlet("!-tag", "With the command !-tag you can remove a search tag.", tagHandler, this, CommandScope.Both, "twitter_admin"));

            base.Activate();
        }

        
        public override void Deactivate() {
            BotMethods.RemoveCommand("!tweet");
            BotMethods.RemoveCommand("!+tweet");
            BotMethods.RemoveCommand("!-tweet");
            BotMethods.RemoveCommand("!+tag");
            BotMethods.RemoveCommand("!-tag");
            
            base.Deactivate();
        }
        
        public override string AboutHelp() {
            return "The Twitter Plugin will post a twitter message to the channel feed, try !help tweet.";
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
                foreach(string line in PersistentMemory.Instance.GetValues("twitter_search_tag").ToLines(350)) {
                    BotMethods.SendMessage(SendType.Message, e.Data.Channel, line);
                }
            }
        }
        
        private void tweetStatsHandler(object sender, IrcEventArgs e) {
            BotMethods.SendMessage(SendType.Message, e.Data.Channel, "http://twitter.com/users/show/ppsde.xml http://twitter.com/users/show/ppsfr.xml");
        }
        
        private void tweetHandler(object sender, IrcEventArgs e) {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick  : e.Data.Channel;
            if (e.Data.MessageArray.Length < 2) {
                foreach(string lines in twitterAccounts.Select(item => item.Value.FriendlyName).ToLines(350, ", ", "Tweet accounts loaded: ", ".")) {
                    BotMethods.SendMessage(SendType.Message, sendto, lines);
                }
                return;
            }
            if (e.Data.MessageArray.Length < 3) {
                BotMethods.SendMessage(SendType.Message, sendto, "Nothing to say? I don't tweet empty messages! try !help !tweet.");
                return;
            }
            if (twitterAccounts.ContainsKey(e.Data.MessageArray[1].ToLower())) {
                string status = e.Data.Message.Substring(e.Data.MessageArray[0].Length + e.Data.MessageArray[1].Length + 2);
                
                try {
                    string returnFromTwitter = twitterAccounts[e.Data.MessageArray[1].ToLower()].SendStatus(status);
                } catch (Exception) {}
            } else {
                BotMethods.SendMessage(SendType.Message, sendto, "I dont know a tweet with the name: {0}.".Fill(e.Data.MessageArray[1].ToLower()));
                return;
            }
        }
        
        
        private void adminTweet(object sender, IrcEventArgs e) {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick  : e.Data.Channel;
            switch(e.Data.MessageArray[0].ToLower()) {
                case "!+tweet":
                    if (e.Data.MessageArray.Length < 4) {
                        BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments for 'add'! Try '!help rss-admin'.");
                        return;
                    }
                    if (twitterAccounts.ContainsKey(e.Data.MessageArray[1].ToLower())) {
                        BotMethods.SendMessage(SendType.Message, sendto, "Tweet '{0}' already exists.".Fill(twitterAccounts[e.Data.MessageArray[1].ToLower()].FriendlyName));
                        break;
                    }
                    PersistentMemory.Instance.SetValue(twitteraccountconst, e.Data.MessageArray[1].ToLower());
                    twitterAccounts.Add(e.Data.MessageArray[1].ToLower(), new TwitterWrapper(e.Data.MessageArray[1].ToLower(), e.Data.MessageArray[1], e.Data.MessageArray[2], e.Data.MessageArray[3]));
                    BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' successfully added.".Fill(twitterAccounts[e.Data.MessageArray[1].ToLower()].FriendlyName));
                    break;
                case "!-tweet":
                    if (e.Data.MessageArray.Length < 2) {
                        BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments! Try '!help rss-admin'.");
                        return;
                    }
                    if (twitterAccounts.ContainsKey(e.Data.MessageArray[1].ToLower())) {
                        twitterAccounts[e.Data.MessageArray[1].ToLower()].RemoveAccount();
                        twitterAccounts.Remove(e.Data.MessageArray[1].ToLower());
                        BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' successfully removed.".Fill(e.Data.MessageArray[1].ToLower()));
                    } else {
                        BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' does not exists! Try '!rss'.".Fill(e.Data.MessageArray[1].ToLower()));
                    }
                    break;
                default:
                    break;
            }
        }
        
        
        private void mentionsHandler(object sender, IrcEventArgs e) {
        }
    }
}
