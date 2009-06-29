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
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;
using AIMLbot;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class AIMLPlugin : AbstractPlugin
    {
        public AIMLPlugin(IrcBot botInstance) : base(botInstance) {}
        
        public override string Name {
            get {
                return "Artificial Intelligence Markup Language Plugin";
            }
        }

        Bot myAimlBot;
        Dictionary<string, User> myUsers = new Dictionary<string, User>();
        
        public override void Init()
        {
            myAimlBot = new Bot();
            myAimlBot.loadSettings();
            myAimlBot.isAcceptingUserInput = false;
            myAimlBot.loadAIMLFromFiles();
            myAimlBot.isAcceptingUserInput = true;
            
            base.Init();
        }
        
        public override void Activate()
        {
            BotEvents.OnChannelMessage += new IrcEventHandler(BotEvents_OnChannelMessage);
            //BotEvents.OnQueryMessage += new IrcEventHandler(BotEvents_OnQueryMessage);
            
            base.Activate();
        }
        
        public override  void Deactivate()
        {
            BotEvents.OnChannelMessage -= new IrcEventHandler(BotEvents_OnChannelMessage);
            //BotEvents.OnQueryMessage -= new IrcEventHandler(BotEvents_OnQueryMessage);
            
            base.Deactivate();
        }
        
        public override  string AboutHelp()
        {
            return "Artificial Intelligence Markup Language Plugin";
        }
        
        void BotEvents_OnChannelMessage(object sender, IrcEventArgs e)
        {
            string msg;
            if(e.Data.Message.ToLower().Contains(BotMethods.Nickname.ToLower()))
            {
                if (e.Data.Message.ToLower().Trim().StartsWith(BotMethods.Nickname.ToLower()))
                    msg = e.Data.Message.Trim().Substring(BotMethods.Nickname.Length+1);
                else
                    msg = e.Data.Message.Trim();
                User myUser = null;
                if (myUsers.ContainsKey(e.Data.Nick)) {
                    myUser = myUsers[e.Data.Nick];
                } else {
                    myUser = new User(e.Data.Nick, myAimlBot);
                    myUser.Predicates.addSetting("name", e.Data.Nick);
                    
                    myUsers.Add(e.Data.Nick, myUser);
                }
                Request r = new Request(msg, myUser, myAimlBot);
                Result res = myAimlBot.Chat(r);
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, res.Output);
            }
        }
        
        void BotEvents_OnQueryMessage(object sender, IrcEventArgs e)
        {
            //throw new NotImplementedException();
        }

        
    }
}