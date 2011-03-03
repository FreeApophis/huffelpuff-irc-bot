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

using System;
using Meebey.SmartIrc4net;

namespace Huffelpuff.Plugins
{
    /// <summary>
    /// Description of SharedClientSide.
    /// </summary>
    public class SharedClientSide : MarshalByRefObject
    {
        private readonly SharedServerSide serverSide;

        public SharedClientSide(IrcBot botInstance)
        {
            serverSide = new SharedServerSide(botInstance);

            serverSide.OnAdmin += HandleOnAdmin;
            serverSide.OnAutoConnectError += HandleOnAutoConnectError;
            serverSide.OnAway += HandleOnAway;
            serverSide.OnBan += HandleOnBan;
            serverSide.OnChannelAction += HandleOnChannelAction;
            serverSide.OnChannelActiveSynced += HandleOnChannelActiveSynced;
            serverSide.OnChannelMessage += HandleOnChannelMessage;
            serverSide.OnChannelModeChange += HandleOnChannelModeChange;
            serverSide.OnChannelNotice += HandleOnChannelNotice;
            serverSide.OnChannelPassiveSynced += HandleOnChannelPassiveSynced;
            serverSide.OnConnected += HandleOnConnected;
            serverSide.OnConnecting += HandleOnConnecting;
            serverSide.OnConnectionError += HandleOnConnectionError;
            serverSide.OnCtcpReply += HandleOnCtcpReply;
            serverSide.OnCtcpRequest += HandleOnCtcpRequest;
            serverSide.OnDccChatReceiveLineEvent += HandleOnDccChatReceiveLineEvent;
            serverSide.OnDccChatRequestEvent += HandleOnDccChatRequestEvent;
            serverSide.OnDccChatSentLineEvent += HandleOnDccChatSentLineEvent;
            serverSide.OnDccChatStartEvent += HandleOnDccChatStartEvent;
            serverSide.OnDccChatStopEvent += HandleOnDccChatStopEvent;
            serverSide.OnDccSendReceiveBlockEvent += HandleOnDccSendReceiveBlockEvent;
            serverSide.OnDccSendRequestEvent += HandleOnDccSendRequestEvent;
            serverSide.OnDccSendSentBlockEvent += HandleOnDccSendSentBlockEvent;
            serverSide.OnDccSendStartEvent += HandleOnDccSendStartEvent;
            serverSide.OnDccSendStopEvent += HandleOnDccSendStopEvent;
            serverSide.OnDeadmin += HandleOnDeadmin;
            serverSide.OnDehalfop += HandleOnDehalfop;
            serverSide.OnDeop += HandleOnDeop;
            serverSide.OnDeowner += HandleOnDeowner;
            serverSide.OnDevoice += HandleOnDevoice;
            serverSide.OnDisconnected += HandleOnDisconnected;
            serverSide.OnDisconnecting += HandleOnDisconnecting;
            serverSide.OnError += HandleOnError;
            serverSide.OnErrorMessage += HandleOnErrorMessage;
            serverSide.OnHalfop += HandleOnHalfop;
            serverSide.OnInvite += HandleOnInvite;
            serverSide.OnJoin += HandleOnJoin;
            serverSide.OnKick += HandleOnKick;
            serverSide.OnList += HandleOnList;
            serverSide.OnModeChange += HandleOnModeChange;
            serverSide.OnMotd += HandleOnMotd;
            serverSide.OnNames += HandleOnNames;
            serverSide.OnNickChange += HandleOnNickChange;
            serverSide.OnNowAway += HandleOnNowAway;
            serverSide.OnOp += HandleOnOp;
            serverSide.OnOwner += HandleOnOwner;
            serverSide.OnPart += HandleOnPart;
            serverSide.OnPing += HandleOnPing;
            serverSide.OnPong += HandleOnPong;
            serverSide.OnQueryAction += HandleOnQueryAction;
            serverSide.OnQueryMessage += HandleOnQueryMessage;
            serverSide.OnQueryNotice += HandleOnQueryNotice;
            serverSide.OnQuit += HandleOnQuit;
            serverSide.OnRawMessage += HandleOnRawMessage;
            serverSide.OnReadLine += HandleOnReadLine;
            serverSide.OnRegistered += HandleOnRegistered;
            serverSide.OnTopic += HandleOnTopic;
            serverSide.OnTopicChange += HandleOnTopicChange;
            serverSide.OnUnAway += HandleOnUnAway;
            serverSide.OnUnban += HandleOnUnban;
            serverSide.OnUserModeChange += HandleOnUserModeChange;
            serverSide.OnVoice += HandleOnVoice;
            serverSide.OnWho += HandleOnWho;
            serverSide.OnWriteLine += HandleOnWriteLine;
            serverSide.SupportNonRfcChanged += HandleSupportNonRfcChanged;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Unload()
        {
            serverSide.Unload();

            serverSide.OnAdmin -= HandleOnAdmin;
            serverSide.OnAutoConnectError -= HandleOnAutoConnectError;
            serverSide.OnAway -= HandleOnAway;
            serverSide.OnBan -= HandleOnBan;
            serverSide.OnChannelAction -= HandleOnChannelAction;
            serverSide.OnChannelActiveSynced -= HandleOnChannelActiveSynced;
            serverSide.OnChannelMessage -= HandleOnChannelMessage;
            serverSide.OnChannelModeChange -= HandleOnChannelModeChange;
            serverSide.OnChannelNotice -= HandleOnChannelNotice;
            serverSide.OnChannelPassiveSynced -= HandleOnChannelPassiveSynced;
            serverSide.OnConnected -= HandleOnConnected;
            serverSide.OnConnecting -= HandleOnConnecting;
            serverSide.OnConnectionError -= HandleOnConnectionError;
            serverSide.OnCtcpReply -= HandleOnCtcpReply;
            serverSide.OnCtcpRequest -= HandleOnCtcpRequest;
            serverSide.OnDccChatReceiveLineEvent -= HandleOnDccChatReceiveLineEvent;
            serverSide.OnDccChatRequestEvent -= HandleOnDccChatRequestEvent;
            serverSide.OnDccChatSentLineEvent -= HandleOnDccChatSentLineEvent;
            serverSide.OnDccChatStartEvent -= HandleOnDccChatStartEvent;
            serverSide.OnDccChatStopEvent -= HandleOnDccChatStopEvent;
            serverSide.OnDccSendReceiveBlockEvent -= HandleOnDccSendReceiveBlockEvent;
            serverSide.OnDccSendRequestEvent -= HandleOnDccSendRequestEvent;
            serverSide.OnDccSendSentBlockEvent -= HandleOnDccSendSentBlockEvent;
            serverSide.OnDccSendStartEvent -= HandleOnDccSendStartEvent;
            serverSide.OnDccSendStopEvent -= HandleOnDccSendStopEvent;
            serverSide.OnDeadmin -= HandleOnDeadmin;
            serverSide.OnDehalfop -= HandleOnDehalfop;
            serverSide.OnDeop -= HandleOnDeop;
            serverSide.OnDeowner -= HandleOnDeowner;
            serverSide.OnDevoice -= HandleOnDevoice;
            serverSide.OnDisconnected -= HandleOnDisconnected;
            serverSide.OnDisconnecting -= HandleOnDisconnecting;
            serverSide.OnError -= HandleOnError;
            serverSide.OnErrorMessage -= HandleOnErrorMessage;
            serverSide.OnHalfop -= HandleOnHalfop;
            serverSide.OnInvite -= HandleOnInvite;
            serverSide.OnJoin -= HandleOnJoin;
            serverSide.OnKick -= HandleOnKick;
            serverSide.OnList -= HandleOnList;
            serverSide.OnModeChange -= HandleOnModeChange;
            serverSide.OnMotd -= HandleOnMotd;
            serverSide.OnNames -= HandleOnNames;
            serverSide.OnNickChange -= HandleOnNickChange;
            serverSide.OnNowAway -= HandleOnNowAway;
            serverSide.OnOp -= HandleOnOp;
            serverSide.OnOwner -= HandleOnOwner;
            serverSide.OnPart -= HandleOnPart;
            serverSide.OnPing -= HandleOnPing;
            serverSide.OnPong -= HandleOnPong;
            serverSide.OnQueryAction -= HandleOnQueryAction;
            serverSide.OnQueryMessage -= HandleOnQueryMessage;
            serverSide.OnQueryNotice -= HandleOnQueryNotice;
            serverSide.OnQuit -= HandleOnQuit;
            serverSide.OnRawMessage -= HandleOnRawMessage;
            serverSide.OnReadLine -= HandleOnReadLine;
            serverSide.OnRegistered -= HandleOnRegistered;
            serverSide.OnTopic -= HandleOnTopic;
            serverSide.OnTopicChange -= HandleOnTopicChange;
            serverSide.OnUnAway -= HandleOnUnAway;
            serverSide.OnUnban -= HandleOnUnban;
            serverSide.OnUserModeChange -= HandleOnUserModeChange;
            serverSide.OnVoice -= HandleOnVoice;
            serverSide.OnWho -= HandleOnWho;
            serverSide.OnWriteLine -= HandleOnWriteLine;
            serverSide.SupportNonRfcChanged -= HandleSupportNonRfcChanged;
        }

        public event EventHandler<AdminEventArgs> OnAdmin;
        public event EventHandler<AutoConnectErrorEventArgs> OnAutoConnectError;
        public event EventHandler<AwayEventArgs> OnAway;
        public event EventHandler<BanEventArgs> OnBan;
        public event EventHandler<ActionEventArgs> OnChannelAction;
        public event EventHandler<IrcEventArgs> OnChannelActiveSynced;
        public event EventHandler<IrcEventArgs> OnChannelMessage;
        public event EventHandler<IrcEventArgs> OnChannelModeChange;
        public event EventHandler<IrcEventArgs> OnChannelNotice;
        public event EventHandler<IrcEventArgs> OnChannelPassiveSynced;
        public event EventHandler<EventArgs> OnConnected;
        public event EventHandler<EventArgs> OnConnecting;
        public event EventHandler<EventArgs> OnConnectionError;
        public event EventHandler<CtcpEventArgs> OnCtcpReply;
        public event EventHandler<CtcpEventArgs> OnCtcpRequest;
        public event EventHandler<DccChatEventArgs> OnDccChatReceiveLineEvent;
        public event EventHandler<DccEventArgs> OnDccChatRequestEvent;
        public event EventHandler<DccChatEventArgs> OnDccChatSentLineEvent;
        public event EventHandler<DccEventArgs> OnDccChatStartEvent;
        public event EventHandler<DccEventArgs> OnDccChatStopEvent;
        public event EventHandler<DccSendEventArgs> OnDccSendReceiveBlockEvent;
        public event EventHandler<DccSendRequestEventArgs> OnDccSendRequestEvent;
        public event EventHandler<DccSendEventArgs> OnDccSendSentBlockEvent;
        public event EventHandler<DccEventArgs> OnDccSendStartEvent;
        public event EventHandler<DccEventArgs> OnDccSendStopEvent;
        public event EventHandler<DeadminEventArgs> OnDeadmin;
        public event EventHandler<DehalfopEventArgs> OnDehalfop;
        public event EventHandler<DeopEventArgs> OnDeop;
        public event EventHandler<DeownerEventArgs> OnDeowner;
        public event EventHandler<DevoiceEventArgs> OnDevoice;
        public event EventHandler<EventArgs> OnDisconnected;
        public event EventHandler<EventArgs> OnDisconnecting;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<IrcEventArgs> OnErrorMessage;
        public event EventHandler<HalfopEventArgs> OnHalfop;
        public event EventHandler<InviteEventArgs> OnInvite;
        public event EventHandler<JoinEventArgs> OnJoin;
        public event EventHandler<KickEventArgs> OnKick;
        public event EventHandler<ListEventArgs> OnList;
        public event EventHandler<IrcEventArgs> OnModeChange;
        public event EventHandler<MotdEventArgs> OnMotd;
        public event EventHandler<NamesEventArgs> OnNames;
        public event EventHandler<NickChangeEventArgs> OnNickChange;
        public event EventHandler<IrcEventArgs> OnNowAway;
        public event EventHandler<OpEventArgs> OnOp;
        public event EventHandler<OwnerEventArgs> OnOwner;
        public event EventHandler<PartEventArgs> OnPart;
        public event EventHandler<PingEventArgs> OnPing;
        public event EventHandler<PongEventArgs> OnPong;
        public event EventHandler<IrcEventArgs> OnRawMessage;
        public event EventHandler<ReadLineEventArgs> OnReadLine;
        public event EventHandler<EventArgs> OnRegistered;
        public event EventHandler<ActionEventArgs> OnQueryAction;
        public event EventHandler<IrcEventArgs> OnQueryMessage;
        public event EventHandler<IrcEventArgs> OnQueryNotice;
        public event EventHandler<QuitEventArgs> OnQuit;
        public event EventHandler<TopicEventArgs> OnTopic;
        public event EventHandler<TopicChangeEventArgs> OnTopicChange;
        public event EventHandler<UnbanEventArgs> OnUnban;
        public event EventHandler<IrcEventArgs> OnUnAway;
        public event EventHandler<IrcEventArgs> OnUserModeChange;
        public event EventHandler<VoiceEventArgs> OnVoice;
        public event EventHandler<WhoEventArgs> OnWho;
        public event EventHandler<WriteLineEventArgs> OnWriteLine;
        public event EventHandler<EventArgs> SupportNonRfcChanged;

        internal void HandleOnAdmin(object sender, AdminEventArgs e)
        {
            if (OnAdmin != null)
                OnAdmin(this, e);
        }

        internal void HandleOnAutoConnectError(object sender, AutoConnectErrorEventArgs e)
        {
            if (OnAutoConnectError != null)
                OnAutoConnectError(this, e);
        }

        internal void HandleOnAway(object sender, AwayEventArgs e)
        {
            if (OnAway != null)
                OnAway(this, e);
        }

        internal void HandleOnBan(object sender, BanEventArgs e)
        {
            if (OnBan != null)
                OnBan(this, e);
        }

        internal void HandleOnChannelAction(object sender, ActionEventArgs e)
        {
            if (OnChannelAction != null)
                OnChannelAction(this, e);
        }

        internal void HandleOnChannelActiveSynced(object sender, IrcEventArgs e)
        {
            if (OnChannelActiveSynced != null)
                OnChannelActiveSynced(this, e);
        }

        internal void HandleOnChannelMessage(object sender, IrcEventArgs e)
        {
            if (OnChannelMessage != null)
                OnChannelMessage(this, e);
        }

        internal void HandleOnChannelModeChange(object sender, IrcEventArgs e)
        {
            if (OnChannelModeChange != null)
                OnChannelModeChange(this, e);
        }

        internal void HandleOnChannelNotice(object sender, IrcEventArgs e)
        {
            if (OnChannelNotice != null)
                OnChannelNotice(this, e);
        }

        internal void HandleOnChannelPassiveSynced(object sender, IrcEventArgs e)
        {
            if (OnChannelPassiveSynced != null)
                OnChannelPassiveSynced(this, e);
        }

        internal void HandleOnConnected(object sender, EventArgs e)
        {
            if (OnConnected != null)
                OnConnected(this, e);
        }

        internal void HandleOnConnecting(object sender, EventArgs e)
        {
            if (OnConnecting != null)
                OnConnecting(this, e);
        }

        internal void HandleOnConnectionError(object sender, EventArgs e)
        {
            if (OnConnectionError != null)
                OnConnectionError(this, e);
        }

        internal void HandleOnCtcpReply(object sender, CtcpEventArgs e)
        {
            if (OnCtcpReply != null)
                OnCtcpReply(this, e);
        }

        internal void HandleOnCtcpRequest(object sender, CtcpEventArgs e)
        {
            if (OnCtcpRequest != null)
                OnCtcpRequest(this, e);
        }

        internal void HandleOnDccChatReceiveLineEvent(object sender, DccChatEventArgs e)
        {
            if (OnDccChatReceiveLineEvent != null)
                OnDccChatReceiveLineEvent(this, e);
        }

        internal void HandleOnDccChatRequestEvent(object sender, DccEventArgs e)
        {
            if (OnDccChatRequestEvent != null)
                OnDccChatRequestEvent(this, e);
        }

        internal void HandleOnDccChatSentLineEvent(object sender, DccChatEventArgs e)
        {
            if (OnDccChatSentLineEvent != null)
                OnDccChatSentLineEvent(this, e);
        }

        internal void HandleOnDccChatStartEvent(object sender, DccEventArgs e)
        {
            if (OnDccChatStartEvent != null)
                OnDccChatStartEvent(this, e);
        }

        internal void HandleOnDccChatStopEvent(object sender, DccEventArgs e)
        {
            if (OnDccChatStopEvent != null)
                OnDccChatStopEvent(this, e);
        }

        internal void HandleOnDccSendReceiveBlockEvent(object sender, DccSendEventArgs e)
        {
            if (OnDccSendReceiveBlockEvent != null)
                OnDccSendReceiveBlockEvent(this, e);
        }

        internal void HandleOnDccSendRequestEvent(object sender, DccSendRequestEventArgs e)
        {
            if (OnDccSendRequestEvent != null)
                OnDccSendRequestEvent(this, e);
        }

        internal void HandleOnDccSendSentBlockEvent(object sender, DccSendEventArgs e)
        {
            if (OnDccSendSentBlockEvent != null)
                OnDccSendSentBlockEvent(this, e);
        }

        internal void HandleOnDccSendStartEvent(object sender, DccEventArgs e)
        {
            if (OnDccSendStartEvent != null)
                OnDccSendStartEvent(this, e);
        }

        internal void HandleOnDccSendStopEvent(object sender, DccEventArgs e)
        {
            if (OnDccSendStopEvent != null)
                OnDccSendStopEvent(this, e);
        }

        internal void HandleOnDeadmin(object sender, DeadminEventArgs e)
        {
            if (OnDeadmin != null)
                OnDeadmin(this, e);
        }

        internal void HandleOnDehalfop(object sender, DehalfopEventArgs e)
        {
            if (OnDehalfop != null)
                OnDehalfop(this, e);
        }

        internal void HandleOnDeop(object sender, DeopEventArgs e)
        {
            if (OnDeop != null)
                OnDeop(this, e);
        }

        internal void HandleOnDeowner(object sender, DeownerEventArgs e)
        {
            if (OnDeowner != null)
                OnDeowner(this, e);
        }

        internal void HandleOnDevoice(object sender, DevoiceEventArgs e)
        {
            if (OnDevoice != null)
                OnDevoice(this, e);
        }

        internal void HandleOnDisconnected(object sender, EventArgs e)
        {
            if (OnDisconnected != null)
                OnDisconnected(this, e);
        }

        internal void HandleOnDisconnecting(object sender, EventArgs e)
        {
            if (OnDisconnecting != null)
                OnDisconnecting(this, e);
        }

        internal void HandleOnError(object sender, ErrorEventArgs e)
        {
            if (OnError != null)
                OnError(this, e);
        }

        internal void HandleOnErrorMessage(object sender, IrcEventArgs e)
        {
            if (OnErrorMessage != null)
                OnErrorMessage(this, e);
        }

        internal void HandleOnHalfop(object sender, HalfopEventArgs e)
        {
            if (OnHalfop != null)
                OnHalfop(this, e);
        }

        internal void HandleOnInvite(object sender, InviteEventArgs e)
        {
            if (OnInvite != null)
                OnInvite(this, e);
        }

        internal void HandleOnJoin(object sender, JoinEventArgs e)
        {
            if (OnJoin != null)
                OnJoin(this, e);
        }

        internal void HandleOnKick(object sender, KickEventArgs e)
        {
            if (OnKick != null)
                OnKick(this, e);
        }

        internal void HandleOnList(object sender, ListEventArgs e)
        {
            if (OnList != null)
                OnList(this, e);
        }

        internal void HandleOnModeChange(object sender, IrcEventArgs e)
        {
            if (OnModeChange != null)
                OnModeChange(this, e);
        }

        internal void HandleOnMotd(object sender, MotdEventArgs e)
        {
            if (OnMotd != null)
                OnMotd(this, e);
        }

        internal void HandleOnNames(object sender, NamesEventArgs e)
        {
            if (OnNames != null)
                OnNames(this, e);
        }

        internal void HandleOnNickChange(object sender, NickChangeEventArgs e)
        {
            if (OnNickChange != null)
                OnNickChange(this, e);
        }

        internal void HandleOnNowAway(object sender, IrcEventArgs e)
        {
            if (OnNowAway != null)
                OnNowAway(this, e);
        }

        internal void HandleOnOp(object sender, OpEventArgs e)
        {
            if (OnOp != null)
                OnOp(this, e);
        }

        internal void HandleOnOwner(object sender, OwnerEventArgs e)
        {
            if (OnOwner != null)
                OnOwner(this, e);
        }

        internal void HandleOnPart(object sender, PartEventArgs e)
        {
            if (OnPart != null)
                OnPart(this, e);
        }

        internal void HandleOnPing(object sender, PingEventArgs e)
        {
            if (OnPing != null)
                OnPing(this, e);
        }

        internal void HandleOnPong(object sender, PongEventArgs e)
        {
            if (OnPong != null)
                OnPong(this, e);
        }

        internal void HandleOnQueryAction(object sender, ActionEventArgs e)
        {
            if (OnQueryAction != null)
                OnQueryAction(this, e);
        }

        internal void HandleOnQueryMessage(object sender, IrcEventArgs e)
        {
            if (OnQueryMessage != null)
                OnQueryMessage(this, e);
        }

        internal void HandleOnQueryNotice(object sender, IrcEventArgs e)
        {
            if (OnQueryNotice != null)
                OnQueryNotice(this, e);
        }

        internal void HandleOnQuit(object sender, QuitEventArgs e)
        {
            if (OnQuit != null)
                OnQuit(this, e);
        }

        internal void HandleOnRawMessage(object sender, IrcEventArgs e)
        {
            if (OnRawMessage != null)
                OnRawMessage(this, e);
        }

        internal void HandleOnReadLine(object sender, ReadLineEventArgs e)
        {
            if (OnReadLine != null)
                OnReadLine(this, e);
        }

        internal void HandleOnRegistered(object sender, EventArgs e)
        {
            if (OnRegistered != null)
                OnRegistered(this, e);
        }

        internal void HandleOnTopic(object sender, TopicEventArgs e)
        {
            if (OnTopic != null)
                OnTopic(this, e);
        }

        internal void HandleOnTopicChange(object sender, TopicChangeEventArgs e)
        {
            if (OnTopicChange != null)
                OnTopicChange(this, e);
        }

        internal void HandleOnUnAway(object sender, IrcEventArgs e)
        {
            if (OnUnAway != null)
                OnUnAway(this, e);
        }

        internal void HandleOnUnban(object sender, UnbanEventArgs e)
        {
            if (OnUnban != null)
                OnUnban(this, e);
        }

        internal void HandleOnUserModeChange(object sender, IrcEventArgs e)
        {
            if (OnUserModeChange != null)
                OnUserModeChange(this, e);
        }

        internal void HandleOnVoice(object sender, VoiceEventArgs e)
        {
            if (OnVoice != null)
                OnVoice(this, e);
        }

        internal void HandleOnWho(object sender, WhoEventArgs e)
        {
            if (OnWho != null)
                OnWho(this, e);
        }

        internal void HandleOnWriteLine(object sender, WriteLineEventArgs e)
        {
            if (OnWriteLine != null)
                OnWriteLine(this, e);
        }

        internal void HandleSupportNonRfcChanged(object sender, EventArgs e)
        {
            if (SupportNonRfcChanged != null)
                SupportNonRfcChanged(this, e);
        }
    }
}