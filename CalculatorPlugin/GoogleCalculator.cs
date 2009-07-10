using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace Plugin
{
    class GoogleCalculator : WebCalculator
    {
        private const string urlBase = "http://www.google.ch/search?num=1&q=";
        private Regex resultMatch = new Regex("(?<=<h2 class=r style=\"font-size:138%\"><b>).*(?=</b></h2>)", RegexOptions.IgnoreCase);

        public override CalculationResult Calculate(string equation)
        {
            WebClient client = new WebClient();
            string eq = HttpUtility.UrlEncode(equation);
            string query = client.DownloadString(urlBase + eq);
            Match match = resultMatch.Match(query);

            if (match.Success)
            {
                string result = match.Value;
                result = parseSpecial(result);
                string[] splitres = result.Split(new char[] { '=' });
                return new CalculationResult(splitres[1], result, false);
            }
            else
            {
                return new CalculationResult();
            }
        }

        public string parseSpecial(string result)
        {
            StringBuilder resultBuilder = new StringBuilder(result);
            resultBuilder.Replace("&#215;", "*");
            resultBuilder.Replace("<sup>", "^");
            resultBuilder.Replace("</sup>", "");
            return resultBuilder.ToString();
        }
    }
}
