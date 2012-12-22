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

using apophis.SharpIRC;
using Huffelpuff;
using Huffelpuff.Plugins;

namespace Plugin
{
    /// <summary>
    /// FileServer Plugin
    /// </summary>
    public class FileServerPlugin : AbstractPlugin
    {

        public FileServerPlugin(IrcBot botInstance) : base(botInstance) { }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!download", "Starts a DCC Filetransfer to you", SendFile, this));
            base.Activate();
        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!download");
            base.Deactivate();
        }


        public override string AboutHelp()
        {
            return "File Plugin Help";
        }

        public void SendFile(object sender, IrcEventArgs e)
        {
            BotMethods.SendFile(e.Data.Nick, "./Huffelpuff.exe");
        }
    }
}