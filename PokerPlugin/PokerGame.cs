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

namespace Plugin
{
    internal class PokerGame
    {
        internal PokerGame()
        {
            players = new List<PokerPlayer>();
            State = GameState.CleanBoard;
            deck = new RandomDeck();
        }

        public GameState Next()
        {
            switch (State)
            {
                case GameState.CleanBoard:
                    Start();
                    break;
                case GameState.PocketCards:
                    break;
                case GameState.Flop:
                    break;
                case GameState.Turn:
                    break;
                case GameState.River:
                    break;
                case GameState.PayOut:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return State;
        }

        private void Start()
        {
            deck.InitDeck();

            foreach (var player in players)
            {
                deck.NextCard();
                player.PocketCards.Card1 = deck.CurrentCard;

                deck.NextCard();
                player.PocketCards.Card2 = deck.CurrentCard;
            }

            State = GameState.PocketCards;
        }

        private readonly RandomDeck deck;

        public GameState State { get; private set; }

        private readonly List<PokerPlayer> players;

        public List<PokerPlayer> Players
        {
            get { return players; }
        }

        public enum GameState
        {
            CleanBoard,
            PocketCards,
            Flop,
            Turn,
            River,
            PayOut
        }
    }
}
