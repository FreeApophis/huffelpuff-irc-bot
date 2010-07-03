/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 05.09.2009 10:45
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
using Dimebrain.TweetSharp.Extensions;
using Dimebrain.TweetSharp.Fluent;
using Dimebrain.TweetSharp.Model;
using Huffelpuff.Utils;

namespace Plugin
{
    /// <summary>
    /// Description of TwitterInfos.
    /// </summary>
    public class TwitterWrapper
    {
        public string Name { get; private set; }
        public string NameSpace
        {
            get
            {
                return "twitteraccount_" + Name;
            }
        }


        private const string Friendlynameconst = "friendlyname";
        private string friendlyName;
        public string FriendlyName
        {
            get
            {
                return friendlyName;
            }
            set
            {
                PersistentMemory.Instance.SetValue(NameSpace, Friendlynameconst, value);
                friendlyName = value;
            }
        }

        private const string Userconst = "user";
        private string user;
        public string User
        {
            get
            {
                return user;
            }
            set
            {
                PersistentMemory.Instance.SetValue(NameSpace, Userconst, value);
                user = value;
            }
        }

        private const string Passconst = "pass";
        private string pass;
        public string Pass
        {
            get
            {
                return pass;
            }
            set
            {
                PersistentMemory.Instance.SetValue("twitteraccount_" + Name, Passconst, value);
                pass = value;
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

        public TwitterWrapper(string name)
        {
            Name = name;
            friendlyName = PersistentMemory.Instance.GetValueOrTodo(NameSpace, Friendlynameconst);
            user = PersistentMemory.Instance.GetValueOrTodo(NameSpace, Userconst);
            pass = PersistentMemory.Instance.GetValueOrTodo(NameSpace, Passconst);

            string lastDateTimeString = PersistentMemory.Instance.GetValue(NameSpace, Lastconst);
            last = (lastDateTimeString == null) ? DateTime.MinValue : DateTime.Parse(lastDateTimeString);
        }

        public TwitterWrapper(string name, string friendlyName, string user, string pass)
        {
            this.friendlyName = friendlyName;
            this.user = user;
            this.pass = pass;
            last = DateTime.MinValue;
            Name = name;

            PersistentMemory.Instance.ReplaceValue(NameSpace, Friendlynameconst, friendlyName);
            PersistentMemory.Instance.ReplaceValue(NameSpace, Userconst, user);
            PersistentMemory.Instance.ReplaceValue(NameSpace, Passconst, pass);
            PersistentMemory.Instance.ReplaceValue(NameSpace, Lastconst, last.ToString());
        }

        public void RemoveAccount()
        {
            PersistentMemory.Instance.RemoveValue(TwitterPlugin.TwitterAccountConst, Name);
            PersistentMemory.Instance.RemoveGroup(NameSpace);
        }


        private IEnumerable<TwitterStatus> GetMentions()
        {
            var mentions = FluentTwitter
                .CreateRequest(TwitterPlugin.ClientInfo)
                .AuthenticateAs(user, pass)
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
                .CreateRequest(TwitterPlugin.ClientInfo)
                .AuthenticateAs(user, pass)
                .Search()
                .Query()
                .ContainingHashTag(tag)
                .AsJson()
                .Request()
                .AsSearchResult()
                .Statuses;
        }

        public Dimebrain.TweetSharp.Model.Twitter.TwitterSearchTrends GetTrends()
        {
            return FluentTwitter
                .CreateRequest(TwitterPlugin.ClientInfo)
                .AuthenticateAs(user, pass)
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
                        .CreateRequest(TwitterPlugin.ClientInfo)
                        .AuthenticateAs(user, pass)
                        .Statuses()
                        .Retweet(statusId, RetweetMode.SymbolPrefix)
                        .AsJson()
                        .Request();
                }
            }

            return FluentTwitter
                .CreateRequest(TwitterPlugin.ClientInfo)
                .AuthenticateAs(user, pass)
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
            return FluentTwitter
                .CreateRequest(TwitterPlugin.ClientInfo)
                .AuthenticateAs(user, pass)
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
