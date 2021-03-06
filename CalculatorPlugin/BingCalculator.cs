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
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace Plugin
{
    class BingCalculator : ICalculator
    {
        private const string UrlBase = "http://www.bing.com/search?q=";
        private readonly Regex _resultMatch = new Regex("(?<=<div><div><span class=\"sc_bigLine\">).*(?=</span></div></div></div>)", RegexOptions.IgnoreCase);

        public CalculationResult Calculate(string equation)
        {
            try
            {
                var client = new WebClient();
                var eq = HttpUtility.UrlEncode(equation);
                var query = client.DownloadString(UrlBase + eq);
                var match = _resultMatch.Match(query);

                if (match.Success)
                {
                    string result = match.Value;
                    result = ParseSpecial(result);
                    string[] complexSplit = result.Split(':');

                    return complexSplit.Length == 1 ? new CalculationResult(result.Split('=')[1], result, false) : new CalculationResult(complexSplit[1], result, true);
                }
                return new CalculationResult();
            }
            catch (Exception)
            {
                return CalculationResult.NoResult();
            }
        }

        public string ParseSpecial(string result)
        {
            return result;
        }
    }
}
