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


using System.Diagnostics;

namespace Plugin
{
    public enum ResultProvider
    {
        Unknown,
        Bing,
        Google,
        Octave
    }

    public class CalculationResult
    {
        public bool HasResult { get; private set; }
        public bool IsComplex { get; private set; }

        public string Result { get; private set; }
        public string CompleteEquation { get; private set; }

        public ResultProvider ResultProvider { get; private set; }


        public CalculationResult()
        {
            SetResultProvider();

            HasResult = false;
            IsComplex = false;
        }

        public CalculationResult(string resultonly, string complete, bool isComplex)
        {
            SetResultProvider();

            Result = resultonly.Trim();
            CompleteEquation = complete.Trim();

            HasResult = true;
            IsComplex = isComplex;
        }

        private void SetResultProvider()
        {
            var trace = new StackTrace();
            var t = trace.GetFrame(2).GetMethod().DeclaringType;
            if (t == typeof(GoogleCalculator))
            {
                ResultProvider = ResultProvider.Google;
            }
            else if (t == typeof(BingCalculator))
            {
                ResultProvider = ResultProvider.Bing;
            }
            else if (t == typeof(OctaveCalculator))
            {
                ResultProvider = ResultProvider.Octave;
            }
            else
            {
                ResultProvider = ResultProvider.Unknown;
            }
        }

        internal static CalculationResult NoResult()
        {
            return new CalculationResult();
        }

        public override string ToString()
        {
            if (!HasResult)
            {
                return "no result";
            }
            return Result + " [" + ResultProvider + "]";
        }
    }
}
