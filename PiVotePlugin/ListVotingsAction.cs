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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;
using Pirate.PiVote;
using Pirate.PiVote.Crypto;
using Pirate.PiVote.Rpc;

namespace PiVotePlugin
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
      Client.GetVotingList(CertificateStorage, Environment.CurrentDirectory, GetVotingListCompleted);
    }

    private void GetVotingListCompleted(IEnumerable<VotingDescriptor> votings, Exception exception)
    {
      BotMethods.SendMessage(SendType.Message, Channel, string.Format("Pi-Vote: There are {0} votings.", votings.Count()));

      StringTable table = new StringTable();

      table.AddColumn("Title", 24);
      table.AddColumn("From", 12);
      table.AddColumn("Until", 12);
      table.AddColumn("Status", 12); 

      foreach (VotingDescriptor voting in votings.OrderBy(v => v.VoteFrom))
      {
        table.AddRow(voting.Title.Text, voting.VoteFrom.ToShortDateString(), voting.VoteUntil.ToShortDateString(), voting.Status.Text(), voting.EnvelopeCount.ToString());
      }

      BotMethods.SendMessage(SendType.Message, Channel, "Pi-Vote: Voting list:"); 
      string[] lines = table.Render().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

      foreach (string line in lines)
      {
        BotMethods.SendMessage(SendType.Message, Channel, line);
      }

      OnCompleted();
    }

    public override string StatusMessage
    {
      get
      {
        return this.Client.CurrentOperation == null ? "Unknown status." : this.Client.CurrentOperation.Text;
      }
    }
  }
}
