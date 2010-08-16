﻿/*
 *  The Huffelpuff Irc Bot, versatile pluggable bot for IRC chats
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

namespace Huffelpuff.Utils
{
    public class NullLogger : Logger
    {
        public override void Log(string message)
        {
            return;
        }

        public override void Log(string message, Level level)
        {
            return;
        }

        public override void Log(string message, Level level, ConsoleColor color)
        {
            return;
        }

        public override void Log(Exception exception)
        {
            return;
        }

        public override Level MinLogLevel { get; set; }
        public override bool Verbose { get; set; }
    }
}
