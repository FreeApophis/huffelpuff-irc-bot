using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace Plugin
{
    class OctaveCalculator : WebCalculator
    {
        private const string urlBase = "http://hara.mimuw.edu.pl/weboctave/web/index.php";
        private Regex resultMatch = new Regex("(?<=gt;</span>).*(?=</pre><p class=\"msg\">)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private Regex whiteSpaceMatch = new Regex(@"\s+");
        public override CalculationResult Calculate(string equation)
        {
            string requestString = "submit=Submit%20to%20Octave&commands=" + HttpUtility.UrlEncode(equation);

            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(requestString);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(urlBase);
            request.ServicePoint.Expect100Continue = false;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();

            StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream());
            string query = reader.ReadToEnd();


            Match match = resultMatch.Match(query);

            string[] lines = match.Value.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length > 1)
            {
                if (lines.Length > 2)
                {
                    string result = ""; int i = 0;
                    if (!lines[1].StartsWith("ans"))
                    {
                        result = lines[1]+" ";
                    }
                    foreach (string line in lines)
                    {
                        if (i > 2)
                        {
                            result += ",[" + whiteSpaceMatch.Replace(line.Trim(), ",") + "]";
                        }
                        else if (i > 1)
                        {
                            result += "[" + whiteSpaceMatch.Replace(line.Trim(), ",") + "]";
                        }
                        i++;
                    }
                    return new CalculationResult(result, lines[0] + " => " + result, true);
                }
                else
                {
                    string[] splitres = lines[1].Split(new char[] { '=' });
                    return new CalculationResult(splitres[1], lines[0] + " => " + RemoveAns(lines[1]), false);
                }

            }
            else
            {
                return new CalculationResult();
            }
        }

        private string RemoveAns(string res)
        {
            return res.Replace("ans = ", "");
        }

        public string parseSpecial(string result)
        {
            return result;
        }

    }
}
