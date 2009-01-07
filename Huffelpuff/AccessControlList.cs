/*
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

namespace Huffelpuff
{
    /// <summary>
    /// Description of AccessControlList.
    /// </summary>
    public class AccessControlList
    {
        public AccessControlList()
        {
        }
        
        public bool Access(string nick, string command)
        {
            return false;
        }
    }
    
    public class AccessRights
    {
        public virtual bool hasAccess(string Nick) {
            return false;
        }
    }
    
    public class ChannelRightAccess : AccessRights
    {
        
    }
    
    public class NickServAccess : AccessRights
    {    
        
    }
    
    public class PasswordAccess : AccessRights
    {
        
    }
    
    public class GuestAccess : AccessRights
    {
        public override bool hasAccess(string Nick) {
            return true;
        }
    }
    
}
