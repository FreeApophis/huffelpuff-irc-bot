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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using SharpIrc;
using Huffelpuff;
using Huffelpuff.Plugins;
using Huffelpuff.Utils;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class UrlToTitlePlugin : AbstractPlugin
    {
        public UrlToTitlePlugin(IrcBot botInstance) : base(botInstance) { }


        public override string AboutHelp()
        {
            return "If an Url is sent on a single line the Bot tries to get the Title of the page and posts it to the channel";
        }

        public override void Activate()
        {
            BotEvents.OnChannelMessage += BotEventsOnChannelMessage;

            base.Activate();
        }

        public override void Deactivate()
        {
            BotEvents.OnChannelMessage -= BotEventsOnChannelMessage;

            base.Deactivate();
        }

        private readonly Regex _titleMatch = new Regex("(?<=<title>)[^<]*(?=</title>)", RegexOptions.IgnoreCase);
        private readonly Regex _fMatch = new Regex("(?<=f=)[0-9]*");
        private readonly Regex _tMatch = new Regex("(?<=t=)[0-9]*");
        private readonly Regex charsetMatch = new Regex("(?<=charset=)[^(;\" )]*", RegexOptions.IgnoreCase);

        private readonly Regex _whiteSpaceMatch = new Regex(@"\s+");

        private void BotEventsOnChannelMessage(object sender, IrcEventArgs e)
        {
            try
            {
                string lang = (e.Data.Channel == "#piraten-schweiz") ? "de" : "en";
                lang = (e.Data.Channel == "#pirates-suisse") ? "fr" : lang;

                if (e.Data.MessageArray.Length == 1)
                {
                    if (e.Data.Message.StartsWith("http://forum.piratenpartei.ch/viewtopic.php?f="))
                    {
                        var f = _fMatch.Match(e.Data.Message).Value;
                        var t = _tMatch.Match(e.Data.Message).Value;
                        var item = GetTopic("http://forum.piratenpartei.ch/rss.php?f=" + f + "&t=" + t + "&start=last", lang, false);
                        if (lang == "fr")
                        {
                            BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Forum des pirates - " + item.Title + " (by " + item.Author + ")");
                        }
                        else
                        {
                            BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Piratenforum - " + item.Title + " (by " + item.Author + ")");
                        }
                    }
                    else if (e.Data.Message.StartsWith("http://"))
                    {
                        var client = new WebClient();

                        string page = client.DownloadString(e.Data.Message);
                        string charset = charsetMatch.Match(page).Value;
                        string title = _whiteSpaceMatch.Replace(_titleMatch.Match(page).Value, " ");

                        if (charset.ToLower() != "")
                        {
                            var enc = Encoding.GetEncoding(charset);
                            var encBytes = client.Encoding.GetBytes(title);
                            var utfBytes = Encoding.Convert(enc, new UTF8Encoding(), encBytes);
                            var utfChars = Encoding.UTF8.GetChars(utfBytes);

                            title = new string(utfChars);
                        }

                        title = RemoveNewLine(title);
                        title = HttpUtility.HtmlDecode(title);
                        BotMethods.SendMessage(SendType.Message, e.Data.Channel, title);

                    }
                }
            }
            catch (Exception exception)
            {
                Log.Instance.Log(exception);
            }

        }
        private readonly char c10 = Convert.ToChar(10);
        private readonly char c13 = Convert.ToChar(13);

        private string RemoveNewLine(IEnumerable<char> s)
        {
            var sb = new StringBuilder();
            foreach (var c in s.Where(c => (c != c10) && (c != c13)))
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        private ForumItem GetTopic(string uri, string lang, bool hack)
        {

            ForumItem item = null;
            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.AcceptLanguage, lang);
            var feed = XmlReader.Create(client.OpenRead(uri));
            while (feed.Read())
            {
                if ((feed.NodeType != XmlNodeType.Element) || (feed.Name != "item")) continue;
                var temp = GetItem(feed);
                if (temp.Published == DateTime.MinValue)
                {
                    if (!hack && item != null)
                    {
                        item = GetTopic(uri + "&start=" + GetHack(item.Description), lang, true);
                    }
                }
                else
                {
                    item = temp;
                }
            }
            return item;
        }

        private readonly Regex _hackMatch = new Regex("(?<=Antworten )[0-9]*", RegexOptions.IgnoreCase);

        private string GetHack(string s)
        {
            return _hackMatch.Match(s).Value;
        }

        private static ForumItem GetItem(XmlReader feed)
        {

            string title = "", author = "", desc = "";

            var published = DateTime.MinValue;

            while (feed.Read())
            {
                if ((feed.NodeType == XmlNodeType.EndElement) && (feed.Name == "item"))
                {
                    break;
                }
                if (feed.NodeType == XmlNodeType.Element)
                {
                    switch (feed.Name)
                    {
                        case "title": feed.Read();
                            title = feed.ReadContentAsString();
                            break;
                        case "link": feed.Read();
                            //link = feed.ReadContentAsString();
                            break;
                        case "description": feed.Read();
                            desc = feed.ReadContentAsString();
                            break;
                        case "content:encoded": feed.Read();
                            //content
                            break;
                        case "category": feed.Read();
                            //category = feed.ReadContentAsString();
                            break;
                        case "author": feed.Read();
                            author = feed.ReadContentAsString();
                            break;
                        case "pubDate": feed.Read();
                            published = DateTime.Parse(feed.ReadContentAsString());
                            break;
                        case "guid": feed.Read();
                            //guid
                            break;
                        default: Log.Instance.Log("unparsed Element: " + feed.Name, Level.Info);
                            break;
                    }
                }
            }
            return new ForumItem(title, author, published, desc);
        }
    }
}
