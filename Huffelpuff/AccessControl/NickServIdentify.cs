/*
 *  The Huffelpuff Irc Bot, versatile pluggable bot for IRC chats
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
using Meebey.SmartIrc4net;
using System.Threading;

namespace Huffelpuff
{
    /// <summary>
    /// Description of NickServ.
    /// </summary>
    public class NickServIdentify : IdentifyUser
    {
        
        public NickServIdentify(IrcBot bot) : base(bot) {
            bot.OnPart += new PartEventHandler(bot_OnPart);
            bot.OnKick += new KickEventHandler(bot_OnKick);
            bot.OnQuit += new QuitEventHandler(bot_OnQuit);
            bot.OnNickChange += new NickChangeEventHandler(bot_OnNickChange);
        }

        void bot_OnNickChange(object sender, NickChangeEventArgs e)
        {
            RemoveNick(e.OldNickname);
        }

        void bot_OnQuit(object sender, QuitEventArgs e)
        {
            RemoveNick(e.Who);
        }

        void bot_OnKick(object sender, KickEventArgs e)
        {
            RemoveNick(e.Whom);
        }

        void bot_OnPart(object sender, PartEventArgs e)
        {
            RemoveNick(e.Who);
        }
        
        private void RemoveNick(string nick) {
            nickCache.Remove(nick);
        }
        
        public Dictionary<string, string> nickCache = new Dictionary<string, string>();

        DateTime lastCheck;
        public override string Identified(string nick)
        {
            string id;
            if (nickCache.TryGetValue(nick, out id)) {
                if ((id != null) || (lastCheck.AddSeconds(10) > DateTime.Now)) {
                    return id;
                }
            }
            
            lastCheck = DateTime.Now;
            NickServIdentifyRequest nsir = new NickServIdentifyRequest(nick, bot);
            lock (nsir) Monitor.Wait (nsir);
            
            nickCache.Add(nick, nsir.Identity);
            return nsir.Identity;
        }
    }
    
    internal class NickServIdentifyRequest {

        private IrcBot bot;
        private string nick;
        
        public NickServIdentifyRequest(string nick, IrcBot bot) {
            this.bot = bot;
            this.nick = nick;

            bot.RfcWhois(nick);
            bot.OnRawMessage += new IrcEventHandler(bot_OnRawMessage);
        }
        
        void bot_OnRawMessage(object sender, IrcEventArgs e)
        {
            if ((e.Data.ReplyCode == ReplyCode.IdentifiedToServices) && (e.Data.RawMessageArray[3] == nick) && (e.Data.Message.StartsWith("is signed on as account")))
            {
                bot.OnRawMessage -= new IrcEventHandler(bot_OnRawMessage);
                identity = e.Data.MessageArray[5];
                lock (this) Monitor.Pulse (this);
            }
            if ((e.Data.ReplyCode == ReplyCode.EndOfWhoIs) && (e.Data.RawMessageArray[3] == nick)) {
                bot.OnRawMessage -= new IrcEventHandler(bot_OnRawMessage);
                lock (this) Monitor.Pulse (this);
            }

            if (e.Data.ReplyCode == ReplyCode.WhoIsRegistered) {
                Console.WriteLine(e.Data.Message);
            }
        }
        
        private string identity = null;
        
        public string Identity {
            get { return (identity!=null)?"ns/" + identity:null; }
        }

    }
    
    
}
