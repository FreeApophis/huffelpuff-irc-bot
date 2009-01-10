/*
 *  AIML Plugin: Artificial Intelligence Metalanguage Plugin for
 *  the Huffelpuff IRC Bot
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
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

using Huffelpuff;
using Huffelpuff.SimplePlugins;
using Meebey.SmartIrc4net;
using AIMLbot;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class AIMLPlugin : IPlugin
    {
        
        
        public string Name {
            get {
                return "Artificial Intelligence Markup Language Plugin";
            }
        }
        
        private IrcBot bot = null;
        private bool ready = false;
        public bool Ready {
            get {
                return ready;
            }
        }
        
        private bool active = false;    
        public bool Active {
            get {
                return active;
            }
        }        

        Bot myBot;
        Dictionary<string, User> myUsers = new Dictionary<string, User>();
        
        public void Init(IrcBot botInstance)
        {
            bot = botInstance;
            
            myBot = new Bot();
            myBot.loadSettings();
            myBot.isAcceptingUserInput = false;
            myBot.loadAIMLFromFiles();
            myBot.isAcceptingUserInput = true;
            
            ready = true;
        }
        
        public void Activate()
        {
            bot.OnChannelMessage += new IrcEventHandler(messageHandler);
            active = true;
        }
        
        public void Deactivate()
        {
            bot.OnChannelMessage -= new IrcEventHandler(messageHandler);
            active = false;
        }
        
        public void DeInit()
        {
            ready = false;
        }        
        
        public string AboutHelp()
        {
            return "Artificial Intelligence Markup Language Plugin";
        }
        
        public List<KeyValuePair<string, string>> Commands()
        {
            return new List<KeyValuePair<string, string>>();
        }
        
        private void messageHandler(object sender, IrcEventArgs e) {
            
            
            string msg;
            if(e.Data.Message.ToLower().Contains(bot.Nickname.ToLower()))
            {
                if (e.Data.Message.ToLower().Trim().StartsWith(bot.Nickname.ToLower()))
                    msg = e.Data.Message.Trim().Substring(bot.Nickname.Length+1);
                else 
                    msg = e.Data.Message.Trim();
                User myUser = null;
                if (myUsers.ContainsKey(e.Data.Nick)) {
                    myUser = myUsers[e.Data.Nick];
                } else {
                    myUser = new User(e.Data.Nick, myBot);
                    myUser.Predicates.addSetting("name", e.Data.Nick);
                    
                    myUsers.Add(e.Data.Nick, myUser);
                }
                Request r = new Request(msg, myUser, myBot);
                Result res = myBot.Chat(r);
                bot.SendMessage(SendType.Message, e.Data.Channel, res.Output);
            }
        }
    }

}