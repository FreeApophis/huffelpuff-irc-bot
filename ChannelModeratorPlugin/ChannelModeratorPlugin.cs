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
using SharpIrc;
using Huffelpuff;
using Huffelpuff.Commands;
using Huffelpuff.Plugins;

namespace Plugin
{
    /// <summary>
    /// Ready for kick, op and war ;)
    /// </summary>
    public class ChannelModeratorPlugin : AbstractPlugin
    {
        public ChannelModeratorPlugin(IrcBot botInstance) :
            base(botInstance)
        { }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!kick", "kicks an annyoing user", KickUser, this));
            BotMethods.AddCommand(new Commandlet("!ban", "ban an annyoing user", BanUser, this));

            base.Activate();
        }

        private void BanUser(object sender, IrcEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!kick");
            BotMethods.RemoveCommand("!ban");

            base.Deactivate();
        }

        public override string AboutHelp()
        {
            return "The channel moderator plugins, manages your channel...";
        }

        private void KickUser(object sender, IrcEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
