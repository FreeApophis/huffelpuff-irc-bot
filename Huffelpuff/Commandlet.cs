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
using Huffelpuff.Plugins;

namespace Huffelpuff
{
    public enum CommandScope {
        Private,
        Public,
        Both
    }
    
    /// <summary>
    /// Description of Commandlet.
    /// </summary>
    public class Commandlet : MarshalByRefObject
    {
        private string command;
        private string helpText;
        private IrcEventHandler handler;
        private string handlerName;
        private object owner;
        private CommandScope scope;

        private object acl;

        public string Command {
            get {
                return command;
            }
        }
            
        public string HelpText {
            get {
                return helpText;
            }
        }
        
        public IrcEventHandler Handler {
            get {
                return handler;
            }
        }
        
        public string HandlerName {
            get {
                return handlerName;
            }
        }
            
        public object Owner {
            get {
                return owner;
            }
        }

        public CommandScope Scope {
            get { 
                return scope; 
            }
        }
        
        public Commandlet(string command, string helptext, IrcEventHandler handler, object owner) : 
            this(command, helptext, handler, owner, CommandScope.Both)
        {}

        public Commandlet(string command, string helptext, IrcEventHandler handler, object owner, CommandScope scope) : 
            this(command, helptext, handler, owner, scope, "*")
        {}
        
        public Commandlet(string command, string helptext, IrcEventHandler handler, object owner, CommandScope scope, string channelList)
        {
            this.command = command;
            this.helpText = helptext;
            
            if (owner is AbstractPlugin) {
                this.handler = null;
                this.handlerName = handler.Method.Name;
                this.owner = owner.GetType().FullName;
            } else {
                this.handler = handler;
                this.owner = owner;
            }
            
            this.scope = scope;
            this.acl = null;
        }
        
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
