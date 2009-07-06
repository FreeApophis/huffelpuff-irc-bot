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
            bot.OnNames += new NamesEventHandler(bot_OnNames);
        }

        void bot_OnNickChange(object sender, NickChangeEventArgs e)
        {
            RemoveNick(e.OldNickname);
            Identified(e.NewNickname);
        }

        void bot_OnQuit(object sender, QuitEventArgs e)
        {
            if (e.Who == bot.Nickname) {
                lock (nickCache) {
                    nickCache.Clear();
                }
            } else {
                RemoveNick(e.Who);
            }
        }

        void bot_OnKick(object sender, KickEventArgs e)
        {
            if (e.Whom == bot.Nickname) {
                lock (nickCache) {
                    nickCache.Clear();
                }
            } else {
                RemoveNick(e.Whom);
            }
        }

        void bot_OnPart(object sender, PartEventArgs e)
        {
            if (e.Who == bot.Nickname) {
                lock (nickCache) {
                    nickCache.Clear();
                }
            } else {
                RemoveNick(e.Who);
            }
        }
        
        private void RemoveNick(string nick) {
            lock (nickCache) {
                nickCache.Remove(nick);
            }
        }
        
        void bot_OnNames(object sender, NamesEventArgs e)
        {
            foreach(string nick in e.UserList) {
                Identified(nick);
            }
        }
        
        public Dictionary<string, string> nickCache = new Dictionary<string, string>();

        public string IdToNick(string id) {
            lock (nickCache) {
                foreach(KeyValuePair<string, string> kvp in nickCache) {
                    if (kvp.Value == id) {
                        return kvp.Key;
                    }
                    
                }
            }
            return null;
        }
        
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
            
            if (nickCache.ContainsKey(nick)) {
                nickCache.Remove(nick);
            }
            
            lock (nsir) Monitor.Wait (nsir);
            lock (nickCache) {
                if(!nickCache.ContainsKey(nick))
                    nickCache.Add(nick, nsir.Identity);
            }
            return nsir.Identity;
        }
    }
}
