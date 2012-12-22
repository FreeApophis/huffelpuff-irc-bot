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

using System.Threading;
using apophis.SharpIRC;
using Huffelpuff.Utils;

namespace Huffelpuff.AccessControl
{
    internal class NickServIdentifyRequest
    {

        private readonly IrcBot bot;
        private readonly string nick;

        public NickServIdentifyRequest(string nick, IrcBot bot)
        {
            this.bot = bot;
            this.nick = nick;

            bot.RfcWhois(nick);
            bot.OnRawMessage += BotOnRawMessage;

        }

        void BotOnRawMessage(object sender, IrcEventArgs e)
        {

            if ((e.Data.ReplyCode == ReplyCode.IdentifiedToServices) && (e.Data.RawMessageArray[3] == nick) && (e.Data.Message.StartsWith("is signed on as account")))
            {
                bot.OnRawMessage -= BotOnRawMessage;
                identity = e.Data.MessageArray[5];
                lock (this) Monitor.Pulse(this);
            }
            if ((e.Data.ReplyCode == ReplyCode.IdentifiedFreenode) && (e.Data.RawMessageArray[3] == nick) && (e.Data.Message.StartsWith("is logged in as")))
            {
                bot.OnRawMessage -= BotOnRawMessage;
                identity = e.Data.RawMessageArray[4];
                lock (this) Monitor.Pulse(this);
            }
            if ((e.Data.ReplyCode == ReplyCode.WhoIsRegistered) && (e.Data.RawMessageArray[3] == nick) && (e.Data.Message.StartsWith("is a registered nick")))
            {
                bot.OnRawMessage -= BotOnRawMessage;
                identity = e.Data.RawMessageArray[3];
                lock (this) Monitor.Pulse(this);
            }
            if ((e.Data.ReplyCode == ReplyCode.EndOfWhoIs) && (e.Data.RawMessageArray[3] == nick))
            {
                bot.OnRawMessage -= BotOnRawMessage;
                lock (this) Monitor.Pulse(this);
            }

            if (e.Data.ReplyCode == ReplyCode.WhoIsRegistered)
            {
                Log.Instance.Log(e.Data.Message, Level.Trace);
            }
        }

        private string identity;

        public string Identity
        {
            get { return (identity != null) ? "ns/" + identity : null; }
        }

    }
}
