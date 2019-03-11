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
using SharpIrc;
using Huffelpuff;
using Huffelpuff.Commands;
using Huffelpuff.Plugins;
using Pirate.PiVote.Crypto;
using Pirate.PiVote.Rpc;

namespace Plugin
{
    public class PiVotePlugin : AbstractPlugin
    {
        private const string CommandListVotings = "!pivote-list";
        private const string CommandListVotingsDescription = "Lists all votings";
        private const string CommandStatus = "!pivote-status";
        private const string CommandStatusDescription = "Status of the Pi-Vote plugin";
        private const string CommandTally = "!pivote-tally";
        private const string CommandTallyDescription = "Tally a voting";

        private const string PiVoteServerAddress = "pivote.piratenpartei.ch";
        private const int PiVoteServerPort = 4242;

        private CertificateStorage _certificateStorage;
        private VotingClient _client;
        private Queue<PiVoteAction> _actionQueue;

        public PiVotePlugin(IrcBot botInstance)
            : base(botInstance)
        {
        }

        public override void Activate()
        {
            _actionQueue = new Queue<PiVoteAction>();
            _certificateStorage = new CertificateStorage();

            if (!_certificateStorage.TryLoadRoot("./root.pi-cert"))
            {
                throw new Exception("Cannot find root certificate file.");
            }

            _client = new VotingClient(_certificateStorage);

            var serverIpAddress = Dns.GetHostEntry(PiVoteServerAddress).AddressList.First();
            var serverIpEndPoint = new IPEndPoint(serverIpAddress, PiVoteServerPort);
            _client.Connect(serverIpEndPoint);

            BotMethods.AddCommand(new Commandlet(CommandListVotings, CommandListVotingsDescription, ListVotingsHandler, this));
            BotMethods.AddCommand(new Commandlet(CommandTally, CommandTallyDescription, TallyHandler, this));
            BotMethods.AddCommand(new Commandlet(CommandStatus, CommandStatusDescription, StatusHandler, this));

            base.Activate();
        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand(CommandListVotings);
            BotMethods.RemoveCommand(CommandTally);
            BotMethods.RemoveCommand(CommandStatus);

            base.Deactivate();
        }

        private void StatusHandler(object sender, IrcEventArgs e)
        {
            if (_actionQueue.Count == 0)
            {
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Pi-Vote: No action currently executing.");
            }
            else
            {
                var action = _actionQueue.Peek();
                BotMethods.SendMessage(SendType.Message, e.Data.Channel, "Pi-Vote: " + action.StatusMessage);

                if (_actionQueue.Count > 1)
                {
                    BotMethods.SendMessage(SendType.Message, e.Data.Channel, $"Pi-Vote: {_actionQueue.Count - 1} more action queued.");
                }
            }
        }

        private void TallyHandler(object sender, IrcEventArgs e)
        {
            var action = new TallyAction(BotMethods, _client, _certificateStorage, e);
            AddAction(action);
        }

        private void ListVotingsHandler(object sender, IrcEventArgs e)
        {
            var action = new ListVotingsAction(BotMethods, _client, _certificateStorage, e);
            AddAction(action);
        }

        private void AddAction(PiVoteAction action)
        {
            _actionQueue.Enqueue(action);
            action.Completed += Action_Completed;

            if (_actionQueue.Count == 1)
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
            _actionQueue.Peek().Completed -= Action_Completed;
            _actionQueue.Dequeue();
            StartAction();
        }

        private void StartAction()
        {
            if (_actionQueue.Count > 0)
            {
                _actionQueue.Peek().Begin();
            }
        }

        public override string AboutHelp()
        {
            return "Pi-Vote Plugin";
        }

        public override string Name => "Pi-Vote Plugin";
    }
}
