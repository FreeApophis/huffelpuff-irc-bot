/*
 *  Wolfram Alpha Plugin
 *  ---------------------------------------------------------
 * 
 *  Copyright (c) 2011 Thomas Bruderer <apophis@apophis.ch>
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

using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using apophis.SharpIRC;
using Huffelpuff;
using Huffelpuff.Plugins;

namespace Plugin
{
    public class WolframAlphaPlugin : AbstractPlugin
    {

        private const string AppID = "U9E8AG-Q6R9G343K3";
        private const string RequestBase = "http://api.wolframalpha.com/v2/query?";
        private const string InputQuery = "input=";
        private const string AppIDQuery = "appid=" + AppID;
        private const string AmpersAnd = "&";

        public WolframAlphaPlugin(IrcBot botInstance) :
            base(botInstance) { }

        public override string AboutHelp()
        {
            return "This is the help about the whole CommentedExamplePlugin";
        }


        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!alpha", "Wolfram Alpha", HandleAlpha, this));

            base.Activate();
        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!alpha");

            base.Deactivate();
        }

        public void HandleAlpha(object sender, IrcEventArgs e)
        {
            string target = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            var document = new XDocument();
            var request = WebRequest.Create(RequestBase + InputQuery + HttpUtility.UrlEncode(string.Join(" ", e.Data.MessageArray.Skip(1))) + AmpersAnd + AppIDQuery) as HttpWebRequest;
            if (request == null) return;

            request.UserAgent = "Mozilla/5.0 (Huffelpuff)";
            document = XDocument.Load(request.GetResponse().GetResponseStream());

            foreach (var pod in document.Descendants("pod"))
            {
                string id = pod.Attribute("id").Value;
                string title = pod.Attribute("title").Value;

                foreach (var subpod in pod.Descendants("subpod"))
                {
                    string plaintext = subpod.Descendants("plaintext").First().Value.Replace("\n", " ");
                    if (!string.IsNullOrWhiteSpace(plaintext))
                    {
                        BotMethods.SendMessage(SendType.Message, target, title + ": " + plaintext);
                    }
                }
            }
        }
    }
}