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
using Dimebrain.TweetSharp;
using System;
using System.Linq;
using Dimebrain.TweetSharp.Extensions;
using Dimebrain.TweetSharp.Fluent;
using Dimebrain.TweetSharp.Model;
using Huffelpuff;

namespace Plugin
{
    /// <summary>
    /// Description of TwitterInfos.
    /// </summary>
    public class TwitterWrapper
    {
        public string Name { get; private set; }
        public string NameSpace {
            get{
                return "twitteraccount_" + Name;
            }
        }
        
        
        private const string friendlynameconst = "friendlyname";
        private string friendlyName;
        public string FriendlyName {
            get  {
                return friendlyName;
            }
            set {
                PersistentMemory.Instance.SetValue(NameSpace, friendlynameconst, value);
                friendlyName = value;
            }
        }
        
        private const string  userconst = "user";
        private string user;
        public string User {
            get  {
                return user;
            }
            set {
                PersistentMemory.Instance.SetValue(NameSpace, userconst, value);
                user = value;
            }
        }

        private const string passconst = "pass";
        private string pass;
        public string Pass {
            get  {
                return pass;
            }
            set {
                PersistentMemory.Instance.SetValue("twitteraccount_" + Name, passconst, value);
                pass = value;
            }
        }
        
        private const string lastconst = "lastdate";
        private DateTime last;
        public DateTime Last {
            get  {
                return last;
            }
            set {
                PersistentMemory.Instance.SetValue(NameSpace, lastconst, value.ToString());
                last = value;
            }
        }
        

        
        public TwitterWrapper(string name)
        {
            Name = name;
            friendlyName = PersistentMemory.Instance.GetValueOrTodo(NameSpace, friendlynameconst);
            user = PersistentMemory.Instance.GetValueOrTodo(NameSpace, userconst);
            pass = PersistentMemory.Instance.GetValueOrTodo(NameSpace, passconst);
        }
        
        public TwitterWrapper(string name, string friendlyName, string user, string pass)
        {
            Name = name;
            this.friendlyName = friendlyName;
            this.user = user;
            this.pass = pass;
            this.last = DateTime.MinValue;

            PersistentMemory.Instance.ReplaceValue(NameSpace, friendlynameconst, friendlyName);
            PersistentMemory.Instance.ReplaceValue(NameSpace, userconst, user);
            PersistentMemory.Instance.ReplaceValue(NameSpace, passconst, pass);
            PersistentMemory.Instance.ReplaceValue(NameSpace, lastconst, last.ToString());
        }
        
        public void RemoveAccount() {
            PersistentMemory.Instance.RemoveValue(TwitterPlugin.twitteraccountconst, Name);
            PersistentMemory.Instance.RemoveGroup(NameSpace);
        }

        
        public void GetMentions() {
            var request = FluentTwitter
                .CreateRequest(TwitterPlugin.ClientInfo)
                .AuthenticateAs(user, pass)
                .Statuses()
                .OnPublicTimeline()
                .AsJson();
            
            var response = request.Request();
            foreach(var status in response.AsStatuses()) {
                
            }
        }
        
        /// <summary>
        /// Send a message to the twitter account
        /// </summary>
        /// <param name="message">string with maximum 140 characters</param>
        /// <returns>returns the response as a string</returns>
        public string SendStatus(string message) {
            return FluentTwitter
                .CreateRequest(TwitterPlugin.ClientInfo)
                .AuthenticateAs(user, pass)
                .Statuses().Update(message)
                .AsJson()
                .Request();
        }
    }
}
