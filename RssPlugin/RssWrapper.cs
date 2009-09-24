/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 05.09.2009 12:30
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
using Huffelpuff.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Huffelpuff;

namespace Plugin
{
    /// <summary>
    /// Description of RssWrapper.
    /// </summary>
    public class RssWrapper
    {
        public string Name { get; private set; }
        public string NameSpace {
            get{
                return "rssfeed_" + Name;
            }
        }
        
        private const string friendlynameconst = "friendlyname";
        private string friendlyName;
        public string FriendlyName {
            get  {
                return friendlyName;
            }
            set {
                PersistentMemory.Instance.SetValue(NameSpace, friendlynameconst, value);
                friendlyName = value;
            }
        }
        
        private const string urlconst = "url";
        private string url;
        public string Url {
            get  {
                return url;
            }
            set {
                PersistentMemory.Instance.SetValue(NameSpace, urlconst, value);
                url = value;
            }
        }

        private const string lastconst = "lastdate";
        private DateTime last;
        public DateTime Last {
            get  {
                return last;
            }
            set {
                PersistentMemory.Instance.SetValue(NameSpace, lastconst, value.ToString());
                last = value;
            }
        }
        
        private List<RssItem> cachedItems = new List<RssItem>();
        
        public RssWrapper(string name)
        {
            Name = name;
            friendlyName = PersistentMemory.Instance.GetValueOrTodo(NameSpace, friendlynameconst);
            url = PersistentMemory.Instance.GetValueOrTodo(NameSpace, urlconst);
            
            string lastDateTimeString = PersistentMemory.Instance.GetValue(NameSpace, lastconst);
            last = (lastDateTimeString == null) ? DateTime.MinValue : DateTime.Parse(lastDateTimeString);
        }
        
        public RssWrapper(string name, string friendlyName, string url)
        {
            Name = name;
            this.friendlyName = friendlyName;
            this.url = url;
            this.last = DateTime.MinValue;
            
            PersistentMemory.Instance.ReplaceValue(NameSpace, friendlynameconst, friendlyName);
            PersistentMemory.Instance.ReplaceValue(NameSpace, urlconst, url);
            PersistentMemory.Instance.ReplaceValue(NameSpace, lastconst, last.ToString());
        }
        
        public void RemoveFeed() {
            PersistentMemory.Instance.RemoveValue(RssPlugin.rssfeedconst, Name);
            PersistentMemory.Instance.RemoveGroup(NameSpace);
        }

        
        /// <summary>
        /// Returns new posts on the Rss Feed, this call is not idempotent
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RssItem> NewItems () {
            cachedItems = getRss();
            var newItems = cachedItems.Where(item => item.Published > last).OrderBy(item => item.Published).ToList();
            if (newItems.Count() > 0) {
                last = newItems.OrderByDescending(item => item.Published).Take(1).Single().Published;
                PersistentMemory.Instance.ReplaceValue(NameSpace, lastconst, last.ToString());
            }
            return newItems;
        }
        
        
        
        public RssItem this[int i] {
            get {
                if(i < cachedItems.Count) {
                    return cachedItems[i];
                }
                return null;
            }
        }
        
        public int Count {
            get {
                return cachedItems.Count;
            }
        }

        
        private List<RssItem> getRss()
        {
            List<RssItem> rss = new List<RssItem>();

            XmlReader feed = XmlReader.Create(url);
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
                        case "title":
                            title = feed.ReadElementContentAsString();
                            break;
                        case "link":
                            link = feed.ReadElementContentAsString();
                            break;
                        case "description":
                            desc = feed.ReadElementContentAsString();
                            break;
                            // The pubDate is important for notifying
                        case "pubDate":
                            string str = feed.ReadElementContentAsString();
                            DateTime.TryParse(str, out published);
                            break;
                            // Some more RSS 2.0 Standard fields.
                        case "category":
                            category = feed.ReadElementContentAsString();
                            break;
                        case "author":
                            author = feed.ReadElementContentAsString();
                            break;
                        case "guid":
                            //link = feed.ReadElementContentAsString();
                            break;
                            // Special ones (for vBulletin)
                        case "content:encoded":
                            content = feed.ReadElementContentAsString();
                            break;
                        case "dc:creator":
                            author = feed.ReadElementContentAsString();
                            break;
                        case "comments":
                            //Comment
                            break;
                        case "wfw:commentRss":
                            //Comment LInk
                            break;
                        default:
                            Log.Instance.Log("unparsed Element: " + feed.Name);
                            break;
                    }
                }
            }
            return new RssItem(title, author, published, link, desc, category, content);
        }
    }
}
