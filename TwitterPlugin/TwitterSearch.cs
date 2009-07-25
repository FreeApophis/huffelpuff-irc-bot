/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 19.07.2009 23:27
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
    /// Description of TwitterSearch.
    /// </summary>
    public class TwitterSearch
    {
        internal TwitterSearch(string tag) {
            this.tag = tag;
        }
        
        private string tag;
        
        public string Tag {
            get { return tag; }
        }
        
        public long Id {get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
        public string Author { get; set; }
        public string Feed { get; set; }

    }
}
