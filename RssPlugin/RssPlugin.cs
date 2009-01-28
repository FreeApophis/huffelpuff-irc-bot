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
using System.Timers;
using System.Collections.Generic;

using Huffelpuff;
using Huffelpuff.ComplexPlugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class RssPlugin : AbstractPlugin
    {
        public RssPlugin(IrcBot botInstance) :
            base(botInstance) {}
        
        private Timer checkInterval;
        private bool firstrun = true;
        private DateTime lastpost = DateTime.MinValue;
        
        
        public override void Init()
        {
            checkInterval = new Timer();
            checkInterval.Elapsed += new ElapsedEventHandler(checkInterval_Elapsed);
            checkInterval.Interval = 1 * 60 * 1000; // 3 minutes
            base.Init();
        }

        void checkInterval_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!BotMethods.IsConnected)
                return;
            List<RssItem> rss = getRss(PersistentMemory.GetValue("rssFeed"));
            if (firstrun) {
                firstrun = false;    
                BotMethods.SendMessage(SendType.Message, "#botwar", "RSS Plugin loaded with Feed: " + PersistentMemory.GetValue("rssFeed"));
                BotMethods.SendMessage(SendType.Message, "#botwar", "Last Post: " + IrcConstants.IrcBold + rss[0].Title + IrcConstants.IrcBold  + " was published on " + rss[0].Published.ToString() + " by " + IrcConstants.IrcBold + IrcConstants.IrcColor + ((int)IrcColors.Blue) + rss[0].Author + IrcConstants.IrcBold + IrcConstants.IrcColor + " in " + rss[0].Category + " -> " + rss[0].Link);
                lastpost = rss[0].Published;
            } else if (lastpost < rss[0].Published) {
                BotMethods.SendMessage(SendType.Message, "#botwar", "New Post: " + IrcConstants.IrcBold + rss[0].Title + IrcConstants.IrcBold  + " was published on " + rss[0].Published.ToString() + " by " + IrcConstants.IrcBold + IrcConstants.IrcColor + ((int)IrcColors.Blue) + rss[0].Author + IrcConstants.IrcBold + IrcConstants.IrcColor + " in " + rss[0].Category + " -> " + rss[0].Link);
                lastpost = rss[0].Published;
            }
            
        }
        
        public override void Activate()
        {
            BotMethods.AddPublicCommand(new Commandlet("!rss", "With the command !rss you'll get a list of the bots configured RSS feed.", showRss, this));
            checkInterval.Enabled = true;
            
            base.Activate();
        }
        
        public override void Deactivate()
        {
            BotMethods.RemovePublicCommand("!rss");
            
            base.Deactivate();
        }
        
        public override string AboutHelp()
        {
            return "The Rss Plugins reports new posts on the configured RSS feed to the channel, and it provides access to the complete rss via the !rss command";
        }
        
        private void showRss(object sender, IrcEventArgs e) {
            foreach(RssItem item in getRss(PersistentMemory.GetValue("rssFeed"))) {
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, item.Title + " was published on " + item.Published.ToString() + " by " + item.Author + " in " + item.Category + " -> " + item.Link);
            }
        }

        
        private List<RssItem> getRss(string uri) 
        {
            List<RssItem> rss = new List<RssItem>();

            XmlReader feed = XmlReader.Create(uri);
            while(feed.Read()){
                if ((feed.NodeType == XmlNodeType.Element) && (feed.Name == "item")) {
                    rss.Add(getItem(feed));
                }
            }
            return rss;
        }
        
        private RssItem getItem(XmlReader feed)  {

            string title = "", author = ""; string link = ""; string desc = ""; string category = ""; string content = "";

            DateTime published = DateTime.MinValue;
                
            while(feed.Read()){
                if ((feed.NodeType == XmlNodeType.EndElement) && (feed.Name == "item")) {
                    break;
                }
                if (feed.NodeType == XmlNodeType.Element) {
                    switch(feed.Name) {
                        // Main Items every RSS feed has
                        case "title": feed.Read();
                            title = feed.ReadContentAsString();
                        break;
                        case "link":feed.Read();
                            link = feed.ReadContentAsString();
                        break;
                        case "description":feed.Read();
                            desc = feed.ReadContentAsString();
                        break;
                        // The pubDate is important for notifying
                        case "pubDate":feed.Read();
                            published = DateTime.Parse(feed.ReadContentAsString());
                        break;
                        // Some more RSS 2.0 Standard fields.
                        case "category":feed.Read();
                            category = feed.ReadContentAsString();
                        break;
                        case "author":feed.Read();
                            author = feed.ReadContentAsString();
                        break;
                        case "guid":feed.Read();
                            link = feed.ReadContentAsString();
                        break;
                        // Special ones (for vBulletin)
                        case "content:encoded":feed.Read();
                            content = feed.ReadContentAsString();
                        break;
                        case "dc:creator":feed.Read();
                            author = feed.ReadContentAsString();
                        break;
                        default: Console.WriteLine("unparsed Element: " + feed.Name);
                        break;
                    }
                }
            }


            return new RssItem(title, author, published, link, desc, category, content);
        }
    }
}