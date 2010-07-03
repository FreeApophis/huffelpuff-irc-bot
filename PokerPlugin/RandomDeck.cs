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
using HoldemHand;

namespace Plugin
{
    internal class RandomDeck
    {
        private List<string> cards;
        private readonly Random random;

        public string CurrentCard { get; private set; }

        internal RandomDeck()
        {
            cards = new List<string>(Hand.CardTable);
            random = new Random();
        }

        public void InitDeck()
        {
            cards = new List<string>(Hand.CardTable);
            CurrentCard = null;
        }

        public void NextCard()
        {
            if (cards.Count == 0)
            {
                CurrentCard = null;
            }
            else
            {
                CurrentCard = cards[random.Next(cards.Count)];
                cards.Remove(CurrentCard);
            }
        }
    }
}
