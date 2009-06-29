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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;

using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;
using System.Web;

namespace Plugin
{
    /// <summary>
    /// This is a very simple Plugin Example: The Echo Plugin
    /// </summary>
    public class TwitterPlugin : AbstractPlugin
    {
        private string baseUrl;
        private string feedUrl;
        private string user;
        private string pass;

        public TwitterPlugin(IrcBot botInstance) : base(botInstance) {
            baseUrl = PersistentMemory.Instance.GetValueOrTodo("twitter_api_base");
            feedUrl = PersistentMemory.Instance.GetValueOrTodo("twitter_feed_base");
            user = PersistentMemory.Instance.GetValueOrTodo("twitter_user");
            pass = PersistentMemory.Instance.GetValueOrTodo("twitter_pass");
        }
        
        public override string Name {
            get {
                return Assembly.GetExecutingAssembly().FullName;
            }
        }
        
        public override void Activate() {
            BotMethods.AddCommand(new Commandlet("!twitter", "The command !twitter <your text>, will post a twitter message to the channel feed", twitterHandler, this, CommandScope.Public, "twitter_access"));
            base.Activate();
        }

        
        public override void Deactivate() {
            BotMethods.RemoveCommand("!twitter");
            base.Deactivate();
        }
        
        public override string AboutHelp() {
            return "The Twitter Plugin will post a twitter message to the channel feed, try !help twitter.";
        }
        

        
        private void twitterHandler(object sender, IrcEventArgs e) {
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUrl + "statuses/update.xml");
                request.Credentials = new NetworkCredential(user, pass);
                request.PreAuthenticate = true;
                request.Method = "POST";

                string text = e.Data.Message.Substring(9);
                text = tinyUrlFilter(text);
                
                
                if(text.Length > 140) {
                    BotMethods.SendMessage(SendType.Notice, e.Data.Nick, "Your message was too long (" + text.Length + "), please rephrase!");
                    return;
                }
                ASCIIEncoding encoding=new ASCIIEncoding();
                string postData = "status=" + HttpUtility.UrlEncode(text);
                byte[]  data = encoding.GetBytes(postData);
                
                request.ContentType="application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                
                Stream newStream=request.GetRequestStream();
                newStream.Write(data,0,data.Length);
                newStream.Close();
                WebResponse webResponse = request.GetResponse();
                
                BotMethods.SendMessage(SendType.Notice, e.Data.Nick, "twittered your message!" );
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, "New message on twitter-feed: " + feedUrl);
            } catch (Exception ex) {
                Console.WriteLine();
                BotMethods.SendMessage(SendType.Notice, e.Data.Nick, "Twitter Error: " + ex.Message);
            }
        }
        
        private string tinyUrlFilter(string text) {
            Dictionary<string, string> replace = new Dictionary<string, string>();
            foreach(string word in text.Split(new char[] {' '})) {
                if ((word.Length > 25) && (word.StartsWith("http://"))) {
                    WebClient tinyurl = new WebClient();
                    tinyurl.QueryString.Add("url", word);
                    string tiny = tinyurl.DownloadString("http://tinyurl.com/api-create.php");
                    if (!replace.ContainsKey(word)) {
                        replace.Add(word, tiny);
                    }
                }
            }
            
            foreach(KeyValuePair<string, string> kvp in replace) {
                text = text.Replace(kvp.Key, kvp.Value);
            }
            
            return text;
        }
    }
}
