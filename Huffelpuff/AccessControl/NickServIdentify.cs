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
using System.Linq;
using Meebey.SmartIrc4net;
using System.Threading;

namespace Huffelpuff.AccessControl
{
    /// <summary>
    /// Description of NickServ.
    /// </summary>
    public class NickServIdentify : IdentifyUser
    {
        private const int MaxWaitTime = 15000;

        public NickServIdentify(IrcBot bot)
            : base(bot)
        {
            bot.OnPart += BotOnPart;
            bot.OnKick += BotOnKick;
            bot.OnQuit += BotOnQuit;
            bot.OnNickChange += BotOnNickChange;
        }

        void BotOnNickChange(object sender, NickChangeEventArgs e)
        {
            RemoveNick(e.OldNickname);
            Identified(e.NewNickname);
        }

        void BotOnQuit(object sender, QuitEventArgs e)
        {
            if (e.Who == Bot.Nickname)
            {
                lock (NickCache)
                {
                    NickCache.Clear();
                }
            }
            else
            {
                RemoveNick(e.Who);
            }
        }

        void BotOnKick(object sender, KickEventArgs e)
        {
            if (e.Whom == Bot.Nickname)
            {
                lock (NickCache)
                {
                    NickCache.Clear();
                }
            }
            else
            {
                RemoveNick(e.Whom);
            }
        }

        void BotOnPart(object sender, PartEventArgs e)
        {
            if (e.Who == Bot.Nickname)
            {
                lock (NickCache)
                {
                    NickCache.Clear();
                }
            }
            else
            {
                RemoveNick(e.Who);
            }
        }

        private void RemoveNick(string nick)
        {
            lock (NickCache)
            {
                NickCache.Remove(nick);
            }
        }

        public Dictionary<string, string> NickCache = new Dictionary<string, string>();

        public string IdToNick(string id)
        {
            lock (NickCache)
            {
                foreach (var kvp in NickCache.Where(kvp => kvp.Value == id))
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        DateTime lastCheck;
        public override string Identified(string nick)
        {
            string id;
            if (NickCache.TryGetValue(nick, out id))
            {
                if ((id != null) || (lastCheck.AddSeconds(10) > DateTime.Now))
                {
                    return id;
                }
            }

            lastCheck = DateTime.Now;
            var nsir = new NickServIdentifyRequest(nick, Bot);

            if (NickCache.ContainsKey(nick))
            {
                NickCache.Remove(nick);
            }

            lock (nsir)
            {
                if (Monitor.Wait(nsir, MaxWaitTime))
                {
                    lock (NickCache)
                    {
                        if (!NickCache.ContainsKey(nick))
                            NickCache.Add(nick, nsir.Identity);
                    }
                }
                else
                {
                    return null;
                }
            }
            return nsir.Identity;
        }
    }
}
