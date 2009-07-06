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
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
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
        
        private AccesTwitter twitter = new AccesTwitter("http://twitter.com/", "http://twitter.com/");
        private AccesTwitter identica = new AccesTwitter("http://identi.ca/api/", "http://identi.ca/");
        
        private Dictionary<long, TwitterMention> allMention = new Dictionary<long, TwitterMention>();

        public TwitterPlugin(IrcBot botInstance) : base(botInstance) {

        }
        
        private NickServIdentify nickserv;
        
        public override void Init()
        {
            twitter.User = PersistentMemory.Instance.GetValueOrTodo("twitter_user");
            identica.User = PersistentMemory.Instance.GetValueOrTodo("identica_user");
            
            twitter.Pass = PersistentMemory.Instance.GetValueOrTodo("twitter_pass");
            identica.Pass = PersistentMemory.Instance.GetValueOrTodo("identica_pass");
            
            checkInterval = new Timer();
            checkInterval.Interval = 1 * 60 * 1000; // 1 minute
            checkInterval.Elapsed += new ElapsedEventHandler(CheckInterval_Elapsed);
            base.Init();
            
            
            foreach(IdentifyUser idu in BotMethods.Acl.IdentifyPlugins) {
                if (idu is NickServIdentify) {
                    nickserv = (NickServIdentify)idu;
                }
            }
            
            BotEvents.OnRawMessage += new IrcEventHandler(BotEvents_OnRawMessage);
        }

        private void BotEvents_OnRawMessage(object sender, IrcEventArgs e)
        {
            if(e.Data.ReplyCode == ReplyCode.ErrorNoSuchNickname) {
                PersistentMemory.Instance.RemoveValue("mention_subscriber", e.Data.RawMessageArray[3]);
                PersistentMemory.Instance.Flush();
            }
        }

        private void CheckInterval_Elapsed(object sender, ElapsedEventArgs e)
        {
            IEnumerable<TwitterMention> mentions;
            long newid;
            //twitter
            mentions = twitter.GetNewMentions();
            newid = SendMentions(mentions, PersistentMemory.Instance.GetValue("twitter_maxid"));
            PersistentMemory.Instance.ReplaceValue("twitter_maxid", newid.ToString());
            
            //identi.ca
            mentions = identica.GetNewMentions();
            newid = SendMentions(mentions, PersistentMemory.Instance.GetValue("identica_maxid"));
            PersistentMemory.Instance.ReplaceValue("identica_maxid", newid.ToString());
            
            PersistentMemory.Instance.Flush();
        }

        private long SendMentions(IEnumerable<TwitterMention> mentions, string maxid)
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
                    foreach (string sendto in PersistentMemory.Instance.GetValues("mention_subscriber")) {
                        string nick;
                        if (sendto.StartsWith("ns/")) {
                            nick = nickserv.IdToNick(sendto);
                            if (nick != null) {
                                BotMethods.SendMessage(SendType.Message, nick, mention.Text + " (by " + mention.User.Nick + " [" + mention.User.Name + "|" + mention.User.Statuses + "|" + mention.User.Followers + "|" + mention.User.Friends + "] posted " + Ago(DateTime.Now - mention.Created) + " on " + mention.Feed + ") !reply " + mention.Id + ")");
                            }
                        }
                        else {
                            BotMethods.SendMessage(SendType.Message, sendto, mention.Text + " (by " + mention.User.Nick + " [" + mention.User.Name + "|" + mention.User.Statuses + "|" + mention.User.Followers + "|" + mention.User.Friends + "] posted " + Ago(DateTime.Now - mention.Created) + " on " + mention.Feed + ") !reply " + mention.Id + ")");
                        }
                    }
                    nid = Math.Max(mention.Id, nid);
                }
                
            }
            return nid;
        }
        
        public override string Name {
            get {
                return Assembly.GetExecutingAssembly().FullName;
            }
        }
        
        public override void Activate() {
            BotMethods.AddCommand(new Commandlet("!tweet", "The command !tweet <your text>, will post a twitter message to the channel feed", twitterHandler, this, CommandScope.Public, "twitter_access"));
            BotMethods.AddCommand(new Commandlet("!rt", "The command !rt <your text>, will post a twitter message to the channel feed", twitterHandler, this, CommandScope.Public, "twitter_access"));
            BotMethods.AddCommand(new Commandlet("!reply", "The command !reply <twitter_message_id> <your text>, will post a twitter message to the channel feed as a reply to a twitter message", twitterHandler, this, CommandScope.Public, "twitter_access"));
            BotMethods.AddCommand(new Commandlet("!mentions", "The command !mentions will inform you when the pirates are mentioned on twitter or identi.ca. ", mentionAboHandler, this, CommandScope.Both));
            BotMethods.AddCommand(new Commandlet("!endmentions", "Will inform you about ", mentionAboHandler, this, CommandScope.Both));
            BotMethods.AddCommand(new Commandlet("!trends", "The command !trends Will inform you about the newest emerging trends on twitter.", trendAboHandler, this, CommandScope.Both));
            BotMethods.AddCommand(new Commandlet("!endtrends", ". ", trendAboHandler, this, CommandScope.Both));
            
            checkInterval.Enabled = true;
            base.Activate();
        }

        
        public override void Deactivate() {
            BotMethods.RemoveCommand("!twitter");
            BotMethods.RemoveCommand("!mentions");
            BotMethods.RemoveCommand("!endmentions");
            BotMethods.RemoveCommand("!trends");
            BotMethods.RemoveCommand("!endtrends");

            checkInterval.Enabled = false;
            base.Deactivate();
        }
        
        public override string AboutHelp() {
            return "The Twitter Plugin will post a twitter message to the channel feed, try !help twitter.";
        }
        
        private void mentionAboHandler(object sender, IrcEventArgs e) {
            string id;
            
            // Stop Abo
            if (e.Data.MessageArray[0] == "!endmentions") {
                PersistentMemory.Instance.RemoveValue("mention_subscriber", e.Data.Nick);
                if ((id = nickserv.Identified(e.Data.Nick)) != null) {
                    PersistentMemory.Instance.RemoveValue("mention_subscriber", id);
                }
                PersistentMemory.Instance.Flush();
                return;
            }
            
            // Start Abo
            if ((id = nickserv.Identified(e.Data.Nick)) != null) {
                PersistentMemory.Instance.SetValue("mention_subscriber", id);
                BotMethods.SendMessage(SendType.Notice, e.Data.Nick, "You subscribed a mention abo permanently with your identified id: '" + id + "'. Join the channel identified to use your abo after a leave. (use !endmentions to stop)");
            } else {
                PersistentMemory.Instance.SetValue("mention_subscriber", e.Data.Nick);
                BotMethods.SendMessage(SendType.Notice, e.Data.Nick, "You subscribed a mention abo temporarily with your nick: '" + e.Data.Nick + "' it will stop as soon as you leave, change your nick or use !endmentions.");
            }
            PersistentMemory.Instance.Flush();
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

        private void trendAboHandler(object sender, IrcEventArgs e) {
            
        }

        private void twitterHandler(object sender, IrcEventArgs e) {
            try {
                if(e.Data.MessageArray[0].ToLower()=="!reply") {
                    long id = long.Parse(e.Data.MessageArray[1]);
                    // fucking bad heuristic
                    if (id > 1000000000) {
                        twitter.StatusUpdate(e.Data.Message.Substring(8 + e.Data.MessageArray[1].Length), id);
                    } else {
                        identica.StatusUpdate(e.Data.Message.Substring(8 + e.Data.MessageArray[1].Length), id);
                    }
                } else if (e.Data.MessageArray[0].ToLower()=="!rt"){
                    identica.StatusUpdate("RT " + e.Data.Message.Substring(4));
                    //twitter.StatusUpdate("RT " + e.Data.Message.Substring(4));
                } else {
                    identica.StatusUpdate(e.Data.Message.Substring(9));
                    //twitter.StatusUpdate(e.Data.Message.Substring(9));
                }
                
                BotMethods.SendMessage(SendType.Notice, e.Data.Nick, "twittered your message!" );
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, "New message on twitter-feed: " + identica.FeedUrl + " and " + twitter.FeedUrl);
            } catch (Exception ex) {
                Console.WriteLine();
                BotMethods.SendMessage(SendType.Notice, e.Data.Nick, "Twitter Error: " + ex.Message);
            }
        }
    }
}
