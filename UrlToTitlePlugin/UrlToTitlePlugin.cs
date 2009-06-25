/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 18.06.2009 13:57
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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class UrlToTitlePlugin : AbstractPlugin
    {
        public UrlToTitlePlugin(IrcBot botInstance) : base(botInstance) {}
        
        
        public override string AboutHelp()
        {
            return "Translates any discussion";
        }

        public override void Init()
        {
            base.Init();
        }
        
        
        
        public override void Activate()
        {
            if (!this.active) {
                BotEvents.OnChannelMessage += new IrcEventHandler(BotEvents_OnChannelMessage);
            }

            base.Activate();
        }
        
        public override void Deactivate()
        {
            BotEvents.OnChannelMessage -= new IrcEventHandler(BotEvents_OnChannelMessage);

            base.Deactivate();
        }
        
        private Regex titleMatch = new Regex("(?<=<title>)[^<]*(?=</title>)", RegexOptions.IgnoreCase);
        private Regex fMatch = new Regex("(?<=f=)[0-9]*");
        private Regex tMatch = new Regex("(?<=t=)[0-9]*");
        private Regex pMatch = new Regex("(?<=t=)[0-9]*");
        private Regex charsetMatch = new Regex("(?<=charset=)[^(;\" )]*", RegexOptions.IgnoreCase);
        
        private void BotEvents_OnChannelMessage(object sender, IrcEventArgs e)
        {
            try {
                if (e.Data.MessageArray.Length == 1) {
                    if (e.Data.Message.StartsWith("http://forum.piraten-partei.ch/viewtopic.php?f=")) {
                        string f = fMatch.Match(e.Data.Message).Value;
                        string t = tMatch.Match(e.Data.Message).Value;
                        ForumItem item = getTopic("http://forum.piraten-partei.ch/rss.php?f=" + f + "&t=" + t + "&start=last", false);
                        BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Piratenforum - " + item.Title + " (by " + item.Author + ")");
                    } else if (e.Data.Message.StartsWith("http://")) {
                        WebClient client = new WebClient();
                        
                        string page = client.DownloadString(e.Data.Message);
                        string charset = charsetMatch.Match(page).Value;
                        string title = titleMatch.Match(page).Value;
                        
                        if (charset.ToLower()!="") {
                            Encoding enc = System.Text.Encoding.GetEncoding(charset);
                            byte[] encBytes = client.Encoding.GetBytes(title);
                            byte[] utfBytes = System.Text.Encoding.Convert(enc, new System.Text.UTF8Encoding(), encBytes);
                            char[] utfChars = System.Text.Encoding.UTF8.GetChars(utfBytes);
                            title = new String(utfChars);
                        }

                        title = RemoveNewLine(title);
                        title = HttpUtility.HtmlDecode(title);
                        BotMethods.SendMessage(SendType.Message, e.Data.Channel, title);
                        
                    }
                }
            } catch {}
            
        }
        private char c10 = Convert.ToChar(10);
        private char c13 = Convert.ToChar(13);
        
        private string RemoveNewLine(string s) {
            StringBuilder sb = new StringBuilder();
            foreach (char c in s) {
                if ((c != c10) && (c != c13)) {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        
        private ForumItem getTopic(string uri, bool hack)
        {

            ForumItem item = null;
            XmlReader feed = XmlReader.Create(uri);
            while(feed.Read()){
                if ((feed.NodeType == XmlNodeType.Element) && (feed.Name == "item")) {
                    ForumItem temp = getItem(feed);
                    if (temp.Published == DateTime.MinValue) {
                        if (!hack) {
                            item = getTopic(uri + "&start=" + getHack(item.Description), true);
                        }
                    } else {
                        item = temp;
                    }
                }
            }
            return item;
        }
        
        private Regex hackMatch = new Regex("(?<=Antworten )[0-9]*", RegexOptions.IgnoreCase);

        private string getHack(string s) {
            return hackMatch.Match(s).Value;
        }
        
        private ForumItem getItem(XmlReader feed)  {

            string title = "", author = "", desc = "";

            DateTime published = DateTime.MinValue;
            
            while(feed.Read()){
                if ((feed.NodeType == XmlNodeType.EndElement) && (feed.Name == "item")) {
                    break;
                }
                if (feed.NodeType == XmlNodeType.Element) {
                    switch(feed.Name) {
                            case "title": feed.Read();
                            title = feed.ReadContentAsString();
                            break;
                            case "link":feed.Read();
                            //link = feed.ReadContentAsString();
                            break;
                            case "description":feed.Read();
                            desc = feed.ReadContentAsString();
                            break;
                            case "content:encoded":feed.Read();
                            //content
                            break;
                            case "category":feed.Read();
                            //category = feed.ReadContentAsString();
                            break;
                            case "author":feed.Read();
                            author = feed.ReadContentAsString();
                            break;
                            case "pubDate":feed.Read();
                            published = DateTime.Parse(feed.ReadContentAsString());
                            break;
                            case "guid":feed.Read();
                            //guid
                            break;
                            default: Console.WriteLine("unparsed Element: " + feed.Name);
                            break;
                    }
                }
            }
            return new ForumItem(title, author, published, desc);
        }
    }
}
