/*
 *  The Poker Plugin to deal some Poker games
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



using HoldemHand;
using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace PlugIn
{
    public class PokerPlugin : AbstractPlugin
    {
        public PokerPlugin(IrcBot botInstance)
            : base(botInstance)
        {

        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!hand", "What Hand is that?", EvaluateHand, this, CommandScope.Both));

            base.Activate();
        }

        private void EvaluateHand(object sender, IrcEventArgs e)
        {
            var destination = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            //BotMethods.SendMessage(SendType.Message, destination, Hand.MaskToDescription(Hand.ParseHand(e.Data.MessageArray[1])));
            BotMethods.SendMessage(SendType.Message, destination, Hand.DescriptionFromHand(e.Data.MessageArray[1]));
            Hand.HandTypes.

        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!hand");

            base.Deactivate();
        }

        public override string AboutHelp()
        {
            return "";
        }
    }
}
