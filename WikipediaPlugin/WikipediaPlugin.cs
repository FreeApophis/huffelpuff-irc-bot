/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 03.07.2009 18:53
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
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description Wikipedia Plugin.
    /// </summary>
    public class WikipediaPlugin : AbstractPlugin
    {
        public WikipediaPlugin(IrcBot botInstance) : base(botInstance) { }

        public override void Init()
        {
            base.Init();
        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!w", "!w <word> Returns the wikipedia desciption.", WikiHandler, this, CommandScope.Both));

            base.Activate();
        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!w");

            base.Deactivate();
        }

        public override string AboutHelp()
        {
            return "Wikipedia Plugin!!!";
        }

        private void WikiHandler(object sender, IrcEventArgs e)
        {
            var word = new StringBuilder();
            var first = true;
            foreach (var c in e.Data.Message)
            {
                if (first && c == ' ')
                {
                    first = false;
                }
                if (!first)
                {
                    word.Append(c);
                }
            }

            var document = new XmlDocument();
            var request = WebRequest.Create("http://en.wikipedia.org/w/api.php?action=query&prop=revisions&rvprop=content&format=xml&titles=" + word.ToString().Replace(' ', '_')) as HttpWebRequest;

            if (request == null) return;

            request.UserAgent = "Mozilla/5.0 (Huffelpuff)";
            document.Load(request.GetResponse().GetResponseStream());

            if (document.DocumentElement == null) return;

            var text = document.DocumentElement.InnerText;

            BotMethods.SendMessage(SendType.Message, e.Data.Channel, FilterText(text));
        }

        private static string FilterText(string text)
        {
            // Normalize Whitespace, spaces only, makes regex easie
            text = Regex.Replace(text, "\\s", " ");

            // Remove Special
            text = Regex.Replace(text, "{{(.*?)}}", string.Empty);

            // Remove 2 level of [[
            text = Regex.Replace(text, "\\[\\[([^\\]]*?)(\\[\\[.*?\\]\\].*?)+\\]\\]", string.Empty);

            //Remove Image
            text = Regex.Replace(text, "\\[\\[Image(.*?)\\]\\]", string.Empty);

            //Remove Refernces
            text = Regex.Replace(text, "<ref[^<]*?</ref>", string.Empty);

            // Remove Internal links
            text = Regex.Replace(text, "\\[\\[([^\\]]*?)\\|(.*?)\\]\\]", "$2");
            text = Regex.Replace(text, "\\[\\[(.*?)\\]\\]", "$1");

            //Bold text
            text = Regex.Replace(text, "'''(.*?)'''", IrcConstants.IrcBold + "$1" + IrcConstants.IrcNormal);

            // remove everything after the first caption
            text = Regex.Replace(text, "==.*", string.Empty);

            text = text.Trim();
            if (text.Any())
            {
                return text.Length > 300 ? text.Substring(0, 300) : text;
            }
            return "No entry";
        }
    }
}