/*
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 14.09.2009 19:02
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
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Plugin
{
    class GoogleCalculator : WebCalculator
    {
        private const string URLBase = "http://www.google.ch/search?num=1&q=";
        private readonly Regex resultMatch = new Regex("(?<=<h2 class=r style=\"font-size:138%\"><b>).*(?=</b></h2>)", RegexOptions.IgnoreCase);

        public override CalculationResult Calculate(string equation)
        {
            try {
                var client = new WebClient();
                var eq = HttpUtility.UrlEncode(equation);
                var query = client.DownloadString(URLBase + eq);
                var match = resultMatch.Match(query);

                if (match.Success)
                {
                    var result = match.Value;
                    result = ParseSpecial(result);
                    var splitres = result.Split(new[] { '=' });
                    return new CalculationResult(splitres[1], result, false);
                }
                return new CalculationResult();
            } catch (Exception) {
                return CalculationResult.NoResult();
            }
        }

        public string ParseSpecial(string result)
        {
            var resultBuilder = new StringBuilder(result);

            resultBuilder.Replace("&#215;", "*");
            resultBuilder.Replace("<sup>", "^");
            resultBuilder.Replace("</sup>", "");
            resultBuilder.Replace("<font size=-2> </font>", "");

            return resultBuilder.ToString();
        }
    }
}
