/*
 *  The Huffelpuff Irc Bot, versatile pluggable bot for IRC chats
 * 
 *  Copyright (c) 2008-2010 Thomas Bruderer <apophis@apophis.ch>
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
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Huffelpuff.Utils;

namespace Plugin
{
    /// <summary>
    /// Description of RssWrapper.
    /// </summary>
    public class RssWrapper
    {
        public string Name { get; private set; }
        public string NameSpace
        {
            get
            {
                return "rssfeed_" + Name;
            }
        }

        private const string FriendlyNameConst = "friendlyname";
        private string friendlyName;
        public string FriendlyName
        {
            get
            {
                return friendlyName;
            }
            set
            {
                PersistentMemory.Instance.SetValue(NameSpace, FriendlyNameConst, value);
                friendlyName = value;
            }
        }

        private const string UrlConst = "url";
        private string url;
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                PersistentMemory.Instance.SetValue(NameSpace, UrlConst, value);
                url = value;
            }
        }

        private const string CredentialsConst = "credentials";
        private string credentials;
        public string Credentials
        {
            get
            {
                return credentials;
            }
            set
            {
                PersistentMemory.Instance.SetValue(NameSpace, CredentialsConst, value);
                credentials = value;
            }
        }


        private const string LastConst = "lastdate";
        private DateTime last;
        public DateTime Last
        {
            get
            {
                return last;
            }
            set
            {
                PersistentMemory.Instance.SetValue(NameSpace, LastConst, value.ToString());
                last = value;
            }
        }

        public int ErrorCount { get; private set; }
        public int CallCount { get; private set; }


        private List<RssItem> cachedItems = new List<RssItem>();

        public RssWrapper(string name)
        {
            Name = name;
            friendlyName = PersistentMemory.Instance.GetValueOrTodo(NameSpace, FriendlyNameConst);
            url = PersistentMemory.Instance.GetValueOrTodo(NameSpace, UrlConst);
            credentials = PersistentMemory.Instance.GetValue(NameSpace, CredentialsConst);

            string lastDateTimeString = PersistentMemory.Instance.GetValue(NameSpace, LastConst);
            last = (lastDateTimeString == null) ? DateTime.MinValue : DateTime.Parse(lastDateTimeString);
        }

        public RssWrapper(string name, string friendlyName, string url, DateTime last, string credentials)
        {
            Name = name;
            this.friendlyName = friendlyName;
            this.url = url;
            this.last = last;
            this.credentials = credentials;

            PersistentMemory.Instance.ReplaceValue(NameSpace, FriendlyNameConst, friendlyName);
            PersistentMemory.Instance.ReplaceValue(NameSpace, UrlConst, url);
            PersistentMemory.Instance.ReplaceValue(NameSpace, LastConst, last.ToString());
            if (credentials == null)
            {
                PersistentMemory.Instance.RemoveKey(NameSpace, CredentialsConst);
            }
            else
            {
                PersistentMemory.Instance.ReplaceValue(NameSpace, CredentialsConst, credentials);
            }
            PersistentMemory.Instance.Flush();
        }

        public void RemoveFeed()
        {
            PersistentMemory.Instance.RemoveValue(RssPlugin.RssFeedConst, Name);
            PersistentMemory.Instance.RemoveGroup(NameSpace);
            PersistentMemory.Instance.Flush();
        }


        /// <summary>
        /// Returns new posts on the Rss Feed, this call is not idempotent
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RssItem> NewItems()
        {
            CallCount++;
            try
            {
                cachedItems = GetRss();
                var newItems = cachedItems.Where(item => item.Published > last).OrderBy(item => item.Published).ToList();
                if (newItems.Count() > 0)
                {
                    last = newItems.OrderByDescending(item => item.Published).Take(1).Single().Published;
                    PersistentMemory.Instance.ReplaceValue(NameSpace, LastConst, last.ToString());
                }
                return newItems;
            }
            catch (Exception)
            {
                ErrorCount++;
                throw;
            }
        }



        public RssItem this[int i]
        {
            get
            {
                if (cachedItems.Count == 0)
                {
                    cachedItems = GetRss();
                }
                return i < cachedItems.Count ? cachedItems[i] : null;
            }
        }

        public int Count
        {
            get
            {
                return cachedItems.Count;
            }
        }

        private static readonly XNamespace AtomNamespace = "http://www.w3.org/2005/Atom";
        private static readonly XNamespace PurlNamespace = "http://purl.org/dc/elements/1.1/";

        private List<RssItem> GetRss()
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            if (!string.IsNullOrEmpty(credentials))
            {
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials)));
            }

            var response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();
            var reader = XmlReader.Create(stream);

            var rssFeed = XDocument.Load(reader);

            var atomEntry = XName.Get("entry", AtomNamespace.NamespaceName);
            var rssEntry = XName.Get("item", "");

            // Parse RSS
            var rss = rssFeed.Descendants(atomEntry).Select(GetAtomItem).Where(rssItem => rssItem != null).ToList();

            //Parse Rss
            rss.AddRange(rssFeed.Descendants(rssEntry).Select(GetRssItem).Where(rssItem => rssItem != null).ToList());

            return rss;
        }

        private static RssItem GetRssItem(XElement item)
        {
            DateTime published;

            DateTime.TryParse(item.Descendants("pubDate").Select(n => n.Value).FirstOrDefault(), out published);

            var title = item.Descendants("title").Select(n => n.Value).FirstOrDefault();
            var author = item.Descendants("author").Select(n => n.Value).FirstOrDefault() ??
                item.Descendants(PurlNamespace + "creator").Select(n => n.Value).FirstOrDefault();
            var link = item.Descendants("link").Select(n => n.Value).FirstOrDefault();
            var description = item.Descendants("description").Select(n => n.Value).FirstOrDefault();
            var category = item.Descendants("category").Select(n => n.Value).FirstOrDefault();
            var content = item.Descendants("content").Select(n => n.Value).FirstOrDefault();

            return (title != null) ? new RssItem(title, author, published, link, description, category, content) : null;
        }

        private static RssItem GetAtomItem(XElement entry)
        {
            string author = null, link = null;
            DateTime published;

            DateTime.TryParse(
                entry.Descendants(AtomNamespace + "published").Select(n => n.Value).FirstOrDefault() ??
                entry.Descendants(AtomNamespace + "updated").Select(n => n.Value).FirstOrDefault(), out published);

            var title = entry.Descendants(AtomNamespace + "title").Select(n => n.Value).FirstOrDefault();
            var authorEntry = entry.Descendants(AtomNamespace + "author").FirstOrDefault();
            if (authorEntry != null)
            {
                author = authorEntry.Descendants(AtomNamespace + "name").Select(n => n.Value).FirstOrDefault();
            }
            var linkEntry = entry.Descendants(AtomNamespace + "link").Select(n => n.Attribute(("href"))).FirstOrDefault();
            if (linkEntry != null)
            {
                link = linkEntry.Value;
            }
            var description = entry.Descendants(AtomNamespace + "summary").Select(n => n.Value).FirstOrDefault();
            var category = entry.Descendants("todo").Select(n => n.Value).FirstOrDefault();
            var content = entry.Descendants("todo").Select(n => n.Value).FirstOrDefault();

            return (title != null) ? new RssItem(title, author, published, link, description, category, content) : null;
        }
    }
}
