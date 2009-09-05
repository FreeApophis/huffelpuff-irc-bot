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
using Dimebrain.TweetSharp.Extensions;
using Dimebrain.TweetSharp.Fluent;
using Dimebrain.TweetSharp.Model;
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
        
        public TwitterPlugin(IrcBot botInstance) : base(botInstance) {}
        
        private Dictionary<string, TwitterWrapper> twitterAccounts = new Dictionary<string, TwitterWrapper>();
        
        public static TwitterClientInfo ClientInfo = new TwitterClientInfo() {
            ClientName = "Huffelpuff IRC Bot - Twitter Plugin",
            ClientUrl = "http://huffelpuff-irc-bot.origo.ethz.ch/",
            ClientVersion = "1.0",
        };
        
        
        public override void Init()
        {

            foreach(string account in PersistentMemory.Instance.GetValues("twitter_account"))
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
            BotMethods.AddCommand(new Commandlet("!tweet-admin", "The !tweet-add [remove|add]", tweetHandler, this, CommandScope.Private, "twitter_admin"));

            base.Activate();
        }

        
        public override void Deactivate() {
            BotMethods.RemoveCommand("!tweet");
            BotMethods.RemoveCommand("!tweet-admin");
            
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
        
        private void tweetHandler(object sender, IrcEventArgs e) {
        }
        
        private void mentionsHandler(object sender, IrcEventArgs e) {
        }
    }
}
