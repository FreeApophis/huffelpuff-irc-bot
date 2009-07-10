/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 04.07.2009 17:30
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
    /// Description of TwitterMention.
    /// </summary>
    public class TwitterMention
    {
        internal TwitterMention(string feed) {
            this.feed = feed;
        }

        private string feed;
        
        public string Feed {
            get { return feed; }
        }
        
        private TwitterUser user = new TwitterUser();
        
        public TwitterUser User {
            get { return user; }
        }
        
        public long Id { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
        public string Source { get; set; }
        public bool answered { get; set; }
        
        
    }
    

}


