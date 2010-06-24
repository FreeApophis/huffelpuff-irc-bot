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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using HoldemHand;
using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
    public class PokerPlugin : AbstractPlugin
    {
        private readonly Dictionary<char, char> suits = new Dictionary<char, char> { { 's', '♠' }, { 'c', '♣' }, { 'h', '♥' }, { 'd', '♦' } };

        private readonly RandomDeck deck = new RandomDeck();

        private string ToIrcCard(string card)
        {

            if (card.Length == 2)
            {
                string value = card[0] == 'T' ? "10" : card[0].ToString();

                if (card[1] == 's' || card[1] == 'c')
                {
                    return IrcConstants.IrcColor + ((int)IrcColors.Black).ToString("00") + value + suits[card[1]] + IrcConstants.IrcNormal;
                }
                if (card[1] == 'h' || card[1] == 'd')
                {
                    return IrcConstants.IrcColor + ((int)IrcColors.LightRed).ToString("00") + value + suits[card[1]] + IrcConstants.IrcNormal;
                }
            }
            return null;
        }

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
            if (e.Data.MessageArray.Length > 1)
            {
                //BotMethods.SendMessage(SendType.Message, destination, Hand.MaskToDescription(Hand.ParseHand(e.Data.MessageArray[1])));
                BotMethods.SendMessage(SendType.Message, destination, Hand.DescriptionFromHand(e.Data.MessageArray[1]));
            }
            else
            {
                var hand = new StringBuilder();
                var mask = new StringBuilder();
                foreach (var cards in Enumerable.Range(0, 5))
                {
                    deck.NextCard();
                    hand.Append(ToIrcCard(deck.CurrentCard));
                    mask.Append(deck.CurrentCard);
                }


                BotMethods.SendMessage(SendType.Message, destination, hand + " is " + Hand.DescriptionFromHand(mask.ToString()));
            }
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
