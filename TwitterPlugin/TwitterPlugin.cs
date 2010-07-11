/*
 *  This is an example Plugin, you can use it as a base for your own plugins.
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
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
using System.Reflection;
using System.Timers;
using System.Web;

using Dimebrain.TweetSharp;
using Dimebrain.TweetSharp.Extensions;
using Huffelpuff;
using Huffelpuff.Plugins;
using Huffelpuff.Utils;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// This is a very simple Plugin Example: The Echo Plugin
    /// </summary>
    public class TwitterPlugin : AbstractPlugin
    {
        private Timer checkInterval;

        public TwitterPlugin(IrcBot botInstance) : base(botInstance) { }

        private readonly Dictionary<string, TwitterWrapper> twitterAccounts = new Dictionary<string, TwitterWrapper>();

        public const string TwitterAccountConst = "twitter_account";
        public static TwitterClientInfo ClientInfo = new TwitterClientInfo { ClientName = "Huffelpuff IRC Bot - Twitter Plugin", ClientUrl = "http://huffelpuff-irc-bot.origo.ethz.ch/", ClientVersion = "1.0", ConsumerKey = "yRaZB2ljtZ1ldg84Uvu4Iw", ConsumerSecret = "26SPIqzqcfQZPsgchKipqWLX3bCGu7vw0JaAUghuKs" };


        public override void Init()
        {

            foreach (var twitterInfo in PersistentMemory.Instance.GetValues(TwitterAccountConst).Select(a => new TwitterWrapper(a)).Where(twitterInfo => twitterInfo.FriendlyName != PersistentMemory.TodoValue))
            {
                twitterAccounts.Add(twitterInfo.FriendlyName, twitterInfo);
                tocolorize.Add(new ColorText { Text = twitterInfo.User, Color = (int)IrcColors.Blue });
            }

            checkInterval = new Timer(90000);

            checkInterval.Elapsed += CheckIntervalElapsed;

            base.Init();

        }

        void CheckIntervalElapsed(object sender, ElapsedEventArgs e)
        {
            if (!BotMethods.IsConnected)
                return;
            try
            {
                foreach (var twitteraccount in twitterAccounts)
                {
                    foreach (var mention in twitteraccount.Value.GetNewMentions())
                    {
                        foreach (var channel in PersistentMemory.Instance.GetValues(IrcBot.Channelconst))
                        {
                            BotMethods.SendMessage(SendType.Message, channel, "{8}New Mention{9} ({7}): {0} (by {6}{1}{6} ({2}/{3}/{4}) - {5})".Fill(
                                Colorize(mention.Text), mention.User.ScreenName, mention.User.StatusesCount, mention.User.FriendsCount, mention.User.FollowersCount, mention.CreatedDate.ToRelativeTime(),
                                IrcConstants.IrcBold, mention.Id, "" + IrcConstants.IrcBold + IrcConstants.IrcColor + "" + (int)IrcColors.Orange, "" + IrcConstants.IrcColor + IrcConstants.IrcBold));
                        }
                    }
                }

                foreach (var tag in PersistentMemory.Instance.GetValues("twitter_search_tag"))
                {
                    foreach (var tagStatus in twitterAccounts.First().Value.SearchNewTag(tag))
                    {
                        foreach (var channel in PersistentMemory.Instance.GetValues(IrcBot.Channelconst))
                        {
                            BotMethods.SendMessage(SendType.Message, channel, "Tag: {0} (by {1})".Fill(tagStatus.Text, tagStatus.FromUserScreenName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /*
                private readonly Regex whiteSpaceMatch = new Regex(@"\s+");


                private string SafeString(string str)
                {
                    return whiteSpaceMatch.Replace(str, " ");
                }
        */

        private struct ColorText
        {
            public string Text;
            public int Color;

        }
        private readonly List<ColorText> tocolorize = new List<ColorText>();

        private string Colorize(string text)
        {
            return tocolorize.Aggregate(text, (current, wordAndColor) => current.Replace(wordAndColor.Text, "" + IrcConstants.IrcBold + IrcConstants.IrcColor + "" + wordAndColor.Color + wordAndColor.Text + IrcConstants.IrcNormal + IrcConstants.IrcColor));
        }

        private static string Shorten(string text)
        {
            return UrlShortener.GetUnuUrl(text);
        }

        public override string Name
        {
            get
            {
                return Assembly.GetExecutingAssembly().FullName;
            }
        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!tweet", "The !tweet <account> <text> command tweets a message to the account. If there is only 1 account, the account name can be omitted.", TweetHandler, this, CommandScope.Public, "twitter_access"));
            BotMethods.AddCommand(new Commandlet("!retweet", "The !retweet <account> <id> command retweets a message, just enter the ID of the tweet. If there is only 1 account, the account name can be omitted.", TweetHandler, this, CommandScope.Public, "twitter_access"));
            BotMethods.AddCommand(new Commandlet("!tweet-stats", "The !tweet-stats.", TweetStatsHandler, this, CommandScope.Both));
            BotMethods.AddCommand(new Commandlet("!tweet-trends", "The !tweet-stats.", TweetTrendsHandler, this, CommandScope.Both));
            BotMethods.AddCommand(new Commandlet("!+tweet", "With the command !+tweet <friendlyname> [<username> <password>] you can add a tweets.", AdminTweet, this, CommandScope.Private, "twitter_admin"));
            BotMethods.AddCommand(new Commandlet("!-tweet", "With the command !-tweet <friendlyname> you can remove a tweets.", AdminTweet, this, CommandScope.Private, "twitter_admin"));
            BotMethods.AddCommand(new Commandlet("!+tag", "With the command !+tag you can add a search tag.", TagHandler, this, CommandScope.Both, "twitter_admin"));
            BotMethods.AddCommand(new Commandlet("!-tag", "With the command !-tag you can remove a search tag.", TagHandler, this, CommandScope.Both, "twitter_admin"));
            BotMethods.AddCommand(new Commandlet("!utf8", "!utf8 äöü", Utf8Handler, this, CommandScope.Both));

            checkInterval.Enabled = true;
            base.Activate();
        }


        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!tweet");
            BotMethods.RemoveCommand("!tweet-stats");
            BotMethods.RemoveCommand("!tweet-trends");
            BotMethods.RemoveCommand("!+tweet");
            BotMethods.RemoveCommand("!-tweet");
            BotMethods.RemoveCommand("!+tag");
            BotMethods.RemoveCommand("!-tag");
            BotMethods.RemoveCommand("!utf8");

            checkInterval.Enabled = false;
            base.Deactivate();
        }

        public override string AboutHelp()
        {
            return "The Twitter Plugin will post a twitter message to the channel feed, try !help tweet.";
        }


        private void Utf8Handler(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            if (e.Data.MessageArray.Length > 1)
            {
                BotMethods.SendMessage(SendType.Message, sendto,
                                       e.Data.MessageArray[1] == "äöü"
                                           ? "Yes your text is in UTF-8"
                                           : "Sorry thats either not UTF-8 or you havent typed !utf8 äöü");
            }
        }

        /*
                private string MessageTime(DateTime time)
                {
                    return time.ToString("HH:mm K", new CultureInfo("DE-ch", true));
                }


                private string Ago(TimeSpan ago)
                {
                    if (ago.Days > 0)
                    {
                        return ago.Days + ((ago.Days == 1) ? " day" : " days") + " ago";
                    }
                    if (ago.Hours > 0)
                    {
                        return ago.Hours + ((ago.Days == 1) ? " hour" : " hours") + " ago";
                    }
                    if (ago.Minutes > 0)
                    {
                        return ago.Minutes + ((ago.Days == 1) ? " minute" : " minutes") + " ago";
                    }
                    return ago.Seconds + ((ago.Days == 1) ? " second" : " seconds") + " ago";
                }
        */

        private void TagHandler(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            if (e.Data.MessageArray[0].ToLower() == "!+tag")
            {
                PersistentMemory.Instance.SetValue("twitter_search_tag", e.Data.MessageArray[1]);
                BotMethods.SendMessage(SendType.Message, sendto, "Automatic Search activated: " + "http://search.twitter.com/search?q=" + HttpUtility.UrlEncode(e.Data.MessageArray[1]));
            }
            if (e.Data.MessageArray[0].ToLower() == "!-tag")
            {
                PersistentMemory.Instance.RemoveValue("twitter_search_tag", e.Data.MessageArray[1]);

            }
            if (e.Data.MessageArray[0].ToLower() == "!tags")
            {
                foreach (string line in PersistentMemory.Instance.GetValues("twitter_search_tag").ToLines(350))
                {
                    BotMethods.SendMessage(SendType.Message, sendto, line);
                }
            }
        }

        private void TweetStatsHandler(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            if (e.Data.MessageArray.Length < 2)
            {
                BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments for 'add'! Try '!help !tweet-stats'.");
                return;
            }

            if (twitterAccounts.ContainsKey(e.Data.MessageArray[1].ToLower()))
            {
                var user = twitterAccounts[e.Data.MessageArray[1].ToLower()].GetStats();
                BotMethods.SendMessage(SendType.Message, sendto, "Followers: {0}, Friends: {1}, Statuses: {2}, -> {3}"
                                       .Fill(user.FollowersCount, user.FriendsCount, user.StatusesCount, user.Url));
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, sendto, "I dont know a tweet with the name: {0}.".Fill(e.Data.MessageArray[1].ToLower()));
            }
        }

        private void TweetTrendsHandler(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            var trends = twitterAccounts.First().Value.GetTrends();
            if (trends == null)
            {
                BotMethods.SendMessage(SendType.Message, sendto, "Trends failed");
            }
            else
            {
                foreach (var line in trends.Trends.Select(trend => trend.Name).ToLines(350, ", ", "Current trends: ", ""))
                {
                    BotMethods.SendMessage(SendType.Message, sendto, line);
                }
            }
        }

        private void TweetHandler(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            bool retweet = (e.Data.MessageArray[0] == "!retweet");

            if (e.Data.MessageArray.Length < 2)
            {
                foreach (string line in twitterAccounts.Select(item => item.Value.FriendlyName).ToLines(350, ", ", "Tweet accounts loaded: ", "."))
                {
                    BotMethods.SendMessage(SendType.Message, sendto, line);
                }
                return;
            }
            if (e.Data.MessageArray.Length < 3 && twitterAccounts.Count > 1)
            {
                BotMethods.SendMessage(SendType.Message, sendto, "Nothing to say? I don't tweet empty messages! try !help !tweet.");
                return;
            }
            if (twitterAccounts.ContainsKey(e.Data.MessageArray[1].ToLower()))
            {
                string status = e.Data.Message.Substring(e.Data.MessageArray[0].Length + e.Data.MessageArray[1].Length + 2);
                try
                {
                    status = Shorten(status);
                    if (status.Length > 140)
                    {
                        BotMethods.SendMessage(SendType.Message, sendto, "Error on feed '{0}': Message longer than 140 characters(140+{1}), try rewriting and tweet again, nothing was tweeted.".Fill(twitterAccounts[e.Data.MessageArray[1].ToLower()].FriendlyName, status.Length - 140));
                        return;
                    }
                    var returnFromTwitter = twitterAccounts[e.Data.MessageArray[1].ToLower()].SendStatus(status, retweet);
                    var error = returnFromTwitter.AsError();
                    if (error == null)
                    {
                        var twitterStatus = returnFromTwitter.AsStatus();
                        string statusUrl = "http://twitter.com/{0}/status/{1}".Fill(twitterStatus.User.ScreenName, twitterStatus.Id);
                        BotMethods.SendMessage(SendType.Message, sendto, "successfully tweeted on feed '{0}', Link to Status: {1}".Fill(twitterAccounts[e.Data.MessageArray[1].ToLower()].FriendlyName, statusUrl));
                        return;
                    }
                    BotMethods.SendMessage(SendType.Message, sendto, "Error on feed '{0}': {1}".Fill(twitterAccounts[e.Data.MessageArray[1].ToLower()].FriendlyName, error.ErrorMessage));
                    return;
                }
                catch (Exception) { }
            }
            else if (twitterAccounts.Count == 1)
            {

            }
            else
            {
                BotMethods.SendMessage(SendType.Message, sendto, "I dont know a tweet with the name: {0}.".Fill(e.Data.MessageArray[1].ToLower()));
                return;
            }
        }


        private void AdminTweet(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            switch (e.Data.MessageArray[0].ToLower())
            {
                case "!+tweet":
                    if (e.Data.MessageArray.Length < 4)
                    {
                        BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments for 'add'! Try '!help !+tweet'.");
                        return;
                    }
                    if (twitterAccounts.ContainsKey(e.Data.MessageArray[1].ToLower()))
                    {
                        BotMethods.SendMessage(SendType.Message, sendto, "Tweet '{0}' already exists.".Fill(twitterAccounts[e.Data.MessageArray[1].ToLower()].FriendlyName));
                        break;
                    }
                    PersistentMemory.Instance.SetValue(TwitterAccountConst, e.Data.MessageArray[1].ToLower());
                    twitterAccounts.Add(e.Data.MessageArray[1].ToLower(), new TwitterWrapper(e.Data.MessageArray[1].ToLower(), e.Data.MessageArray[1], e.Data.MessageArray[2], e.Data.MessageArray[3]));
                    BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' successfully added.".Fill(twitterAccounts[e.Data.MessageArray[1].ToLower()].FriendlyName));
                    break;
                case "!-tweet":
                    if (e.Data.MessageArray.Length < 2)
                    {
                        BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments! Try '!help !-tweet'.");
                        return;
                    }
                    if (twitterAccounts.ContainsKey(e.Data.MessageArray[1].ToLower()))
                    {
                        twitterAccounts[e.Data.MessageArray[1].ToLower()].RemoveAccount();
                        twitterAccounts.Remove(e.Data.MessageArray[1].ToLower());
                        BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' successfully removed.".Fill(e.Data.MessageArray[1].ToLower()));
                    }
                    else
                    {
                        BotMethods.SendMessage(SendType.Message, sendto, "Feed '{0}' does not exists! Try '!tweet'.".Fill(e.Data.MessageArray[1].ToLower()));
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
