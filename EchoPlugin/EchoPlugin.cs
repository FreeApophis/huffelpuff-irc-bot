/*
 *  This is an example Plugin, you can use it as a base for your own plugins.
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

using System.Reflection;

using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// This is a very simple Plugin Example: The Echo Plugin
    /// </summary>
    public class EchoPlugin : AbstractPlugin
    {

        public EchoPlugin(IrcBot botInstance) :
            base(botInstance) { }

        public override string Name
        {
            get
            {
                return Assembly.GetExecutingAssembly().FullName;
            }
        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!say", "The command !say <your text>, says whatever text you want it to say", SayHandler, this, CommandScope.Both, "echo_access", null));
            base.Activate();
        }


        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!say");
            base.Deactivate();
        }

        public override string AboutHelp()
        {
            return "This is a very simple Plugin which repeats the message you said";
        }

        private void SayHandler(object sender, IrcEventArgs e)
        {
            BotMethods.SendMessage(SendType.Message, e.Data.Channel, e.Data.Message.Substring(5));
        }
    }
}
