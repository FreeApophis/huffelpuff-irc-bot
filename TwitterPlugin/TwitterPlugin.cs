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
        
        public TwitterPlugin(IrcBot botInstance) :
            base(botInstance) {}
        
        public override string Name {
            get {
                return Assembly.GetExecutingAssembly().FullName;
            }
        }
        
        public override void Activate() {
            BotMethods.AddCommand(new Commandlet("!twitter", "The command !twitter <your text>, will post a twitter message to the channel feed", twitterHandler, this, CommandScope.Both));
            base.Activate();
        }

        
        public override void Deactivate() {
            BotMethods.RemoveCommand("!twitter");
            base.Deactivate();
        }
        
        public override string AboutHelp() {
            return "The Twitter Plugin will post a twitter message to the channel feed, try !help twitter.";
        }
        
        private string baseUrl = "http://identi.ca/api/";
        private string user = "apophis";
        private string pass = "testpass";
        
        private void twitterHandler(object sender, IrcEventArgs e) {
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUrl + "statuses/update.xml");
                request.Credentials = new NetworkCredential(user, pass);
                request.PreAuthenticate = true;
                request.Method = "POST";

                string text = e.Data.Message.Substring(9);
                ASCIIEncoding encoding=new ASCIIEncoding();
                
                string postData = "status=" + HttpUtility.UrlEncode(e.Data.Nick + ":" + text);
                byte[]  data = encoding.GetBytes(postData);
                                   
                request.ContentType="application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                
                Stream newStream=request.GetRequestStream();
                newStream.Write(data,0,data.Length);
                newStream.Close();                               
                WebResponse webResponse = request.GetResponse();
                
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, "twittered your message!");                
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
