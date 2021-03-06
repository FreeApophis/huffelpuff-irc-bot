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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Huffelpuff;
using Huffelpuff.Commands;
using Huffelpuff.Plugins;
using Huffelpuff.Utils;
using SharpIrc;
using SharpIrc.IrcFeatures;

namespace Plugin
{
    /// <summary>
    /// Description Wikipedia Plugin.
    /// </summary>
    public class WikipediaPlugin : AbstractPlugin
    {

        private const string RequestBaseEn = "http://en.wikipedia.org/w/api.php?action=query&prop=revisions&rvprop=content&format=xml&titles=";
        private const string RequestBaseDe = "http://de.wikipedia.org/w/api.php?action=query&prop=revisions&rvprop=content&format=xml&titles=";
        private const string RequestBaseFr = "http://fr.wikipedia.org/w/api.php?action=query&prop=revisions&rvprop=content&format=xml&titles=";
        private const string RequestBaseIt = "http://it.wikipedia.org/w/api.php?action=query&prop=revisions&rvprop=content&format=xml&titles=";

        private readonly List<string> _requestBases = new List<string> { RequestBaseDe, RequestBaseEn, RequestBaseFr, RequestBaseIt };
        public WikipediaPlugin(IrcBot botInstance) : base(botInstance)
        {
        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!w", "!w <word> Returns the wikipedia desciption.", WikiHandler, this));

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


        private const string Redirect = "#REDIRECT";
        private const string NoEntry = "No entry";

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
            foreach (var requestBase in _requestBases)
            {
                while (true)
                {
                    var msg = GetWikiArticle(word, requestBase);
                    BotMethods.SendMessage(SendType.Message, e.SendBackTo(), msg);

                    if (msg == null || !msg.StartsWith(Redirect)) return;

                    if (msg.StartsWith(Redirect))
                    {
                        word = new StringBuilder(msg.Substring(Redirect.Length + 1));
                    }
                    if (msg.StartsWith(NoEntry))
                        break;
                }
            }

            BotMethods.SendMessage(SendType.Message, e.SendBackTo(), NoEntry);
        }

        private static string GetWikiArticle(StringBuilder word, string requestBase)
        {
            var document = new XmlDocument();

            if (WebRequest.Create(requestBase + word.ToString().Replace(' ', '_')) is HttpWebRequest request)
            {
                request.UserAgent = "Mozilla/5.0 (Huffelpuff)";
                document.Load(request.GetResponse().GetResponseStream());

                if (document.DocumentElement == null)
                {
                    return null;
                }

                var text = document.DocumentElement.InnerText;
                return FilterText(text);
            }

            return null;
        }

        private static string FilterText(string text)
        {
            // Normalize Whitespace, spaces only, makes regex easier
            text = Regex.Replace(text, "\\s", " ");

            // Text in another language
            text = Regex.Replace(text, "{{lang\\-([^{}|]*)\\|([^}]*)}}", "$2 ($1)");

            // Remove 2 level of {{
            text = Regex.Replace(text, "{{([^}]*?)({{.*?}}.*?)+}}", string.Empty);

            // Remove Special
            text = Regex.Replace(text, "{{(.*?)}}", string.Empty);

            // Remove 2 level of [[
            text = Regex.Replace(text, "\\[\\[([^\\]]*?)(\\[\\[.*?\\]\\].*?)+\\]\\]", string.Empty);

            //Remove Image
            text = Regex.Replace(text, "\\[\\[Image(.*?)\\]\\]", string.Empty);

            // Remove Internal links
            text = Regex.Replace(text, "\\[\\[([^\\]]*?)\\|(.*?)\\]\\]", "$2");
            text = Regex.Replace(text, "\\[\\[(.*?)\\]\\]", "$1");

            //Remove Refernces
            text = Regex.Replace(text, "<ref[^<]*?</ref>", string.Empty);

            // Remove HTML
            text = Regex.Replace(text, "<[^<]*>", string.Empty);
            text = Regex.Replace(text, "&nbsp;", " ");

            //Bold text
            text = Regex.Replace(text, "'''(.*?)'''", IrcConstants.IrcBold + "$1" + IrcConstants.IrcNormal);
            text = Regex.Replace(text, "''(.*?)''", IrcConstants.IrcUnderline + "$1" + IrcConstants.IrcNormal);

            // remove everything after the first caption
            text = Regex.Replace(text, "==.*", string.Empty);

            text = text.Trim();
            if (text.Any())
            {
                return text.Length > 350 ? text.Substring(0, 350) : text;
            }
            return NoEntry;
        }
    }
}