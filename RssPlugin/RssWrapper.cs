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
using Plugin.Database.Rss;

namespace Plugin
{
    /// <summary>
    /// Description of RssWrapper.
    /// </summary>
    public class RssWrapper
    {
        private RssAccount feed;

        public string FriendlyName
        {
            get
            {
                return feed.FriendlyName;
            }
            set
            {
                feed.FriendlyName = value;
                RssPlugin.RssData.SubmitChanges();
            }
        }

        public string Url
        {
            get
            {
                return feed.Url;
            }
            set
            {
                feed.Url = value;
                RssPlugin.RssData.SubmitChanges();
            }
        }

        public string Credentials
        {
            get
            {
                return feed.Credentials;
            }
            set
            {
                feed.Credentials = value;
                RssPlugin.RssData.SubmitChanges();
            }
        }


        public DateTime Last
        {
            get
            {
                return feed.LastMessage.HasValue ? feed.LastMessage.Value : DateTime.Now;
            }
            set
            {
                feed.LastMessage = value;
                RssPlugin.RssData.SubmitChanges();
            }
        }

        public int ErrorCount { get; private set; }
        public int CallCount { get; private set; }


        private List<RssItem> cachedItems = new List<RssItem>();

        public RssWrapper(string friendlyName)
        {
            feed = RssPlugin.RssData.RssAccounts.Where(a => a.FriendlyName == friendlyName).FirstOrDefault();
        }

        public RssWrapper(string friendlyName, string url, DateTime last, string credentials)
        {
            feed = new RssAccount();
            RssPlugin.RssData.RssAccounts.InsertOnSubmit(feed);

            feed.FriendlyName = friendlyName;
            feed.Url = url;
            feed.LastMessage = last;
            feed.Credentials = credentials;

            RssPlugin.RssData.SubmitChanges();
        }

        public void RemoveFeed()
        {
            RssPlugin.RssData.RssAccounts.DeleteOnSubmit(feed);
            RssPlugin.RssData.SubmitChanges();
            feed = null;
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
                var newItems = cachedItems.Where(item => item.Published > Last).OrderBy(item => item.Published).ToList();
                if (newItems.Count() > 0)
                {
                    Last = newItems.OrderByDescending(item => item.Published).Take(1).Single().Published;
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
            var request = (HttpWebRequest)WebRequest.Create(Url);

            if (!string.IsNullOrEmpty(Credentials))
            {
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(Credentials)));
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
