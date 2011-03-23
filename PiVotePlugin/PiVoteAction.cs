/*
 *  <project description>
 * 
 *  Copyright (c) 2010 Stefan Thöni <stefan@savvy.ch>
 *  File created by exception at 03.07.2009 18:53
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
using Huffelpuff;
using Meebey.SmartIrc4net;
using Pirate.PiVote.Crypto;
using Pirate.PiVote.Rpc;

namespace PiVotePlugin
{
    public abstract class PiVoteAction
    {
        public event EventHandler Completed;

        protected VotingClient Client { get; private set; }

        protected IrcBot BotMethods { get; private set; }

        protected CertificateStorage CertificateStorage { get; private set; }

        protected IrcEventArgs EventArgs { get; set; }

        public string Channel
        {
            get { return EventArgs.Data.Channel; }
        }

        protected PiVoteAction(IrcBot botMethods, VotingClient client, CertificateStorage certificateStorage, IrcEventArgs eventArgs)
        {
            BotMethods = botMethods;
            Client = client;
            CertificateStorage = certificateStorage;
            EventArgs = eventArgs;
        }

        public abstract void Begin();

        public abstract string StatusMessage { get; }

        protected void OnCompleted()
        {
            if (Completed != null)
            {
                Completed(this, new EventArgs());
            }
        }

        protected void OutTable(StringTable table)
        {
            string[] lines = table.Render().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                BotMethods.SendMessage(SendType.Message, Channel, line);
            }
        }
    }
}
