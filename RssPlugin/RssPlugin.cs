/*
 *  UT3GlobalStatsPlugin, Access to the GameSpy Stats for UT3
 * 
 *  Copyright (c) 2007-2009 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 2 of the License, or
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
using System.Timers;
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

        private Dictionary<string, RssWrapper> rssFeeds = new Dictionary<string, RssWrapper>();
        private Timer checkInterval;

        public const string RssFeedConst = "rss_feed";
        public override void Init()
        {
            checkInterval = new Timer();
            checkInterval.Elapsed += CheckIntervalElapsed;
            checkInterval.Interval = 1 * 90 * 1000; // 90 seconds
            foreach (var rssInfo in PersistentMemory.Instance.GetValues(RssFeedConst).Select(rss => new RssWrapper(rss)).Where(rssInfo => rssInfo.FriendlyName != PersistentMemory.TodoValue))
            {
                rssFeeds.Add(rssInfo.FriendlyName, rssInfo);
            }
            base.Init();
        }

        void CheckIntervalElapsed(object sender, ElapsedEventArgs e)
        {
            if (!BotMethods.IsConnected)
                return;
            try
            {
                foreach (var rssFeed in rssFeeds.Values)
                {
                    foreach (var newItem in rssFeed.NewItems())
                    {
                        foreach (string channel in PersistentMemory.Instance.GetValues(IrcBot.Channelconst))
                        {
                            BotMethods.SendMessage(SendType.Message, channel, ("" + IrcConstants.IrcBold + "'" + IrcConstants.IrcColor + (int)IrcColors.Blue + "{0}" + IrcConstants.IrcColor + "'" + IrcConstants.IrcBold + ": " + IrcConstants.IrcColor + (int)IrcColors.Brown + "{1}" + IrcConstants.IrcColor).Fill(rssFeed.FriendlyName, newItem.Title));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Instance.Log(exception.Message, Level.Error, ConsoleColor.Red);
            }
            PersistentMemory.Instance.Flush();
        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!rss", "With the command !rss [<feed> [<#post>]] post you'll get a list of the bots configured RSS feed, stats and posts.", ShowRss, this, CommandScope.Both));
            BotMethods.AddCommand(new Commandlet("!+rss", "With the command !+rss <friendlyname> <url> [username:password] you can add an rss feeds even with a basic authentication.", AdminRss, this, CommandScope.Both, "rss_admin"));
            BotMethods.AddCommand(new Commandlet("!-rss", "With the command !-rss <friendlyname>  you can remove an rss feeds.", AdminRss, this, CommandScope.Both, "rss_admin"));

            checkInterval.Enabled = true;

            base.Activate();
        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!rss");
            BotMethods.RemoveCommand("!-rss");
            BotMethods.RemoveCommand("!+rss");

            checkInterval.Enabled = false;

            base.Deactivate();
        }

        public override string AboutHelp()
        {
            return "The Rss Plugins reports new posts on the configured RSS feed to the channel, and it provides access to the complete rss via the !rss command";
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
            if (e.Data.MessageArray.Length >= 3) return;
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