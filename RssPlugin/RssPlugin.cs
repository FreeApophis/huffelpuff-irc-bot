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
        
        public override void Activate()
        {
            BotMethods.AddPublicCommand(new Commandlet("!rss", "RSS_HELP", showRss, this));
            
            base.Activate();
        }
        
        public override void Deactivate()
        {
            BotMethods.RemovePublicCommand("!rss");
            
            base.Deactivate();
        }
        
        public override string AboutHelp()
        {
            return "RSS_PLUGIN_HELP";
        }
        
        private void showRss(object sender, IrcEventArgs e) {
            foreach(RssItem item in getRss()) {
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, item.Title + " was published on " + item.Published.ToString() + " by " + item.Author + " in " + item.Category + " -> " + item.Link);
            }
        }

        
        private List<RssItem> getRss() 
        {
            List<RssItem> rss = new List<RssItem>();

            XmlReader feed = XmlReader.Create("http://forum.vis.ethz.ch/external.php?type=RSS2");
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