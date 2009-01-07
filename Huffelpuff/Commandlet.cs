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
using Meebey.SmartIrc4net;

namespace Huffelpuff
{
    /// <summary>
    /// Description of Commandlet.
    /// </summary>
    public class Commandlet : MarshalByRefObject
    {
        private string _Command;
        private string _HelpText;
        private IrcEventHandler _Handler;
        private object _Owner;
        private object _ACL;

        public string Command {
            get {
                return _Command;
            }
        }
            
        public string HelpText {
            get {
                return _HelpText;
            }
        }
        
        public IrcEventHandler Handler {
            get {
                return _Handler;
            }
        }
            
        public object Owner {
            get {
                return _Owner;
            }
        }
            
        public Commandlet(string command, string helptext, IrcEventHandler handler, object owner)
        {
            _Command = command;
            _HelpText = helptext;
            _Handler = handler;
            _Owner = owner;
            _ACL = null;
        }
    }
}
