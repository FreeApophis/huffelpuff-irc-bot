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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Plugin
{
    class OctaveCalculator : WebCalculator
    {
        private const string URLBase = "http://weboctave.mimuw.edu.pl/weboctave/web/index.php";
        private readonly Regex resultMatch = new Regex("ans.*(?=</pre><p class=\"msg\">)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private readonly Regex whiteSpaceMatch = new Regex(@"\s+");
        public override CalculationResult Calculate(string equation)
        {
            try
            {
                var requestString = "submit=Submit%20to%20Octave&commands=" + HttpUtility.UrlEncode(equation);

                var encoding = new ASCIIEncoding();
                byte[] data = encoding.GetBytes(requestString);

                var request = (HttpWebRequest)WebRequest.Create(URLBase);
                request.ServicePoint.Expect100Continue = false;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                var stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Flush();

                var reader = new StreamReader(request.GetResponse().GetResponseStream());
                var query = reader.ReadToEnd();


                var match = resultMatch.Match(query);

                var lines = match.Value.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length > 0)
                {
                    if (lines.Length > 1)
                    {
                        var result = ""; int i = 0;
                        foreach (var line in lines.Skip(1))
                        {
                            if (i == 0)
                            {
                                result += "[" + whiteSpaceMatch.Replace(line.Trim(), ",") + "]";
                            }
                            else
                            {
                                result += ",[" + whiteSpaceMatch.Replace(line.Trim(), ",") + "]";
                            }
                            i++;
                        }
                        return new CalculationResult(result, equation + " = " + result, true);
                    }
                    var splitres = lines[0].Split(new[] { '=' });
                    return new CalculationResult(splitres[1], equation + " = " + RemoveAns(lines[0]), false);
                }
                return CalculationResult.NoResult();
            }
            catch (Exception)
            {
                return CalculationResult.NoResult();
            }
        }

        private static string RemoveAns(string res)
        {
            return res.Replace("ans = ", "");
        }

        public string ParseSpecial(string result)
        {
            return result;
        }

    }
}
