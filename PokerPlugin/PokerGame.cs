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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Huffelpuff;
using Huffelpuff.Utils;
using Meebey.SmartIrc4net;

namespace Plugin
{
    internal class PokerGame
    {
        private readonly IrcBot bot;
        private readonly string channel;
        private bool run;

        internal string Channel
        {
            get
            {
                return channel;
            }
        }

        private readonly Dictionary<char, char> suits = new Dictionary<char, char> { { 's', '♠' }, { 'c', '♣' }, { 'h', '♥' }, { 'd', '♦' } };

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


        internal PokerGame(IrcBot bot, string channel)
        {
            players = new Queue<PokerPlayer>();
            State = GameState.CleanBoard;
            deck = new RandomDeck();
            run = false;

            this.bot = bot;
            this.channel = channel;
        }

        private void ResetGame()
        {
            State = GameState.CleanBoard;
            deck.InitDeck();
            run = false;
        }

        private const int MaxPlayers = 10;

        private void Run()
        {
            run = true;

            while (run)
            {
                Next();
            }
        }

        private void Next()
        {
            switch (State)
            {
                case GameState.CleanBoard:
                    Start();
                    State = GameState.PreFlop;
                    break;
                case GameState.PreFlop:
                    BetRound();
                    State = GameState.Flop;
                    break;
                case GameState.Flop:
                    BetRound();
                    State = GameState.Turn;
                    break;
                case GameState.Turn:
                    BetRound();
                    State = GameState.River;
                    break;
                case GameState.River:
                    PotPayOut();
                    State = GameState.PayOut;
                    break;
                case GameState.PayOut:
                    State = GameState.CleanBoard;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private void Start()
        {
            deck.InitDeck();

            foreach (var player in players.Where(player => player.State == PlayerState.NotYetPlaying))
            {
                player.State = PlayerState.Playing;
            }

            foreach (var line in players.Where(p => p.State == PlayerState.Playing).Select(p => "%1 (%2, %3)".Fill(p.Nick, p.ChipStack, p.State)).ToLines(350, ", ", "{0}New Round{0}: Playing: ".Fill(IrcConstants.IrcBold), ""))
            {
                bot.SendMessage(SendType.Notice, channel, line);
            }

            foreach (var player in players)
            {
                deck.NextCard();
                player.PocketCards.Card1 = deck.CurrentCard;

                deck.NextCard();
                player.PocketCards.Card2 = deck.CurrentCard;

                bot.SendMessage(SendType.Message, player.Nick, "{0}New Round{0}: Your Chips: {3} - Your Hole Cards: {1} {2}"
                    .Fill(IrcConstants.IrcBold, ToIrcCard(player.PocketCards.Card1), ToIrcCard(player.PocketCards.Card2), player.ChipStack));
            }
        }

        internal bool AddPlayer(string nick)
        {
            bool result = false;
            var before = players.Where(p => p.State != PlayerState.PausedPlaying).Count();

            if (players.Count < MaxPlayers && !players.Any(p => p.Nick == nick))
            {
                players.Enqueue(new PokerPlayer(nick));
                result = true;
            }

            var after = players.Where(p => p.State != PlayerState.PausedPlaying).Count();

            if (before == 1 && after == 2)
            {
                //We have two players and can start the game now
                bot.SendMessage(SendType.Message, channel, "The poker table in channel '{0}' has started! To participate join the table with !poker-join, to leave the table use !poker-leave".Fill(channel));
                Run();
            }

            return result;
        }

        internal void RemovePlayer(string nick)
        {
            var before = players.Where(p => p.State != PlayerState.PausedPlaying).Count();

            players = new Queue<PokerPlayer>(players.Where(p => p.Nick != nick));

            var after = players.Where(p => p.State != PlayerState.PausedPlaying).Count();

            if (before == 2 && after == 1)
            {
                //Game must end, not enough players
                ResetGame();
            }
        }

        private void BetRound()
        {
            throw new NotImplementedException();
        }

        private void PotPayOut()
        {
            throw new NotImplementedException();
        }

        private readonly RandomDeck deck;

        internal GameState State { get; private set; }

        private Queue<PokerPlayer> players;

        internal ReadOnlyCollection<PokerPlayer> Players
        {
            get { return new ReadOnlyCollection<PokerPlayer>(players.ToList()); }
        }

        internal void NextDealer()
        {
            players.Enqueue(players.Dequeue());
        }

        internal enum GameState
        {
            CleanBoard,
            PreFlop,
            Flop,
            Turn,
            River,
            PayOut
        }
    }
}
