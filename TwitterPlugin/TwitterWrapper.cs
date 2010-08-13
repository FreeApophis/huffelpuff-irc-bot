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
using Huffelpuff.Utils;
using TweetSharp;
using TweetSharp.Model;
using TweetSharp.Twitter.Extensions;
using TweetSharp.Twitter.Fluent;
using TweetSharp.Twitter.Model;
using TweetSharp.Twitter.Service;

namespace Plugin
{
    /// <summary>
    /// Description of TwitterInfos.
    /// </summary>
    public class TwitterWrapper
    {
        public static TwitterClientInfo ClientInfo = new TwitterClientInfo
        {
            ClientName = "Huffelpuff IRC Bot - Twitter Plugin",
            ClientUrl = "http://huffelpuff-irc-bot.origo.ethz.ch/",
            ClientVersion = "1.0",
            ConsumerKey = "yRaZB2ljtZ1ldg84Uvu4Iw",
            ConsumerSecret = "26SPIqzqcfQZPsgchKipqWLX3bCGu7vw0JaAUghuKs"
        };

        public string Name { get; private set; }
        public string NameSpace
        {
            get
            {
                return "twitteraccount_" + Name;
            }
        }


        private const string FriendlynameConst = "friendlyname";
        private string friendlyName;
        public string FriendlyName
        {
            get
            {
                return friendlyName;
            }
            set
            {
                PersistentMemory.Instance.SetValue(NameSpace, FriendlynameConst, value);
                friendlyName = value;
            }
        }

        private const string UserConst = "user";
        private string user;
        public string User
        {
            get
            {
                return user;
            }
            set
            {
                PersistentMemory.Instance.SetValue(NameSpace, UserConst, value);
                user = value;
            }
        }

        private const string TokenConst = "token";
        private string token;
        public string Token
        {
            get
            {
                if (token == null)
                {
                    CreateNewRequestToken();
                }
                return token;
            }
            set
            {
                PersistentMemory.Instance.SetValue(NameSpace, TokenConst, value);
                token = value;
            }
        }

        private const string TokenSecretConst = "tokensecret";
        private string tokenSecret;
        public string TokenSecret
        {
            get
            {
                if (tokenSecret == null)
                {
                    CreateNewRequestToken();
                }
                return tokenSecret;
            }
            set
            {
                PersistentMemory.Instance.SetValue(NameSpace, TokenSecretConst, value);
                tokenSecret = value;
            }
        }

        private const string Lastconst = "lastdate";
        private DateTime last;
        public DateTime Last
        {
            get
            {
                return last;
            }
            set
            {
                PersistentMemory.Instance.SetValue(NameSpace, Lastconst, value.ToString());
                last = value;
            }
        }

        private readonly Dictionary<string, DateTime> lastTag = new Dictionary<string, DateTime>();

        public TwitterWrapper()
        {
            // this is a dummy constructor: if you need an object desperatly!
        }

        public TwitterWrapper(string name)
        {
            Name = name;
            friendlyName = PersistentMemory.Instance.GetValueOrTodo(NameSpace, FriendlynameConst);
            user = PersistentMemory.Instance.GetValueOrTodo(NameSpace, UserConst);
            token = PersistentMemory.Instance.GetValue(NameSpace, TokenConst);

            string lastDateTimeString = PersistentMemory.Instance.GetValue(NameSpace, Lastconst);
            last = (lastDateTimeString == null) ? DateTime.MinValue : DateTime.Parse(lastDateTimeString);
        }

        public TwitterWrapper(string name, string friendlyName, string user)
        {
            this.friendlyName = friendlyName;
            this.user = user;
            last = DateTime.MinValue;
            Name = name;

            PersistentMemory.Instance.ReplaceValue(NameSpace, FriendlynameConst, friendlyName);
            PersistentMemory.Instance.ReplaceValue(NameSpace, UserConst, user);
            PersistentMemory.Instance.ReplaceValue(NameSpace, Lastconst, last.ToString());

            CreateNewRequestToken();
        }

        public string AuthenticationUrl { get; private set; }
        public OAuthToken UnauthorizedToken { get; private set; }

        private void CreateNewRequestToken()
        {
            var service = new TwitterService(ClientInfo);

            UnauthorizedToken = service.GetRequestToken(ClientInfo.ConsumerKey, ClientInfo.ConsumerSecret);
            AuthenticationUrl = FluentTwitter.CreateRequest().Authentication.GetAuthorizationUrl(UnauthorizedToken.Token);
        }

