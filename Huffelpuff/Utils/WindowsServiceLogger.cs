/* 
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
using System.Diagnostics;

namespace Huffelpuff.Utils
{
    public class WindowsServiceLogger : Logger
    {
        public override void Log(string message)
        {
            Log(message, Level.Info);
        }

        public override void Log(string message, Level level)
        {
            if (!IsLogged(Level.Info)) return;

            ServiceEngine.WriteToLog(message, LevelToEventLogEntryType(level));
        }

        private static EventLogEntryType LevelToEventLogEntryType(Level level)
        {
            switch (level)
            {
                case Level.Trace:
                    return EventLogEntryType.Information;
                case Level.Info:
                    return EventLogEntryType.Information;
                case Level.Warning:
                    return EventLogEntryType.Warning;
                case Level.Fail:
                    return EventLogEntryType.Error;
                case Level.Error:
                    return EventLogEntryType.Error;
                case Level.Fatal:
                    return EventLogEntryType.Error;
                default:
                    return EventLogEntryType.Information;
            }
        }

        public override void Log(string message, Level level, ConsoleColor color)
        {
            Log(message, level);
        }

        public override void Log(Exception exception)
        {
            Log(exception.Message, Level.Error);
            if (Verbose)
            {
                Log(exception.StackTrace, Level.Info);
            }
        }

        public override Level MinLogLevel { get; set; }

        public override bool Verbose { get; set; }
    }
}
