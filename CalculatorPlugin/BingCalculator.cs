using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Plugin
{
    class BingCalculator : WebCalculator
    {
        private const string urlBase = "http://www.bing.com/search?q=";
        private Regex resultMatch = new Regex("(?<=<div><div><span class=\"sc_bigLine\">).*(?=</span></div></div></div>)", RegexOptions.IgnoreCase);

        public override CalculationResult Calculate(string equation)
        {
            try {
                WebClient client = new WebClient();
                string eq = HttpUtility.UrlEncode(equation);
                string query = client.DownloadString(urlBase + eq);
                Match match = resultMatch.Match(query);

                if (match.Success)
                {
                    string result = match.Value;
                    result = parseSpecial(result);
                    string[] complexSplit = result.Split(new char[] { ':' });
                    if (complexSplit.Length == 1)
                    {
                        return new CalculationResult(result.Split(new char[] { '=' })[1], result, false);
                    }
                    else
                    {
                        return new CalculationResult(complexSplit[1], result, true);
                    }
                }
                else
                {
                    return new CalculationResult();
                }
            } catch (Exception) {
                return CalculationResult.NoResult();
            }
        }

        public string parseSpecial(string result)
        {
            return result;
        }
    }
}
