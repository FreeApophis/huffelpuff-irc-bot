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
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        private string accessString;
        private List<string> channelList;

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
        
        public string AccessString {
            get {
                return accessString;
            }
        }
        
        public ReadOnlyCollection<string> ChannelList {
            get {
                if (channelList==null)
                    return null;
                else 
                    return new ReadOnlyCollection<string>(channelList);
            }
        }
        
        /// <summary>
        /// A commandlet represents the abstract idea of the typical IRC command represented with a start-character (!) and a string for identify. All the parsing will be done
        /// by the bot, you only get exactly the events you registered to. For example: you can register an event which is only thrown if used in a private message by certain users,
        /// or in a certain channel.
        /// </summary>
        /// <param name="command">the command string including initial charakter. like "!example" </param>
        /// <param name="helptext">A help for this certain command which should be displayed by the !help command</param>
        /// <param name="handler">The name of the method which should be called, can be private</param>
        /// <param name="owner">this (the class where this command is provided)</param>
        public Commandlet(string command, string helptext, IrcEventHandler handler, object owner) : 
            this(command, helptext, handler, owner, CommandScope.Both)
        {}

        /// <summary>
        /// A commandlet represents the abstract idea of the typical IRC command represented with a start-character (!) and a string for identify. All the parsing will be done
        /// by the bot, you only get exactly the events you registered to. For example: you can register an event which is only thrown if used in a private message by certain users,
        /// or in a certain channel.
        /// </summary>
        /// <param name="command">the command string including initial charakter. like "!example" </param>
        /// <param name="helptext">A help for this certain command which should be displayed by the !help command</param>
        /// <param name="handler">The name of the method which should be called, can be private</param>
        /// <param name="owner">this (the class where this command is provided)</param>
        /// <param name="scope">Should the event be fired by Channelmessages or Privatemessages or Both</param>
        public Commandlet(string command, string helptext, IrcEventHandler handler, object owner, CommandScope scope) : 
            this(command, helptext, handler, owner, scope, null)
        {}

        /// <summary>
        /// A commandlet represents the abstract idea of the typical IRC command represented with a start-character (!) and a string for identify. All the parsing will be done
        /// by the bot, you only get exactly the events you registered to. For example: you can register an event which is only thrown if used in a private message by certain users,
        /// or in a certain channel.
        /// </summary>
        /// <param name="command">the command string including initial charakter. like "!example" </param>
        /// <param name="helptext">A help for this certain command which should be displayed by the !help command</param>
        /// <param name="handler">The name of the method which should be called, can be private</param>
        /// <param name="owner">this (the class where this command is provided)</param>
        /// <param name="scope">Should the event be fired by Channelmessages or Privatemessages or Both</param>
        /// <param name="accessString">globally unique string which identifies a restricted function, something like: plugin_function, can be done manually too, via bot.acl.*</param>
        public Commandlet(string command, string helptext, IrcEventHandler handler, object owner, CommandScope scope, string accessString) :
            this(command, helptext, handler, owner, scope, accessString, null)
        {}
        
        /// <summary>
        /// A commandlet represents the abstract idea of the typical IRC command represented with a start-character (!) and a string for identify. All the parsing will be done
        /// by the bot, you only get exactly the events you registered to. For example: you can register an event which is only thrown if used in a private message by certain users,
        /// or in a certain channel.
        /// </summary>
        /// <param name="command">the command string including initial charakter. like "!example" </param>
        /// <param name="helptext">A help for this certain command which should be displayed by the !help command</param>
        /// <param name="handler">The name of the method which should be called, can be private</param>
        /// <param name="owner">this (the class where this command is provided)</param>
        /// <param name="scope">Should the event be fired by Channelmessages or Privatemessages or Both</param>
        /// <param name="accessString">globally unique string which identifies a restricted function, something like: plugin_function, can be done manually too, via bot.acl.*</param>
        /// <param name="channelList">Only usefull if the Scope is Public! The Handler will only be called if the request was made in a certain channel. No restriction == null</param>
        public Commandlet(string command, string helptext, IrcEventHandler handler, object owner, CommandScope scope, string accessString,  List<string> channelList)
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
            this.accessString = accessString;
            this.channelList = channelList;
        }
        
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
