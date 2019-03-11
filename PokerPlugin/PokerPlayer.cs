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

namespace Plugin
{
    internal class PokerPlayer
    {
        internal PokerPlayer(string nick)
        {
            ChipStack = 1000;
            PocketCards = new PocketCards();
            Nick = nick;
        }

        internal string Nick { get; }
        internal string Channel { get; set; }
        internal int ChipStack { get; set; }
        internal PocketCards PocketCards { get; set; }
        internal PlayerState State { get; set; }
        internal bool Dealer;
        internal bool BigBlind;
        internal bool SmallBlind;
    }

    internal enum PlayerState
    {
        NotYetPlaying,
        Playing,
        PausedPlaying
    }
}
