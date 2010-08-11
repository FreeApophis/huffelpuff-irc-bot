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
using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class CalculatorPlugin : AbstractPlugin
    {
        public CalculatorPlugin(IrcBot botInstance) : base(botInstance) { }

        private MagicCalculator magicCalculator;

        public override string AboutHelp()
        {
            return "This Plugin will do complex calculation for you. The Services are called via a webinterface, multiple services get called, therefore different Problems can be solved, and there should be always a solution to simple problems";
        }

        public override void Init()
        {
            magicCalculator = new MagicCalculator();
            base.Init();
        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!calc", "!calc <function> calculates plenty of different equations, different backends are used. Here are some working Examples: 3*3*3, 20!, 1km^3 in teaspoons, x*(x+2)=15, 2*Pi, 1 USD in CHF, three times four, A = [1, 2, 3]; B=4; A*B, unidrnd(10), S = [2,1,4;3,2,2;-2,2,3]; D = diag(diag(S),0)", CalculateHandler, this));
            BotMethods.AddCommand(new Commandlet("!eval", "!eval <function> calculates plenty of different equations, different backends are used. eval returns not only the result but the whole equation again. Here are some working Examples: 3*3*3, 20!, 1km^3 in teaspoons, x*(x+2)=15, 2*Pi, 1 USD in CHF, three times four, A = [1, 2, 3]; B=4; A*B, unidrnd(10), S = [2,1,4;3,2,2;-2,2,3]; D = diag(diag(S),0)", EvaluateHandler, this));

            base.Activate();
        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!calc");
            BotMethods.RemoveCommand("!eval");

            base.Deactivate();
        }

        private void CalculateHandler(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            try
            {
                var result = magicCalculator.Calculate(e.Data.Message.Substring(6));
                if (result.HasResult)
                {
                    BotMethods.SendMessage(SendType.Message, sendto, "[" + result.ResultProvider.ToString()[0] + "] Result: " + result.Result);
                    return;
                }
            }
            catch (Exception) { }
            BotMethods.SendMessage(SendType.Message, sendto, "no result!");
        }

        private void EvaluateHandler(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            try
            {
                var result = magicCalculator.Calculate(e.Data.Message.Substring(6));
                if (result.HasResult)
                {
                    BotMethods.SendMessage(SendType.Message, sendto, "[" + result.ResultProvider.ToString()[0] + "] Result: " + result.CompleteEquation);
                    return;
                }
            }
            catch (Exception) { }
            BotMethods.SendMessage(SendType.Message, sendto, "no result!");
        }

    }
}