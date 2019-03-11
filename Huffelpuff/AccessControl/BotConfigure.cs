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
using SharpIrc;
using Huffelpuff.Commands;

namespace Huffelpuff.AccessControl
{
    public class BotOption
    {

        public BotOption(string optionName, ref object optionPtr)
        {
            OptionName = optionName;
            OptionPtr = optionPtr;
            OptionType = optionPtr.GetType();
        }

        public string OptionName { get; }
        public object OptionPtr { get; }
        public Type OptionType { get; }
    }

    /// <summary>
    /// Description of BotConfigure.
    /// </summary>
    public class BotConfigure
    {
        private Dictionary<string, BotOption> _options;

        public IrcBot Bot { get; set; }

        public BotConfigure(IrcBot botInstance)
        {
            Bot = botInstance;

            Bot.AddCommand(new Commandlet("!options", "The command !options lists all options you can set in the bot", GetList, this, CommandScope.Both, "engine_list_option"));
            Bot.AddCommand(new Commandlet("!set", "The command !set <option> <value> sets the different options in the bot", SetOption, this, CommandScope.Both, "engine_set_option"));
            Bot.AddCommand(new Commandlet("!get", "The command !get <option> shows the set value in the bot", GetOption, this, CommandScope.Both, "engine_get_option"));

            _options = new Dictionary<string, BotOption>();
        }

        private void GetList(object sender, IrcEventArgs e)
        {

        }

        private void SetOption(object sender, IrcEventArgs e)
        {

        }

        private void GetOption(object sender, IrcEventArgs e)
        {

        }
    }
}
