/*
 *  UT3GlobalStatsPlugin, Access to the GameSpy Stats for UT3
 * 
 *  Copyright (c) 2007-2009 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
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
using System.Xml;
using System.Linq;
using System.Timers;
using System.Collections.Generic;

using Meebey.SmartIrc4net;

using Huffelpuff;
using Huffelpuff.Plugins;
using Huffelpuff.Tools;


namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class RssPlugin : AbstractPlugin
    {
        public RssPlugin(IrcBot botInstance) :
            base(botInstance) {}
        
        private Dictionary<string, RssWrapper> rssFeeds = new Dictionary<string, RssWrapper>();
        private Timer checkInterval;
        private DateTime lastpost = DateTime.MinValue;
        
        public const string rssfeedconst = "rss_feed";
        public override void Init()
        {
            checkInterval = new Timer();
            checkInterval.Elapsed += checkInterval_Elapsed;
            checkInterval.Interval = 1 * 90 * 1000; // 90 seconds
            foreach(string rss in PersistentMemory.Instance.GetValues(rssfeedconst))
            {
                RssWrapper rssInfo = new RssWrapper(rss);
                if (rssInfo.FriendlyName != PersistentMemory.todoValue) {
                    rssFeeds.Add(rssInfo.FriendlyName, rssInfo);
                }
            }
            base.Init();
        }

        void checkInterval_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!BotMethods.IsConnected)
                return;
            foreach(var rssFeed in rssFeeds.Values) {
                var newItems = rssFeed.NewItems();
                foreach(var newItem in newItems) {
                    foreach(string channel in PersistentMemory.Instance.GetValues(IrcBot.channelconst)) {
                        BotMethods.SendMessage(SendType.Message, channel, ("" + IrcConstants.IrcBold + "New Message" + IrcConstants.IrcBold + " on RSS Feed " + IrcConstants.IrcBold + "'" + IrcConstants.IrcColor + (int)IrcColors.Blue + "{0}" + IrcConstants.IrcColor + "'" + IrcConstants.IrcBold + ": " + IrcConstants.IrcColor + (int)IrcColors.Brown + "{1}" + IrcConstants.IrcColor + " (by " + IrcConstants.IrcBold + "{2}" + IrcConstants.IrcBold + " on " + IrcConstants.IrcColor + (int)IrcColors.Blue + "{3}" + IrcConstants.IrcColor + " go: {4})").Fill(rssFeed.FriendlyName, newItem.Title, newItem.Author, newItem.Published.ToString(), newItem.Link));
                    }
                }
            }
            PersistentMemory.Instance.Flush();
        }
        
        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!rss", "With the command !rss [<feed> [<#post>]] post you'll get a list of the bots configured RSS feed, stats and posts.", showRss, this, CommandScope.Both));
            BotMethods.AddCommand(new Commandlet("!rss-admin", "With the command !rss-admin [add|remove] <friendlyname> [<url>] you can modify your rss feeds.", adminRss, this, CommandScope.Both, "rss_admin"));
            
            checkInterval.Enabled = true;
            
            base.Activate();
        }
        
        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!rss");
            BotMethods.RemoveCommand("!rss-admin");
            
            checkInterval.Enabled = false;
            
            base.Deactivate();
        }
        
        public override string AboutHelp()
        {
            return "The Rss Plugins reports new posts on the configured RSS feed to the channel, and it provides access to the complete rss via the !rss command";
        }
        
        private void showRss(object sender, IrcEventArgs e) {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick  : e.Data.Channel;
            if (e.Data.MessageArray.Length < 2) {
                foreach(string lines in rssFeeds.Select(item => item.Value.FriendlyName).ToLines(350, ", ", "Currently checked feeds: ", ".")) {
                    BotMethods.SendMessage(SendType.Message, sendto, lines);
                }
                return;
            }
            if (e.Data.MessageArray.Length < 3) {
                if (rssFeeds.ContainsKey(e.Data.MessageArray[1].ToLower())) {
                    var rssFeed = rssFeeds[e.Data.MessageArray[1].ToLower()];
                    BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' has {1} Elements, last post was on: {2}.".Fill(rssFeed.FriendlyName, rssFeed.Count, rssFeed.Last));
                } else {
                    BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' does not exists! Try '!rss'.".Fill(e.Data.MessageArray[1].ToLower()));
                }
                return;
            }
        }

        private void adminRss(object sender, IrcEventArgs e) {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick  : e.Data.Channel;
            if (e.Data.MessageArray.Length < 3) {
                BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments! Try '!help rss-admin'.");
                return;
            }
            switch(e.Data.MessageArray[1].ToLower()) {
                case "add":
                    if (e.Data.MessageArray.Length < 4) {
                        BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments for 'add'! Try '!help rss-admin'.");
                        return;
                    }
                    if (rssFeeds.ContainsKey(e.Data.MessageArray[2].ToLower())) {
                        BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' already exists.".Fill(rssFeeds[e.Data.MessageArray[2].ToLower()].FriendlyName));
                        break;
                    }
                    PersistentMemory.Instance.SetValue(rssfeedconst, e.Data.MessageArray[2].ToLower());
                    rssFeeds.Add(e.Data.MessageArray[2].ToLower(), new RssWrapper(e.Data.MessageArray[2].ToLower(), e.Data.MessageArray[2], e.Data.MessageArray[3]));
                    BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' successfully added.".Fill(rssFeeds[e.Data.MessageArray[2].ToLower()].FriendlyName));
                    break;
                case "remove":
                    if (rssFeeds.ContainsKey(e.Data.MessageArray[2].ToLower())) {
                        rssFeeds[e.Data.MessageArray[2].ToLower()].RemoveFeed();
                        rssFeeds.Remove(e.Data.MessageArray[2].ToLower());
                        BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' successfully removed.".Fill(e.Data.MessageArray[2].ToLower()));
                    } else {
                        BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' does not exists! Try '!rss'.".Fill(e.Data.MessageArray[2].ToLower()));
                    }
                    break;
                default:
                    BotMethods.SendMessage(SendType.Message, sendto, "Unknown argument '{0}' ! Try '!help rss-admin'.".Fill(e.Data.MessageArray[1].ToLower()));
                    break;
            }
        }
    }
}