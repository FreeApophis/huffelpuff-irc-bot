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
    public class ConsoleLogger : Logger
    {
        public override void Log(string message)
        {
            if (!IsLogged(Level.Info)) return;



            Console.WriteLine("{0}@{1}: {2}", Level.Info, location, message);

        }

        public override void Log(string message, Level level)
        {
            if (!IsLogged(level)) return;

            Console.WriteLine("{0}@{1}: {2}", level, location, message);
        }

        public override void Log(string message, Level level, ConsoleColor color)
        {
            if (!IsLogged(level)) return;

            var lastColor = Console.ForegroundColor;
            Console.ForegroundColor = color;

            Console.WriteLine("{0}@{1}: {2}", level, location, message);
            Console.ForegroundColor = lastColor;
        }

        public override void Log(Exception exception)
        {
            if (!IsLogged(Level.Error)) return;

            var lastColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0}@{1}: {2}", Level.Error, location, exception.Message);
            Console.ForegroundColor = lastColor;

            if (Verbose)
            {
                Console.WriteLine("{0}@{1}: {2}", Level.Info, location, exception.StackTrace);
            }
        }

        public override Level MinLogLevel { get; set; }
        public override bool Verbose { get; set; }

        private string location
        {
            get
            {
                var stackTrace = new StackTrace();

                return stackTrace.GetFrame(2).GetMethod().DeclaringType + "." + stackTrace.GetFrame(2).GetMethod().Name;
            }
        }
    }
}
