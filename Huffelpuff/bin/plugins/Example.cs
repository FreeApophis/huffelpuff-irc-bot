/*
 *  <one line to give the program's name and a brief idea of what it does.>
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
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.CompilerServices;

using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;


namespace Plugin
{
    /// <summary>
    /// This is a very simple Plugin Example: The Echo Plugin
    /// </summary>
    public class ExamplePlugin : AbstractPlugin
    {
        
        public ExamplePlugin(IrcBot botInstance) : 
            base(botInstance) {}

        
        public override void Activate() {
            BotMethods.AddCommand(new Commandlet("!example", "The command !example will send a message with Version to the Channel", exampleHandler, this, CommandScope.Public));
            active = true;
        }

        public override void Deactivate() {
            BotMethods.RemoveCommand("!example");
            active = false;
        }
                
        public override string AboutHelp() {
            return "This is a very simple Plugin which gives its version on !example !!!";
        }
                
        private void exampleHandler(object sender, IrcEventArgs e) {
        	BotMethods.SendMessage(SendType.Message, e.Data.Channel, "This is a dynamically compiled Plugin: " + Assembly.GetExecutingAssembly().FullName);
        }
    }
}