        public bool AuthenticateToken(string pin)
        {
            var service = new TwitterService(ClientInfo);
            var accessToken = service.GetAccessToken(UnauthorizedToken, pin);

            Token = accessToken.Token;
            TokenSecret = accessToken.TokenSecret;

            return IsTweetAuthenticated;
        }

        internal bool IsTweetAuthenticated
        {
            get
            {
                var service = new TwitterService(ClientInfo);
                service.AuthenticateWith(Token, TokenSecret);
                return service.Error == null;
            }
        }

        public void RemoveAccount()
        {
            PersistentMemory.Instance.RemoveValue(TwitterPlugin.TwitterAccountConst, Name);
            PersistentMemory.Instance.RemoveGroup(NameSpace);
        }


        private IEnumerable<TwitterStatus> GetMentions()
        {
            if (Token == null)
            {
                return null;
            }

            var mentions = FluentTwitter
                .CreateRequest(ClientInfo)
                .AuthenticateWith(Token, TokenSecret)
                .Statuses()
                .Mentions()
                .AsJson()
                .Request();

            return mentions.AsStatuses();
        }

        public IEnumerable<TwitterStatus> GetNewMentions()
        {
            var mentions = GetMentions();
            if (mentions == null)
                return new List<TwitterStatus>();
            var newMentions = mentions.Where(item => item.CreatedDate > last).OrderBy(item => item.CreatedDate).ToList();
            if (newMentions.Count() > 0)
            {
                last = newMentions.OrderByDescending(item => item.CreatedDate).Take(1).Single().CreatedDate;
                PersistentMemory.Instance.ReplaceValue(NameSpace, Lastconst, last.ToString());
            }
            return newMentions;
        }

        public IEnumerable<TwitterSearchStatus> SearchNewTag(string tag)
        {
            var allTags = SearchTag(tag);
            var time = lastTag.ContainsKey(tag) ? lastTag[tag] : DateTime.Now.AddHours(-3);
            var newTags = allTags.Where(tss => tss.CreatedDate > time).OrderBy(tss => tss.CreatedDate).ToList();
            if (newTags.Count() > 0)
            {
                lastTag[tag] = newTags.OrderByDescending(tss => tss.CreatedDate).Take(1).Single().CreatedDate;
            }
            return newTags;
        }

        private IEnumerable<TwitterSearchStatus> SearchTag(string tag)
        {
            return FluentTwitter
                .CreateRequest(ClientInfo)
                .AuthenticateWith(Token, TokenSecret)
                .Search()
                .Query()
                .ContainingHashTag(tag)
                .AsJson()
                .Request()
                .AsSearchResult()
                .Statuses;
        }

        public TwitterSearchTrends GetTrends()
        {
            return FluentTwitter
                .CreateRequest(ClientInfo)
                .Search()
                .Trends()
                .Current()
                .Request()
                .AsSearchTrends();
        }

        /// <summary>
        /// Send a message to the twitter account
        /// </summary>
        /// <param name="message">string with maximum 140 characters</param>
        /// <param name="retweet"></param>
        /// <returns>returns the response as a string</returns>
        public TwitterResult SendStatus(string message, bool retweet)
        {
            if (retweet)
            {
                long statusId;
                if (long.TryParse(message, out statusId))
                {
                    return FluentTwitter
                        .CreateRequest(ClientInfo)
                        .AuthenticateWith(Token, TokenSecret)
                        .Statuses()
                        .Retweet(statusId, RetweetMode.SymbolPrefix)
                        .AsJson()
                        .Request();
                }
            }

            return FluentTwitter
                .CreateRequest(ClientInfo)
                .AuthenticateWith(Token, TokenSecret)
                .Statuses()
                .Update(message)
                .AsJson()
                .Request();

        }

        /// <summary>
        /// Returns the current user (fails if there is no status yet)
        /// Missing method in TweetSharp
        /// </summary>
        /// <returns>the current Twitter User</returns>
        public TwitterUser GetStats()
        {
            if (token == null) return null;
            return FluentTwitter
                .CreateRequest(ClientInfo)
                .AuthenticateWith(Token, TokenSecret)
                .Statuses()
                .OnUserTimeline()
                .AsJson()
                .Request()
                .AsStatuses()
                .First()
                .User;
        }
    }
}
