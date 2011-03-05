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
using Huffelpuff;
using Meebey.SmartIrc4net;
using Pirate.PiVote.Crypto;
using Pirate.PiVote.Rpc;

namespace PiVotePlugin
{
    public class TallyAction : PiVoteAction
    {
        private readonly int votingNumber = -1;

        public TallyAction(IrcBot botMethods, VotingClient client, CertificateStorage certificateStorage, IrcEventArgs eventArgs)
            : base(botMethods, client, certificateStorage, eventArgs)
        {
            if (eventArgs.Data.MessageArray.Length == 2)
            {
                int votingNumberTemp;

                if (int.TryParse(eventArgs.Data.MessageArray[1], out votingNumberTemp))
                {
                    if (votingNumberTemp >= 0)
                    {
                        votingNumber = votingNumberTemp;
                    }
                }
            }
        }

        public override void Begin()
        {
            if (votingNumber >= 0)
            {
                BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: Tallying voting.");
                Client.GetCertificateStorage(CertificateStorage, GetCertificateStorageComplete);
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: Enter a voting number for tallying.");
            }
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
                var votingsInOrder = votings.OrderBy(v => v.VoteFrom);

                if (votingNumber >= votingsInOrder.Count())
                {
                    BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: Voting number out of range.");
                }
                else
                {
                    var voting = votingsInOrder.ElementAt(votingNumber);

                    if (voting.Status == VotingStatus.Finished)
                    {
                        BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: Tallying voting " + voting.Title.Text + ".");

                        Client.GetResult(voting.Id, new List<Signed<VoteReceipt>>(), GetResultComplete);
                    }
                    else
                    {
                        BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: Cannot tally " + voting.Title.Text + " at this time.");
                    }
                }
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: " + exception.Message);
            }

            OnCompleted();
        }

        private void GetResultComplete(VotingResult result, IDictionary<Guid, VoteReceiptStatus> voteReceiptsStatus, Exception exception)
        {
            if (exception == null)
            {
                var table = new StringTable();

                table.SetColumnCount(2);

                table.AddRow(result.Title.Text, string.Empty);
                table.AddRow("Total Ballots", result.TotalBallots.ToString());
                table.AddRow("Invalid Ballots", result.InvalidBallots.ToString());
                table.AddRow("Valid Ballots", result.ValidBallots.ToString());

                foreach (var question in result.Questions)
                {
                    table.AddRow(question.Text.Text, string.Empty);

                    foreach (var option in question.Options)
                    {
                        table.AddRow(option.Text.Text, option.Result.ToString());
                    }
                }

                BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: Result of voting:");
                OutTable(table);
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: " + exception.Message);
            }
        }

        public override string StatusMessage
        {
            get
            {
                return "Tallying : " + (Client.CurrentOperation == null ? "Unknown status." : Client.CurrentOperation.Text) + ".";
            }
        }
    }
}
