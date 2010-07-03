/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 07.09.2009 00:27
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

        private EventLog appLog;

        public WindowsServiceLogger()
        {
            appLog = new EventLog { Source = "Huffelpuff" };
        }

        public override void Log(string message)
        {
            appLog.WriteEntry(message);
        }

        public override void Log(string message, Level level)
        {
            EventLogEntryType entryType = LevelToEventLogEntryType(level);
            appLog.WriteEntry(message, entryType);
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
            EventLogEntryType entryType = LevelToEventLogEntryType(level);
            appLog.WriteEntry(message, entryType);
        }

        public override Level MinLogLevel { get; set; }
    }
}
