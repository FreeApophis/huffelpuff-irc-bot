using System;
using apophis.SharpIRC;
using Huffelpuff;
using Huffelpuff.Plugins;
/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 01.07.2009 23:07
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

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class TimePlugin : AbstractPlugin
    {
        public TimePlugin(IrcBot botInstance) :
            base(botInstance) {}
        
        public override string AboutHelp()
        {
            return "Tells you the current Time";
        }
        
        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!time", "The command !time givey the current time", NowHandler, this, CommandScope.Both));
            BotMethods.AddCommand(new Commandlet("!countdown", "The command !time gives the current time", CountdownHandler, this, CommandScope.Public, "access_time_countdown"));
            
            base.Activate();
        }
        
        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!time");
            BotMethods.RemoveCommand("!countdown");
            
            base.Deactivate();
        }

        private void NowHandler(object sender, IrcEventArgs e) {
            BotMethods.SendMessage(SendType.Message, e.Data.Nick, "Es ist jetzt " + DateTime.Now.ToString("HH:mm:ss") + "Uhr" + DateTime.Now.ToString("K") + ".");
        }

        private void CountdownHandler(object sender, IrcEventArgs e) {
            
        }

    }
}