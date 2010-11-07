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
  public class PiVotePlugin : AbstractPlugin
  {
    private const string CommandListVotings = "pivote-list";
    private const string CommandListVotingsDescription = "Lists all votings";

    private const string PiVoteServerAddress = "pivote.piratenpartei.ch";
    private const int PiVoteServerPort = 4242;

    private CertificateStorage certificateStorage;
    private VotingClient client;
    private Queue<PiVoteAction> actionQueue;

    public PiVotePlugin(IrcBot botInstance)
      : base(botInstance)
    {
    }

    public override void Activate()
    {
      this.actionQueue = new Queue<PiVoteAction>();
      this.certificateStorage = new CertificateStorage();

      if (!this.certificateStorage.TryLoadRoot())
      {
        throw new Exception("Cannot find root certificate file.");
      }

      this.client = new VotingClient(this.certificateStorage);

      IPAddress serverIpAddress = Dns.GetHostEntry(PiVoteServerAddress).AddressList.First();
      IPEndPoint serverIpEndPoint = new IPEndPoint(serverIpAddress, PiVoteServerPort);
      this.client.Connect(serverIpEndPoint);

      BotMethods.AddCommand(new Commandlet(CommandListVotings, CommandListVotingsDescription, ListVotingsHandler, this));

      base.Activate();
    }

    public override void Deactivate()
    {
      BotMethods.RemoveCommand(CommandListVotings);
    }

    private void ListVotingsHandler(object sender, IrcEventArgs e)
    {
      var action = new ListVotingsAction(BotMethods, this.client, this.certificateStorage, e);
      AddAction(action);
    }

    private void AddAction(PiVoteAction action)
    {
      this.actionQueue.Enqueue(action);
      action.Completed += new EventHandler(Action_Completed);

      if (this.actionQueue.Count == 1)
      {
        StartAction();
      }
      else
      {
        BotMethods.SendMessage(SendType.Message, action.Channel, "Pi-Vote: Your request was placed in queue.");
      }
    }

    private void Action_Completed(object sender, EventArgs e)
    {
      this.actionQueue.Peek().Completed -= new EventHandler(Action_Completed);
      this.actionQueue.Dequeue();
      StartAction();
    }

    private void StartAction()
    {
      if (this.actionQueue.Count > 0)
      {
        this.actionQueue.Peek().Begin();
      }
    }

    public override string AboutHelp()
    {
      return "Pi-Vote Plugin";
    }

    public override string Name
    {
      get { return "Pi-Vote Plugin"; }
    }
  }
}
