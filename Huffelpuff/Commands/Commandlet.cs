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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpIrc;
using Huffelpuff.Plugins;

namespace Huffelpuff.Commands
{
    /// <summary>
    /// Description of Commandlet.
    /// </summary>
    public class Commandlet : MarshalByRefObject
    {
        private readonly List<string> _channels;

        public string Command { get; }

        public string HelpText { get; }

        public EventHandler<IrcEventArgs> Handler { get; }

        public string HandlerName { get; }

        public object Owner { get; }

        public string SourcePlugin { get; }

        public CommandScope Scope { get; }

        public string AccessString { get; }

        public ReadOnlyCollection<string> Channels => _channels == null ? null : new ReadOnlyCollection<string>(_channels);

        /// <summary>
        /// A commandlet represents the abstract idea of the typical IRC command represented with a start-character (!) and a string for identify. All the parsing will be done
        /// by the bot, you only get exactly the events you registered to. For example: you can register an event which is only thrown if used in a private message by certain users,
        /// or in a certain channel.
        /// </summary>
        /// <param name="command">the command string including initial character. like "!example" </param>
        /// <param name="helptext">A help for this certain command which should be displayed by the !help command</param>
        /// <param name="handler">The name of the method which should be called, can be private</param>
        /// <param name="owner">this (the class where this command is provided)</param>
        /// <param name="scope">Should the event be fired by channel messages or private messages or Both</param>
        /// <param name="accessString">globally unique string which identifies a restricted function, something like: plugin_function, can be done manually too, via bot.acl.*</param>
        /// <param name="channels">Only useful if the Scope is Public! The Handler will only be called if the request was made in a certain channel. No restriction == null</param>
        public Commandlet(string command, string helptext, EventHandler<IrcEventArgs> handler, object owner, CommandScope scope = CommandScope.Both, string accessString = null, List<string> channels = null)
        {
            Command = command;
            HelpText = helptext;

            if (owner is AbstractPlugin plugin)
            {
                Handler = null;
                HandlerName = handler.Method.Name;
                Owner = plugin.FullName;
                SourcePlugin = plugin.FullName;
            }
            else
            {
                Handler = handler;
                Owner = owner;
                SourcePlugin = "Core";
            }

            Scope = scope;
            AccessString = accessString;
            this._channels = channels;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
