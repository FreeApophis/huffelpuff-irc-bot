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
using Huffelpuff;
using Huffelpuff.Commands;
using Huffelpuff.Plugins;
using SharpIrc;
using SharpIrc.IrcClient.EventArgs;

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
            BotMethods.AddCommand(new Commandlet("!bet", "!bet <money> will bet that amount money.", BetHandler, this, CommandScope.Public));
            BotMethods.AddCommand(new Commandlet("!fold", "With !fold you make an unconditional fold of your cards.", FoldHandler, this, CommandScope.Public));
            BotMethods.AddCommand(new Commandlet("!check", "With !check you are checking the current round.", CheckHandler, this, CommandScope.Public));

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
            if (games[e.Data.Channel].IsPlaying(e.Data.Nick))
            {
                BotMethods.SendMessage(SendType.Notice, e.Data.Nick, "Add Failed. You are already playing in '" + e.Data.Channel + "'");
            }
            else
            {
                games[e.Data.Channel].AddPlayer(e.Data.Nick);
            }
        }

        private void PartGame(object sender, IrcEventArgs e)
        {
            if (games[e.Data.Channel].IsPlaying(e.Data.Nick))
            {
                games[e.Data.Channel].RemovePlayer(e.Data.Nick);
            }
            else
            {
                BotMethods.SendMessage(SendType.Notice, e.Data.Nick, "Remove Failed. You are not playing in '" + e.Data.Channel + "'");
            }
        }

        private void BetHandler(object sender, IrcEventArgs e)
        {
        }

        private void FoldHandler(object sender, IrcEventArgs e)
        {
        }

        private void CheckHandler(object sender, IrcEventArgs e)
        {
        }


        public override string AboutHelp()
        {
            return "Texas Hold'em Poker Plugin";
        }
    }
}
