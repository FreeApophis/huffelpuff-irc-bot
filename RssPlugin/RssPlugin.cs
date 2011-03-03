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
using System.Text.RegularExpressions;
using Huffelpuff;
using Huffelpuff.Plugins;
using Huffelpuff.Utils;
using Meebey.SmartIrc4net;


namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class RssPlugin : AbstractPlugin
    {
        public RssPlugin(IrcBot botInstance) :
            base(botInstance) { }

        private readonly Dictionary<string, RssWrapper> rssFeeds = new Dictionary<string, RssWrapper>();

        public const string RssFeedConst = "rss_feed";
        const string RssFormatConst = "rss_format";

        public override void Init()
        {
            // 90 seconds tick intervall
            TickInterval = 90;

            foreach (var rssInfo in PersistentMemory.Instance.GetValues(RssFeedConst).Select(rss => new RssWrapper(rss)).Where(rssInfo => rssInfo.FriendlyName != PersistentMemory.TodoValue))
            {
                rssFeeds.Add(rssInfo.FriendlyName, rssInfo);
            }
            base.Init();
        }

        public override void OnTick()
        {
            if (!BotMethods.IsConnected)
                return;
            foreach (var rssFeed in rssFeeds.Values)
            {
                foreach (var newItem in rssFeed.NewItems())
                {
                    foreach (var channel in PersistentMemory.Instance.GetValues(IrcBot.Channelconst))
                    {
                        SendFormattedItem(rssFeed, newItem, channel);
                    }
                }
            }
            PersistentMemory.Instance.Flush();
        }

        private void SendFormattedItem(RssWrapper rssFeed, RssItem rssItem, string sendto)
        {
            if (rssFeed == null || rssItem == null) return;

            BotMethods.SendMessage(SendType.Message, sendto,
                MessageFormat.FillKeyword(
                    "%FEEDTITLE%", rssFeed.FriendlyName,
                    "%FEEDURL%", rssFeed.Url,
                    "%TITLE%", StripHtml(rssItem.Title),
                    "%AUTHOR%", rssItem.Author,
                    "%CATEGORY%", rssItem.Category,
                    "%CONTENT%", StripHtml(rssItem.Content),
                    "%DESCRIPTION%", StripHtml(rssItem.Desc),
                    "%LINK%", rssItem.Link,
                    "%DATE%", rssItem.Published.ToString(),
                    "%AGO%", rssItem.Published.ToRelativeTime()
                ));
        }

        private string messageFormat;
        protected string MessageFormat
        {
            get
            {
                messageFormat = messageFormat ?? PersistentMemory.Instance.GetValue(RssFormatConst) ?? sourceMessageFormat;
                return messageFormat;
            }
        }

        private static string StripHtml(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            str = Regex.Replace(str, "<[^<]*>", string.Empty);
            str = Regex.Replace(str, "&nbsp;", " ");

            return str;
        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!rss", "With the command !rss [<feed> [<#post>]] post you'll get a list of the bots configured RSS feed, stats and posts.", ShowRss, this));
            BotMethods.AddCommand(new Commandlet("!rssformat", "With the command !rssfromat <formatstring> you can customize your RSS messages. [Vars: %FEEDTITLE% %FEEDURL% %TITLE% %AUTHOR% %CATEGORY% %CONTENT% %DESCRIPTION% %LINK% %DATE% %AGO%.]. You can reset to the initial setting with: !rssformat RESET", SetFormat, this, CommandScope.Both, "rss_admin"));
            BotMethods.AddCommand(new Commandlet("!+rss", "With the command !+rss <friendlyname> <url> [username:password] you can add an rss feeds even with a basic authentication.", AdminRss, this, CommandScope.Both, "rss_admin"));
            BotMethods.AddCommand(new Commandlet("!-rss", "With the command !-rss <friendlyname>  you can remove an rss feeds.", AdminRss, this, CommandScope.Both, "rss_admin"));

            base.Activate();
        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!rss");
            BotMethods.RemoveCommand("!rssformat");
            BotMethods.RemoveCommand("!-rss");
            BotMethods.RemoveCommand("!+rss");

            base.Deactivate();
        }

        public override string AboutHelp()
        {
            return "The Rss Plugins reports new posts on the configured RSS feed to the channel, and it provides access to the complete rss via the !rss command";
        }

        private readonly string sourceMessageFormat = "" + IrcConstants.IrcBold + "New Message" + IrcConstants.IrcBold +
                                       " on RSS Feed " + IrcConstants.IrcBold + "'" + IrcConstants.IrcColor +
                                       (int)IrcColors.Blue + "%FEEDTITLE%" + IrcConstants.IrcColor + "'" +
                                       IrcConstants.IrcBold + ": " + IrcConstants.IrcColor + (int)IrcColors.Brown +
                                       "%TITLE%" + IrcConstants.IrcColor + " (by " + IrcConstants.IrcBold + "%AUTHOR%" +
                                       IrcConstants.IrcBold + " on " + IrcConstants.IrcColor + (int)IrcColors.Blue +
                                       "%DATE%" + IrcConstants.IrcColor + " go: %LINK%)";

        private void SetFormat(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            if (e.Data.MessageArray.Length < 2)
            {
                BotMethods.SendMessage(SendType.Message, sendto, MessageFormat);
            }
            else
            {
                messageFormat = null;
                if (e.Data.MessageArray[1] == "RESET")
                {
                    PersistentMemory.Instance.RemoveKey(RssFormatConst);
                }
                else
                {
                    PersistentMemory.Instance.ReplaceValue(RssFormatConst, e.Data.Message.Substring(e.Data.MessageArray[0].Length + 1));
                }
            }
        }

        private void ShowRss(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            if (e.Data.MessageArray.Length < 2)
            {
                foreach (string lines in rssFeeds.Select(item => item.Value.FriendlyName).ToLines(350, ", ", "Currently checked feeds: ", "."))
                {
                    BotMethods.SendMessage(SendType.Message, sendto, lines);
                }
                return;
            }
            if (e.Data.MessageArray.Length < 3)
            {
                if (rssFeeds.ContainsKey(e.Data.MessageArray[1].ToLower()))
                {
                    var rssFeed = rssFeeds[e.Data.MessageArray[1].ToLower()];
                    BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' has {1} Elements, last post was on: {2}.".Fill(rssFeed.FriendlyName, rssFeed.Count, rssFeed.Last));
                }
                else
                {
                    BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' does not exists! Try '!rss'.".Fill(e.Data.MessageArray[1].ToLower()));
                }
                return;
            }
            if (rssFeeds.ContainsKey(e.Data.MessageArray[1].ToLower()))
            {
                var rssFeed = rssFeeds[e.Data.MessageArray[1].ToLower()];
                int index;
                if (int.TryParse(e.Data.MessageArray[2], out index))
                {
                    SendFormattedItem(rssFeed, rssFeed[index], sendto);
                }
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' does not exists! Try '!rss'.".Fill(e.Data.MessageArray[1].ToLower()));
            }
        }

        private void AdminRss(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            switch (e.Data.MessageArray[0].ToLower())
            {
                case "!+rss":
                    string credentials = null;
                    if (e.Data.MessageArray.Length < 3)
                    {
                        BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments! Try '!help !+rss'.");
                        return;
                    }
                    if (rssFeeds.ContainsKey(e.Data.MessageArray[1].ToLower()))
                    {
                        BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' already exists.".Fill(rssFeeds[e.Data.MessageArray[1].ToLower()].FriendlyName));
                        break;
                    }
                    if (e.Data.MessageArray.Length > 3)
                    {
                        credentials = e.Data.MessageArray[3];
                    }
                    PersistentMemory.Instance.SetValue(RssFeedConst, e.Data.MessageArray[1].ToLower());
                    rssFeeds.Add(e.Data.MessageArray[1].ToLower(), new RssWrapper(e.Data.MessageArray[1].ToLower(), e.Data.MessageArray[1], e.Data.MessageArray[2], DateTime.Now, credentials));
                    BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' successfully added.".Fill(rssFeeds[e.Data.MessageArray[1].ToLower()].FriendlyName));
                    break;
                case "!-rss":
                    if (e.Data.MessageArray.Length < 2)
                    {
                        BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments! Try '!help !-rss'.");
                        return;
                    }
                    if (rssFeeds.ContainsKey(e.Data.MessageArray[1].ToLower()))
                    {
                        rssFeeds[e.Data.MessageArray[1].ToLower()].RemoveFeed();
                        rssFeeds.Remove(e.Data.MessageArray[1].ToLower());
                        BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' successfully removed.".Fill(e.Data.MessageArray[1].ToLower()));
                    }
                    else
                    {
                        BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' does not exists! Try '!rss'.".Fill(e.Data.MessageArray[1].ToLower()));
                    }
                    break;
                default:
                    break;
            }
        }
    }
}