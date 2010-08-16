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

namespace Plugin
{
    internal class PokerGame
    {
        internal PokerGame()
        {
            players = new Queue<PokerPlayer>();
            State = GameState.CleanBoard;
            deck = new RandomDeck();
        }

        public GameState Next()
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
            return State;
        }


        private void Start()
        {
            deck.InitDeck();

            foreach (var player in players.Where(player => player.State != PlayerState.NotYetPlaying))
            {
                deck.NextCard();
                player.PocketCards.Card1 = deck.CurrentCard;

                deck.NextCard();
                player.PocketCards.Card2 = deck.CurrentCard;
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

        public GameState State { get; private set; }

        private readonly Queue<PokerPlayer> players;

        public ReadOnlyCollection<PokerPlayer> Players
        {
            get { return new ReadOnlyCollection<PokerPlayer>(players.ToList()); }
        }

        internal void NextDealer()
        {
            players.Enqueue(players.Dequeue());
        }

        public enum GameState
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
