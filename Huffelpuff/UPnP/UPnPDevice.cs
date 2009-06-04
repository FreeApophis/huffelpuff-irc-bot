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
        
        internal UPnPDevice()
        {
        }
       
    }
}
