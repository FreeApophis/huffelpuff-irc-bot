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
        public PokerPlugin(IrcBot botInstance)
            : base(botInstance)
        {

        }


        readonly Dictionary<string, PokerGame> games = new Dictionary<string, PokerGame>();
        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!poker-join", "Join the Pokertable and participate in the game the next round.", JoinGame, this, CommandScope.Public));
            BotMethods.AddCommand(new Commandlet("!poker-leave", "Stand up and leave the table, this also happens when you Part the Channel or Quit.", PartGame, this, CommandScope.Public));
            BotMethods.AddCommand(new Commandlet("!bet", "!bet <money> will bet that amount money.", EvaluateHand, this, CommandScope.Public));
            BotMethods.AddCommand(new Commandlet("!fold", "With !fold you make an unconditional fold of your cards.", EvaluateHand, this, CommandScope.Public));
            BotMethods.AddCommand(new Commandlet("!check", "With !check you are checking the current round.", EvaluateHand, this, CommandScope.Public));

            foreach (var channel in BotMethods.GetChannels())
            {
                games.Add(channel, new PokerGame(BotMethods, channel));
            }

            BotEvents.OnJoin += OnJoin;
            BotEvents.OnPart += OnPart;
            base.Activate();
        }

        public override void Deactivate()
        {
            games.Clear();

            BotMethods.RemoveCommand("!poker-join");
            BotMethods.RemoveCommand("!poker-leave");
            BotMethods.RemoveCommand("!bet");
            BotMethods.RemoveCommand("!fold");
            BotMethods.RemoveCommand("!check");

            BotEvents.OnJoin -= OnJoin;
            BotEvents.OnPart -= OnPart;
            base.Deactivate();
        }

        void OnJoin(object sender, JoinEventArgs e)
        {
            if (e.Who == BotMethods.Nickname)
            {
                games.Add(e.Channel, new PokerGame(BotMethods, e.Channel));
            }
        }

        void OnPart(object sender, PartEventArgs e)
        {
            if (e.Who == BotMethods.Nickname)
            {
                games.Remove(e.Channel);
            }
        }

        private void JoinGame(object sender, IrcEventArgs e)
        {
            games[e.Data.Channel].AddPlayer(e.Data.Nick);
        }

        private void PartGame(object sender, IrcEventArgs e)
        {
            games[e.Data.Channel].RemovePlayer(e.Data.Nick);
        }

        private void EvaluateHand(object sender, IrcEventArgs e)
        {
            //var destination = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            //if (e.Data.MessageArray.Length > 1)
            //{
            //    //BotMethods.SendMessage(SendType.Message, destination, Hand.MaskToDescription(Hand.ParseHand(e.Data.MessageArray[1])));
            //    BotMethods.SendMessage(SendType.Message, destination, Hand.DescriptionFromHand(e.Data.MessageArray[1]));
            //}
            //else
            //{
            //    var hand = new StringBuilder();
            //    var mask = new StringBuilder();
            //    foreach (var cards in Enumerable.Range(0, 5))
            //    {
            //        deck.NextCard();
            //        hand.Append(ToIrcCard(deck.CurrentCard));
            //        mask.Append(deck.CurrentCard);
            //    }


            //    BotMethods.SendMessage(SendType.Message, destination, hand + " is " + Hand.DescriptionFromHand(mask.ToString()));
            //}
        }



        public override string AboutHelp()
        {
            return "";
        }
    }
}
