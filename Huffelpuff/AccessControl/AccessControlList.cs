﻿/*
 *  The Huffelpuff Irc Bot, versatile pluggable bot for IRC chats
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
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

namespace Huffelpuff
{
    /// <summary>
    /// Description of AccessControlList.
    /// </summary>
    public class AccessControlList
    {
        private List<IdentifyUser> identifyPlugins = new List<IdentifyUser>();
        private Dictionary<string, List<string>> accessList = new Dictionary<string, List<string>>();
        private IrcBot bot = null;
                
        public AccessControlList(IrcBot bot)
        {
            this.bot = bot;
        }
        
        public void AddIdentifyPlugin(IdentifyUser identify) {
            identifyPlugins.Add(identify);
        }
        
        public void AddAccessRight(string identity, string accessString) 
        {
            if (!accessList.ContainsKey(identity)) {
                accessList.Add(identity, new List<string>());
            }
            accessList[identity].Add(accessString);
        }
        
        public void RemoveAccessRight(string identity, string accessString) 
        {
            if (accessList.ContainsKey(identity)) {
                accessList[identity].Remove(accessString);
            }
        }
        
        public bool Access(string nick, string command)
        {
            foreach(IdentifyUser id in identifyPlugins) {
                string identity = id.IdentifyString(nick);
                foreach(string accessString in accessList[identity]) {
                
                }
            }
            return false;
        }
    }
    

    
}
