/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 25.06.2009 01:47
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
using System.Collections.Generic;

namespace Huffelpuff
{
    public class BotOption {
        
        public BotOption(string optionName, ref object optionPtr) {
            OptionName = optionName;
            OptionPtr = optionPtr;
            OptionType = optionPtr.GetType();
        }
        
        public string OptionName {get; private set;}
        public object OptionPtr {get; private set;}
        public Type OptionType {get; private set;}
    }
    
    /// <summary>
    /// Description of BotConfigure.
    /// </summary>
    public class BotConfigure
    {
        private IrcBot bot;
        
        private Dictionary<string, BotOption> options;
        
        public BotConfigure(IrcBot botInstance)
        {
            bot = botInstance;
            
            bot.AddCommand(new Commandlet("!options", "The command !options lists all options you can set in the bot", this.GetList, this, CommandScope.Both, "engine_list_option"));
            bot.AddCommand(new Commandlet("!set", "The command !set <option> <value> sets the different options in the bot", this.SetOption, this, CommandScope.Both, "engine_set_option"));
            bot.AddCommand(new Commandlet("!get", "The command !get <option> shows the set value in the bot", this.GetOption, this, CommandScope.Both, "engine_get_option"));
            
            options = new Dictionary<string, BotOption>();            
        }
        
        private void GetList(object sender, IrcEventArgs e) {
            
        }

        private void SetOption(object sender, IrcEventArgs e) {
            
        }

        private void GetOption(object sender, IrcEventArgs e) {
            
        }

        
    }
}
