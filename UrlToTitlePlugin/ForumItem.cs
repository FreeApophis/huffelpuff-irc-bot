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
    /// Description of ForumItem.
    /// </summary>
    public class ForumItem
    {
        public ForumItem(string title, string author, DateTime published, string description)
        {
            this.title = title;
            this.author = author;
            this.published = published;
            this.description = description;
        }
        
        private string title;
        
        public string Title {
            get {
                return title;
            }
        }

        private string author;

        public string Author {
            get {
                return author;
            }
        }

        private string description;
        
        public string Description {
            get { 
                return description; 
            }
        }

        
        private DateTime published;

        public DateTime Published {
            get {
                return published;
            }
        }
    }
}
