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
using Plugin.Database.Twitter;
using Plugin.Properties;
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
            ConsumerKey = TwitterSettings.Default.TwitterConsumerKey,
            ConsumerSecret = TwitterSettings.Default.TwitterConsumerSecret
        };

        private TwitterAccount account;

        internal string FriendlyName
        {
            get
            {
                return account.FriendlyName;
            }
            set
            {
                account.FriendlyName = value;
                TwitterPlugin.TwitterData.SubmitChanges();
            }
        }

        internal string User
        {
            get
            {
                return account.UserName;
            }
            set
            {
                account.FriendlyName = value;
                TwitterPlugin.TwitterData.SubmitChanges();
            }
        }

        private string Token
        {
            get
            {
                if (account.Token == null)
                {
                    CreateNewRequestToken();
                }
                return account.Token;
            }
            set
            {
                account.Token = value;
                TwitterPlugin.TwitterData.SubmitChanges();
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                if (account.TokenSecret == null)
                {
                    CreateNewRequestToken();
                }
                return account.TokenSecret != null;
            }
        }

        private string TokenSecret
        {
            get
            {
                if (account.TokenSecret == null)
                {
                    CreateNewRequestToken();
                }
                return account.TokenSecret;
            }
            set
            {
                account.TokenSecret = value;
                TwitterPlugin.TwitterData.SubmitChanges();
            }
        }

        internal DateTime Last
        {
            get
            {
                return account.LastMessage.HasValue ? account.LastMessage.Value : DateTime.Now;
            }
            set
            {
                account.LastMessage = value;
                TwitterPlugin.TwitterData.SubmitChanges();
            }
        }

        private static readonly Dictionary<string, DateTime> LastTag = new Dictionary<string, DateTime>();

        public static TwitterResponse LastResponse { get; private set; }

        internal TwitterWrapper(string friendlyName)
        {
            account = TwitterPlugin.TwitterData.TwitterAccounts.Where(a => a.FriendlyName == friendlyName).FirstOrDefault();
        }

        internal TwitterWrapper(string friendlyName, string user)
        {
            account = new TwitterAccount();

            TwitterPlugin.TwitterData.TwitterAccounts.InsertOnSubmit(account);

            account.FriendlyName = friendlyName;
            account.UserName = user;
            account.LastMessage = DateTime.Now;

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

            CreateNewRequestToken();
        }

        internal void RemoveAccount()
        {
            TwitterPlugin.TwitterData.TwitterAccounts.DeleteOnSubmit(account);
            TwitterPlugin.TwitterData.SubmitChanges();
            account = null;
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
            {
                return Enumerable.Empty<TwitterStatus>();
            }

            var newMentions = mentions.Where(item => item.CreatedDate > Last).OrderBy(item => item.CreatedDate).ToList();
            if (newMentions.Count() > 0)
            {
                Last = newMentions.OrderByDescending(item => item.CreatedDate).Take(1).Single().CreatedDate;
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
