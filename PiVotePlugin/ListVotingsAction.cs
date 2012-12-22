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
using System.Collections.Generic;
using System.Linq;
using apophis.SharpIRC;
using Huffelpuff;
using Pirate.PiVote.Crypto;
using Pirate.PiVote.Rpc;

namespace Plugin
{
    public class ListVotingsAction : PiVoteAction
    {
        public ListVotingsAction(IrcBot botMethods, VotingClient client, CertificateStorage certificateStorage, IrcEventArgs eventArgs)
            : base(botMethods, client, certificateStorage, eventArgs)
        {
        }

        public override void Begin()
        {
            BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: Getting voting list.");
            Client.GetCertificateStorage(CertificateStorage, GetCertificateStorageComplete);
        }

        private void GetCertificateStorageComplete(Certificate serverCertificate, Exception exception)
        {
            if (exception == null)
            {
                Client.GetVotingList(CertificateStorage, Environment.CurrentDirectory, GetVotingListCompleted);
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: " + exception.Message);
            }
        }

        private void GetVotingListCompleted(IEnumerable<VotingDescriptor> votings, Exception exception)
        {
            if (exception == null)
            {
                StringTable table = new StringTable();

                table.AddColumn("No");
                table.AddColumn("Title");
                table.AddColumn("From");
                table.AddColumn("Until");
                table.AddColumn("Status");
                table.AddColumn("Votes");

                int number = 0;

                foreach (VotingDescriptor voting in votings.OrderBy(v => v.VoteFrom))
                {
                    table.AddRow(number.ToString(), voting.Title.Text, voting.VoteFrom.ToShortDateString(), voting.VoteUntil.ToShortDateString(), voting.Status.Text(), voting.EnvelopeCount.ToString());
                    number++;
                }

                BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: Voting list:");
                OutTable(table);
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: " + exception.Message);
            }

            OnCompleted();
        }

        public override string StatusMessage
        {
            get
            {
                return "Listing votes : " + (this.Client.CurrentOperation == null ? "Unknown status." : this.Client.CurrentOperation.Text) + ".";
            }
        }
    }
}
