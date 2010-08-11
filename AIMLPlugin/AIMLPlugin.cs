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


using System.Collections.Generic;
using AIMLbot;
using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class AIMLPlugin : AbstractPlugin
    {
        public AIMLPlugin(IrcBot botInstance) : base(botInstance) { }

        public override string Name
        {
            get
            {
                return "Artificial Intelligence Markup Language Plugin";
            }
        }

        Bot myAimlBot;
        readonly Dictionary<string, User> myUsers = new Dictionary<string, User>();

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
            BotEvents.OnChannelMessage += BotEvents_OnChannelMessage;

            base.Activate();
        }

        public override void Deactivate()
        {
            BotEvents.OnChannelMessage -= BotEvents_OnChannelMessage;

            base.Deactivate();
        }

        public override string AboutHelp()
        {
            return "Artificial Intelligence Markup Language Plugin";
        }

        void BotEvents_OnChannelMessage(object sender, IrcEventArgs e)
        {
            if (!e.Data.Message.ToLower().Contains(BotMethods.Nickname.ToLower())) return;

            var msg = e.Data.Message.ToLower().Trim().StartsWith(BotMethods.Nickname.ToLower())
                             ? e.Data.Message.Trim().Substring(BotMethods.Nickname.Length + 1)
                             : e.Data.Message.Trim();
            User myUser;
            if (myUsers.ContainsKey(e.Data.Nick))
            {
                myUser = myUsers[e.Data.Nick];
            }
            else
            {
                myUser = new User(e.Data.Nick, myAimlBot);
                myUser.Predicates.addSetting("name", e.Data.Nick);

                myUsers.Add(e.Data.Nick, myUser);
            }
            var r = new Request(msg, myUser, myAimlBot);
            var res = myAimlBot.Chat(r);
            BotMethods.SendMessage(SendType.Message, e.Data.Channel, res.Output);
        }
    }
}