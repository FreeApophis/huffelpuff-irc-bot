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


using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Plugin
{
    class MagicCalculator
    {
        private readonly List<WebCalculator> calculators = new List<WebCalculator>();
        private readonly List<CalculationResult> results = new List<CalculationResult>();

        public MagicCalculator()
        {
            calculators.Add(new GoogleCalculator());
            calculators.Add(new BingCalculator());
            calculators.Add(new OctaveCalculator());
        }

        public CalculationResult Calculate(string equation)
        {
            CalculateAll(equation);
            foreach (var result in results.Where(result => result.HasResult))
            {
                return result;
            }
            return CalculationResult.NoResult();
        }

        public List<CalculationResult> CalculateAll(string equation)
        {
            results.Clear();
            foreach (var calc in calculators)
            {
                ThreadPool.QueueUserWorkItem(CalulatorThreadStarter, new { Calc = calc, Eq = equation });
            }
            while (results.Count < calculators.Count)
            {
                Thread.Sleep(50);
            }
            return results;

        }

        private void CalulatorThreadStarter(object state)
        {
            var tState = Cast(state, new { Calc = (WebCalculator)new DummyCalculator(), Eq = "" });
            results.Add(tState.Calc.Calculate(tState.Eq));
        }

        private static T Cast<T>(object obj, T t)
        {    
            return (T)obj;
        }

    }
}
