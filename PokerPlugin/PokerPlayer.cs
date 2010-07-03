﻿/*
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

namespace Plugin
{
    internal class PokerPlayer
    {
        internal PokerPlayer()
        {
            ChipStack = 1000;
            PocketCards = new PocketCards();
        }

        public string Nick { get; set; }
        public string Channel { get; set; }
        public int ChipStack { get; set; }
        public PocketCards PocketCards { get; set; }
        public PlayerState State { get; set; }
    }

    internal enum PlayerState
    {
        NotYetPlaying,
        Dealer,
        BigBlind,
        SmallBlind,
        Player
    }
}
