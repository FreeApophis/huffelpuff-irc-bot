/*
 *  Commented Example Plugin, as a help for Plugin developers
 *  ---------------------------------------------------------
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 03.07.2009 18:54
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

/* 
 * To write a Plugin you need to set a Reference to the Huffelpuff-project 
 * and also to the SharpIRC Project.
 * If you work with the source, make a new Project for your Plugin and add
 * project reference to the Project.
 * If you work with the executable directly, Add a reference to the
 * Huffelpuff.exe And SharpIRC.dll
 * 
 * normally you need the following namesspaces from the project:
 */
using apophis.SharpIRC;
using Huffelpuff;
using Huffelpuff.Plugins;
using Protocols.Mumble;


namespace Plugin
{
    public class MumblePlugin : AbstractPlugin
    {
        private MumbleClient client;

        public MumblePlugin(IrcBot botInstance) :
            base(botInstance) { }

        public override string AboutHelp()
        {
            return "Mumble Plugin can connect to the MumbleServer";
        }

        public override void Init()
        {
            TickInterval = 60;

            client = new MumbleClient("huffelpuff");

            base.Init();
        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!mumble", "Testing", MumbleHandler, this, CommandScope.Both));

            base.Activate();
        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!mumble");

            base.Deactivate();
        }

        public override void DeInit()
        {
            base.DeInit();
        }

        public override void OnTick()
        {
        }

        private void MumbleHandler(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            client.Connect();

            BotMethods.SendMessage(SendType.Message, sendto, "Mumble Connect");
        }
    }
}