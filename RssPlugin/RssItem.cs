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

namespace Plugin
{
    /// <summary>
    /// Description of RssItem.
    /// </summary>
    public class RssItem
    {
        public RssItem(string title, string author, DateTime published, string link, string desc, string category, string content)
        {
            this.title = title;
            this.author = author;
            this.published = published;
            this.link = link;
            this.desc = desc;
            this.category = category;
            this.content = content;
        }

        private string link;

        public string Link
        {
            get
            {
                return link;
            }
        }

        private string desc;

        public string Desc
        {
            get
            {
                return desc;
            }
        }

        private string category;

        public string Category
        {
            get
            {
                return category;
            }
        }

        private string content;

        public string Content
        {
            get
            {
                return content;
            }
        }

        private string title;

        public string Title
        {
            get
            {
                return title;
            }
        }

        private string author;

        public string Author
        {
            get
            {
                return author;
            }
        }

        private DateTime published;

        public DateTime Published
        {
            get
            {
                return published;
            }
        }
    }
}
