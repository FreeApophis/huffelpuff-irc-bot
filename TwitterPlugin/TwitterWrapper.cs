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

namespace Plugin
{
    /// <summary>
    /// Description of TwitterInfos.
    /// </summary>
    internal class TwitterWrapper
    {
        private static readonly TwitterClientInfo ClientInfo = new TwitterClientInfo
        {
            ClientName = "Huffelpuff IRC Bot - Twitter Plugin",
            ClientUrl = "http://huffelpuff-irc-bot.origo.ethz.ch/",
            ClientVersion = "1.0",
            ConsumerKey = "yRaZB2ljtZ1ldg84Uvu4Iw",
            ConsumerSecret = "26SPIqzqcfQZPsgchKipqWLX3bCGu7vw0JaAUghuKs"
        };

        internal string Name { get; private set; }
        private string NameSpace
        {
            get
            {
                return "twitteraccount_" + Name;
            }
        }


        private const string FriendlynameConst = "friendlyname";
        private string friendlyName;
        internal string FriendlyName
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
        internal string User
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
        private string Token
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

        public bool IsAuthenticated
        {
            get
            {
                if (tokenSecret == null)
                {
                    CreateNewRequestToken();
                }
                return tokenSecret != null;
            }
        }

        private const string TokenSecretConst = "tokensecret";
        private string tokenSecret;
        private string TokenSecret
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
        internal DateTime Last
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

        private static readonly Dictionary<string, DateTime> LastTag = new Dictionary<string, DateTime>();

        public static TwitterResponse LastResponse { get; private set; }

        internal TwitterWrapper(string name)
        {
            Name = name;
            friendlyName = PersistentMemory.Instance.GetValueOrTodo(NameSpace, FriendlynameConst);
            user = PersistentMemory.Instance.GetValueOrTodo(NameSpace, UserConst);
            token = PersistentMemory.Instance.GetValue(NameSpace, TokenConst);
            tokenSecret = PersistentMemory.Instance.GetValue(NameSpace, TokenSecretConst);

            string lastDateTimeString = PersistentMemory.Instance.GetValue(NameSpace, Lastconst);
            last = (lastDateTimeString == null) ? DateTime.MinValue : DateTime.Parse(lastDateTimeString);
        }

        internal TwitterWrapper(string name, string friendlyName, string user)
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

        internal Uri AuthenticationUrl { get; private set; }
        private OAuthRequestToken UnauthorizedToken { get; set; }

        private void CreateNewRequestToken()
        {
            var service = new TwitterService(ClientInfo);

            UnauthorizedToken = service.GetRequestToken();
            AuthenticationUrl = service.GetAuthorizationUri(UnauthorizedToken);
        }

        internal bool AuthenticateToken(string pin)
        {
            var service = new TwitterService(ClientInfo);
            try
            {
                var accessToken = service.GetAccessToken(UnauthorizedToken, pin);
                Token = accessToken.Token;
                TokenSecret = accessToken.TokenSecret;

                PersistentMemory.Instance.Flush();

                return IsTweetAuthenticated;
            }
            catch (Exception exception)
            {
                Log.Instance.Log(exception);
                return false;
            }
        }

        internal bool IsTweetAuthenticated
        {
            get
            {
                if (string.IsNullOrEmpty(Token))
                    return true;

                var service = new TwitterService(ClientInfo);
                service.AuthenticateWith(Token, TokenSecret);
                return service.VerifyCredentials() != null;
            }
        }

        internal void ResetToken()
        {
            Token = null;
            TokenSecret = null;
            AuthenticationUrl = null;
            UnauthorizedToken = null;
            PersistentMemory.Instance.Flush();

            CreateNewRequestToken();
        }

        internal void RemoveAccount()
        {
            PersistentMemory.Instance.RemoveValue(TwitterPlugin.TwitterAccountConst, Name);
            PersistentMemory.Instance.RemoveGroup(NameSpace);
        }


        private IEnumerable<TwitterStatus> GetMentions()
        {
            var service = new TwitterService(ClientInfo);
            service.AuthenticateWith(Token, TokenSecret);

            LastResponse = service.Response;
            return service.ListTweetsMentioningMe();
        }

        internal IEnumerable<TwitterStatus> GetNewMentions()
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

        internal static IEnumerable<TwitterSearchStatus> SearchNewTag(string tag)
        {
            var allTags = SearchTag(tag);
            var time = LastTag.ContainsKey(tag) ? LastTag[tag] : DateTime.Now.AddHours(-3);
            var newTags = allTags.Where(tss => tss.CreatedDate > time).OrderBy(tss => tss.CreatedDate).ToList();
            if (newTags.Count() > 0)
            {
                LastTag[tag] = newTags.OrderByDescending(tss => tss.CreatedDate).Take(1).Single().CreatedDate;
            }
            return newTags;
        }

        private static IEnumerable<TwitterSearchStatus> SearchTag(string tag)
        {
            var service = new TwitterService(ClientInfo);

            LastResponse = service.Response;
            return service.Search(tag).Statuses;
        }

        internal static TwitterTrends GetTrends()
        {
            var service = new TwitterService(ClientInfo);

            LastResponse = service.Response;
            return service.ListCurrentTrends();
        }

        /// <summary>
        /// Send a message to the twitter account
        /// </summary>
        /// <param name="message">string with maximum 140 characters</param>
        /// <param name="retweet"></param>
        /// <returns>returns the response as a string</returns>
        internal TwitterStatus SendStatus(string message, bool retweet)
        {
            var service = new TwitterService(ClientInfo);
            service.AuthenticateWith(Token, TokenSecret);

            LastResponse = service.Response;
            return service.SendTweet(message);

        }

        /// <summary>
        /// Returns the current user (fails if there is no status yet)
        /// Missing method in TweetSharp
        /// </summary>
        /// <returns>the current Twitter User</returns>
        internal TwitterUser GetStats()
        {
            var service = new TwitterService(ClientInfo);
            service.AuthenticateWith(Token, TokenSecret);

            LastResponse = service.Response;
            return service.GetUserProfile();
        }
    }
}
