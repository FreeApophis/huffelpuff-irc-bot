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
        private bool firstrun = true;
        private DateTime lastpost = DateTime.MinValue;
        
        
        public override void Init()
        {
            checkInterval = new Timer();
            checkInterval.Elapsed += checkInterval_Elapsed;
            checkInterval.Interval = 1 * 60 * 1000; // 1 minute
            foreach(string rss in PersistentMemory.Instance.GetValues("rss_feed"))
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
        }
        
        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!rss", "With the command !rss you'll get a list of the bots configured RSS feed.", showRss, this, CommandScope.Both));
            BotMethods.AddCommand(new Commandlet("!rss-admin", "With the command !rss you'll get a list of the bots configured RSS feed.", adminRss, this, CommandScope.Both, "rss_admin"));
            checkInterval.Enabled = true;
            
            base.Activate();
        }
        
        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!rss");
            BotMethods.RemoveCommand("!rss-admin");
            
            base.Deactivate();
        }
        
        public override string AboutHelp()
        {
            return "The Rss Plugins reports new posts on the configured RSS feed to the channel, and it provides access to the complete rss via the !rss command";
        }
        
        private void showRss(object sender, IrcEventArgs e) {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick  : e.Data.Channel;
            foreach(string lines in rssFeeds.Select(item => item.Value.FriendlyName).ToLines(350)) {
                BotMethods.SendMessage(SendType.Message, sendto, lines);
            }
        }

        private void adminRss(object sender, IrcEventArgs e) {
            
        }
    }
}