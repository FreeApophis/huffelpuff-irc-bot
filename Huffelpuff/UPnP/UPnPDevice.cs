/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 04.06.2009 14:28
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
using System.Text.RegularExpressions;

namespace Huffelpuff.UPnP
{
    /// <summary>
    /// Description of UPnPDevice.
    /// </summary>
    public abstract class UPnPDevice
    {
        private TimeSpan maxCacheAge;
        
        public TimeSpan MaxCacheAge {
            get { return maxCacheAge; }
        }
        
        private string uuid;
        
        public string Uuid {
            get { return uuid; }
        }
        
        private Uri location;
        
        public Uri Location {
            get { return location; }
        }
        
        private string server;
        
        public string Server {
            get { return server; }
        }

        private Regex locationMatch = new Regex("(?<=location:).*(?=(\n|\r\n))", RegexOptions.IgnoreCase);
        private Regex uuidMatch = new Regex("(?<=uuid:).*(?=::)", RegexOptions.IgnoreCase);
        private Regex serverMatch = new Regex("(?<=server:).*(?=(\n|\r\n))", RegexOptions.IgnoreCase);
        private Regex cacheMatch = new Regex("(?<=max-age=).*(?=(\n|\r\n))", RegexOptions.IgnoreCase);

        
        internal UPnPDevice(string response)
        {
            location = new Uri(locationMatch.Match(response).Value);
            uuid = uuidMatch.Match(response).Value;
            server = serverMatch.Match(response).Value;
            maxCacheAge = new TimeSpan(10000000 * long.Parse(cacheMatch.Match(response).Value));
        }
        
    }
}
