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
using SharpIrc;
using SharpIrc.IrcFeatures;
using Huffelpuff;
using Huffelpuff.Utils;

namespace Plugin
{
    internal class PokerGame
    {
        private readonly IrcBot _bot;
        private readonly string _channel;
        private bool _run;

        internal string Channel => _channel;

        private readonly Dictionary<char, char> _suits = new Dictionary<char, char> { { 's', '♠' }, { 'c', '♣' }, { 'h', '♥' }, { 'd', '♦' } };

        private string ToIrcCard(string card)
        {

            if (card.Length == 2)
            {
                string value = card[0] == 'T' ? "10" : card[0].ToString();

                if (card[1] == 's' || card[1] == 'c')
                {
                    return IrcConstants.IrcColor + ((int)IrcColors.Black).ToString("00") + value + _suits[card[1]] + IrcConstants.IrcNormal;
                }
                if (card[1] == 'h' || card[1] == 'd')
                {
                    return IrcConstants.IrcColor + ((int)IrcColors.LightRed).ToString("00") + value + _suits[card[1]] + IrcConstants.IrcNormal;
                }
            }
            return null;
        }


        internal PokerGame(IrcBot bot, string channel)
        {
            _players = new Queue<PokerPlayer>();
            State = GameState.CleanBoard;
            _deck = new RandomDeck();
            _run = false;

            _bot = bot;
            _channel = channel;
        }

        private void ResetGame()
        {
            State = GameState.CleanBoard;
            _deck.InitDeck();
            _run = false;
        }

        private const int MaxPlayers = 10;

        private void Run()
        {
            _run = true;

            while (_run)
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
            _deck.InitDeck();

            foreach (var player in _players.Where(player => player.State == PlayerState.NotYetPlaying))
            {
                player.State = PlayerState.Playing;
            }

            foreach (var line in _players.Where(p => p.State == PlayerState.Playing).Select(p => "%1 (%2, %3)".Fill(p.Nick, p.ChipStack, p.State)).ToLines(350, ", ", "{0}New Round{0}: Playing: ".Fill(IrcConstants.IrcBold), ""))
            {
                _bot.SendMessage(SendType.Notice, _channel, line);
            }

            foreach (var player in _players)
            {
                _deck.NextCard();
                player.PocketCards.Card1 = _deck.CurrentCard;

                _deck.NextCard();
                player.PocketCards.Card2 = _deck.CurrentCard;

                _bot.SendMessage(SendType.Message, player.Nick, "{0}New Round{0}: Your Chips: {3} - Your Hole Cards: {1} {2}"
                    .Fill(IrcConstants.IrcBold, ToIrcCard(player.PocketCards.Card1), ToIrcCard(player.PocketCards.Card2), player.ChipStack));
            }
        }

        internal bool AddPlayer(string nick)
        {
            bool result = false;
            var before = _players.Count(p => p.State != PlayerState.PausedPlaying);

            if (_players.Count < MaxPlayers && _players.All(p => p.Nick != nick))
            {
                _players.Enqueue(new PokerPlayer(nick));
                result = true;
            }

            var after = _players.Count(p => p.State != PlayerState.PausedPlaying);

            if (before == 1 && after == 2)
            {
                //We have two players and can start the game now
                _bot.SendMessage(SendType.Message, _channel, "The poker table in channel '{0}' has started! To participate join the table with !poker-join, to leave the table use !poker-leave".Fill(_channel));
                Run();
            }

            return result;
        }

        internal void RemovePlayer(string nick)
        {
            var before = _players.Count(p => p.State != PlayerState.PausedPlaying);

            _players = new Queue<PokerPlayer>(_players.Where(p => p.Nick != nick));

            var after = _players.Count(p => p.State != PlayerState.PausedPlaying);

            if (before == 2 && after == 1)
            {
                //Game must end, not enough players
                ResetGame();
            }
        }

        internal bool IsPlaying(string nick)
        {
            return _players.Any(p => p.Nick == nick);
        }

        private void BetRound()
        {
            throw new NotImplementedException();
        }

        private void PotPayOut()
        {
            throw new NotImplementedException();
        }

        private readonly RandomDeck _deck;

        internal GameState State { get; private set; }

        private Queue<PokerPlayer> _players;

        internal ReadOnlyCollection<PokerPlayer> Players => new ReadOnlyCollection<PokerPlayer>(_players.ToList());

        internal void NextDealer() => _players.Enqueue(_players.Dequeue());

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
