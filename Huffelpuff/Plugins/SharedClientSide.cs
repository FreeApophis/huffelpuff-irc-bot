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

using SharpIrc;
using SharpIrc.IrcClient.EventArgs;
using SharpIrc.IrcConnection;
using SharpIrc.IrcFeatures.EventArgs;
using System;

namespace Huffelpuff.Plugins
{
    /// <summary>
    /// Description of SharedClientSide.
    /// </summary>
    public class SharedClientSide : MarshalByRefObject
    {
        private readonly SharedServerSide _serverSide;

        public SharedClientSide(IrcBot botInstance)
        {
            _serverSide = new SharedServerSide(botInstance);

            _serverSide.OnAdmin += HandleOnAdmin;
            _serverSide.OnAutoConnectError += HandleOnAutoConnectError;
            _serverSide.OnAway += HandleOnAway;
            _serverSide.OnBan += HandleOnBan;
            _serverSide.OnChannelAction += HandleOnChannelAction;
            _serverSide.OnChannelActiveSynced += HandleOnChannelActiveSynced;
            _serverSide.OnChannelMessage += HandleOnChannelMessage;
            _serverSide.OnChannelModeChange += HandleOnChannelModeChange;
            _serverSide.OnChannelNotice += HandleOnChannelNotice;
            _serverSide.OnChannelPassiveSynced += HandleOnChannelPassiveSynced;
            _serverSide.OnConnected += HandleOnConnected;
            _serverSide.OnConnecting += HandleOnConnecting;
            _serverSide.OnConnectionError += HandleOnConnectionError;
            _serverSide.OnCtcpReply += HandleOnCtcpReply;
            _serverSide.OnCtcpRequest += HandleOnCtcpRequest;
            _serverSide.OnDccChatReceiveLineEvent += HandleOnDccChatReceiveLineEvent;
            _serverSide.OnDccChatRequestEvent += HandleOnDccChatRequestEvent;
            _serverSide.OnDccChatSentLineEvent += HandleOnDccChatSentLineEvent;
            _serverSide.OnDccChatStartEvent += HandleOnDccChatStartEvent;
            _serverSide.OnDccChatStopEvent += HandleOnDccChatStopEvent;
            _serverSide.OnDccSendReceiveBlockEvent += HandleOnDccSendReceiveBlockEvent;
            _serverSide.OnDccSendRequestEvent += HandleOnDccSendRequestEvent;
            _serverSide.OnDccSendSentBlockEvent += HandleOnDccSendSentBlockEvent;
            _serverSide.OnDccSendStartEvent += HandleOnDccSendStartEvent;
            _serverSide.OnDccSendStopEvent += HandleOnDccSendStopEvent;
            _serverSide.OnDeadmin += HandleOnDeadmin;
            _serverSide.OnDehalfop += HandleOnDehalfop;
            _serverSide.OnDeop += HandleOnDeop;
            _serverSide.OnDeowner += HandleOnDeowner;
            _serverSide.OnDevoice += HandleOnDevoice;
            _serverSide.OnDisconnected += HandleOnDisconnected;
            _serverSide.OnDisconnecting += HandleOnDisconnecting;
            _serverSide.OnError += HandleOnError;
            _serverSide.OnErrorMessage += HandleOnErrorMessage;
            _serverSide.OnHalfop += HandleOnHalfop;
            _serverSide.OnInvite += HandleOnInvite;
            _serverSide.OnJoin += HandleOnJoin;
            _serverSide.OnKick += HandleOnKick;
            _serverSide.OnList += HandleOnList;
            _serverSide.OnModeChange += HandleOnModeChange;
            _serverSide.OnMotd += HandleOnMotd;
            _serverSide.OnNames += HandleOnNames;
            _serverSide.OnNickChange += HandleOnNickChange;
            _serverSide.OnNowAway += HandleOnNowAway;
            _serverSide.OnOp += HandleOnOp;
            _serverSide.OnOwner += HandleOnOwner;
            _serverSide.OnPart += HandleOnPart;
            _serverSide.OnPing += HandleOnPing;
            _serverSide.OnPong += HandleOnPong;
            _serverSide.OnQueryAction += HandleOnQueryAction;
            _serverSide.OnQueryMessage += HandleOnQueryMessage;
            _serverSide.OnQueryNotice += HandleOnQueryNotice;
            _serverSide.OnQuit += HandleOnQuit;
            _serverSide.OnRawMessage += HandleOnRawMessage;
            _serverSide.OnReadLine += HandleOnReadLine;
            _serverSide.OnRegistered += HandleOnRegistered;
            _serverSide.OnTopic += HandleOnTopic;
            _serverSide.OnTopicChange += HandleOnTopicChange;
            _serverSide.OnUnAway += HandleOnUnAway;
            _serverSide.OnUnban += HandleOnUnban;
            _serverSide.OnUserModeChange += HandleOnUserModeChange;
            _serverSide.OnVoice += HandleOnVoice;
            _serverSide.OnWho += HandleOnWho;
            _serverSide.OnWriteLine += HandleOnWriteLine;
            _serverSide.SupportNonRfcChanged += HandleSupportNonRfcChanged;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Unload()
        {
            _serverSide.Unload();

            _serverSide.OnAdmin -= HandleOnAdmin;
            _serverSide.OnAutoConnectError -= HandleOnAutoConnectError;
            _serverSide.OnAway -= HandleOnAway;
            _serverSide.OnBan -= HandleOnBan;
            _serverSide.OnChannelAction -= HandleOnChannelAction;
            _serverSide.OnChannelActiveSynced -= HandleOnChannelActiveSynced;
            _serverSide.OnChannelMessage -= HandleOnChannelMessage;
            _serverSide.OnChannelModeChange -= HandleOnChannelModeChange;
            _serverSide.OnChannelNotice -= HandleOnChannelNotice;
            _serverSide.OnChannelPassiveSynced -= HandleOnChannelPassiveSynced;
            _serverSide.OnConnected -= HandleOnConnected;
            _serverSide.OnConnecting -= HandleOnConnecting;
            _serverSide.OnConnectionError -= HandleOnConnectionError;
            _serverSide.OnCtcpReply -= HandleOnCtcpReply;
            _serverSide.OnCtcpRequest -= HandleOnCtcpRequest;
            _serverSide.OnDccChatReceiveLineEvent -= HandleOnDccChatReceiveLineEvent;
            _serverSide.OnDccChatRequestEvent -= HandleOnDccChatRequestEvent;
            _serverSide.OnDccChatSentLineEvent -= HandleOnDccChatSentLineEvent;
            _serverSide.OnDccChatStartEvent -= HandleOnDccChatStartEvent;
            _serverSide.OnDccChatStopEvent -= HandleOnDccChatStopEvent;
            _serverSide.OnDccSendReceiveBlockEvent -= HandleOnDccSendReceiveBlockEvent;
            _serverSide.OnDccSendRequestEvent -= HandleOnDccSendRequestEvent;
            _serverSide.OnDccSendSentBlockEvent -= HandleOnDccSendSentBlockEvent;
            _serverSide.OnDccSendStartEvent -= HandleOnDccSendStartEvent;
            _serverSide.OnDccSendStopEvent -= HandleOnDccSendStopEvent;
            _serverSide.OnDeadmin -= HandleOnDeadmin;
            _serverSide.OnDehalfop -= HandleOnDehalfop;
            _serverSide.OnDeop -= HandleOnDeop;
            _serverSide.OnDeowner -= HandleOnDeowner;
            _serverSide.OnDevoice -= HandleOnDevoice;
            _serverSide.OnDisconnected -= HandleOnDisconnected;
            _serverSide.OnDisconnecting -= HandleOnDisconnecting;
            _serverSide.OnError -= HandleOnError;
            _serverSide.OnErrorMessage -= HandleOnErrorMessage;
            _serverSide.OnHalfop -= HandleOnHalfop;
            _serverSide.OnInvite -= HandleOnInvite;
            _serverSide.OnJoin -= HandleOnJoin;
            _serverSide.OnKick -= HandleOnKick;
            _serverSide.OnList -= HandleOnList;
            _serverSide.OnModeChange -= HandleOnModeChange;
            _serverSide.OnMotd -= HandleOnMotd;
            _serverSide.OnNames -= HandleOnNames;
            _serverSide.OnNickChange -= HandleOnNickChange;
            _serverSide.OnNowAway -= HandleOnNowAway;
            _serverSide.OnOp -= HandleOnOp;
            _serverSide.OnOwner -= HandleOnOwner;
            _serverSide.OnPart -= HandleOnPart;
            _serverSide.OnPing -= HandleOnPing;
            _serverSide.OnPong -= HandleOnPong;
            _serverSide.OnQueryAction -= HandleOnQueryAction;
            _serverSide.OnQueryMessage -= HandleOnQueryMessage;
            _serverSide.OnQueryNotice -= HandleOnQueryNotice;
            _serverSide.OnQuit -= HandleOnQuit;
            _serverSide.OnRawMessage -= HandleOnRawMessage;
            _serverSide.OnReadLine -= HandleOnReadLine;
            _serverSide.OnRegistered -= HandleOnRegistered;
            _serverSide.OnTopic -= HandleOnTopic;
            _serverSide.OnTopicChange -= HandleOnTopicChange;
            _serverSide.OnUnAway -= HandleOnUnAway;
            _serverSide.OnUnban -= HandleOnUnban;
            _serverSide.OnUserModeChange -= HandleOnUserModeChange;
            _serverSide.OnVoice -= HandleOnVoice;
            _serverSide.OnWho -= HandleOnWho;
            _serverSide.OnWriteLine -= HandleOnWriteLine;
            _serverSide.SupportNonRfcChanged -= HandleSupportNonRfcChanged;
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
            OnAdmin?.Invoke(this, e);
        }

        internal void HandleOnAutoConnectError(object sender, AutoConnectErrorEventArgs e)
        {
            OnAutoConnectError?.Invoke(this, e);
        }

        internal void HandleOnAway(object sender, AwayEventArgs e)
        {
            OnAway?.Invoke(this, e);
        }

        internal void HandleOnBan(object sender, BanEventArgs e)
        {
            OnBan?.Invoke(this, e);
        }

        internal void HandleOnChannelAction(object sender, ActionEventArgs e)
        {
            OnChannelAction?.Invoke(this, e);
        }

        internal void HandleOnChannelActiveSynced(object sender, IrcEventArgs e)
        {
            OnChannelActiveSynced?.Invoke(this, e);
        }

        internal void HandleOnChannelMessage(object sender, IrcEventArgs e)
        {
            OnChannelMessage?.Invoke(this, e);
        }

        internal void HandleOnChannelModeChange(object sender, IrcEventArgs e)
        {
            OnChannelModeChange?.Invoke(this, e);
        }

        internal void HandleOnChannelNotice(object sender, IrcEventArgs e)
        {
            OnChannelNotice?.Invoke(this, e);
        }

        internal void HandleOnChannelPassiveSynced(object sender, IrcEventArgs e)
        {
            OnChannelPassiveSynced?.Invoke(this, e);
        }

        internal void HandleOnConnected(object sender, EventArgs e)
        {
            OnConnected?.Invoke(this, e);
        }

        internal void HandleOnConnecting(object sender, EventArgs e)
        {
            OnConnecting?.Invoke(this, e);
        }

        internal void HandleOnConnectionError(object sender, EventArgs e)
        {
            OnConnectionError?.Invoke(this, e);
        }

        internal void HandleOnCtcpReply(object sender, CtcpEventArgs e)
        {
            OnCtcpReply?.Invoke(this, e);
        }

        internal void HandleOnCtcpRequest(object sender, CtcpEventArgs e)
        {
            OnCtcpRequest?.Invoke(this, e);
        }

        internal void HandleOnDccChatReceiveLineEvent(object sender, DccChatEventArgs e)
        {
            OnDccChatReceiveLineEvent?.Invoke(this, e);
        }

        internal void HandleOnDccChatRequestEvent(object sender, DccEventArgs e)
        {
            OnDccChatRequestEvent?.Invoke(this, e);
        }

        internal void HandleOnDccChatSentLineEvent(object sender, DccChatEventArgs e)
        {
            OnDccChatSentLineEvent?.Invoke(this, e);
        }

        internal void HandleOnDccChatStartEvent(object sender, DccEventArgs e)
        {
            OnDccChatStartEvent?.Invoke(this, e);
        }

        internal void HandleOnDccChatStopEvent(object sender, DccEventArgs e)
        {
            OnDccChatStopEvent?.Invoke(this, e);
        }

        internal void HandleOnDccSendReceiveBlockEvent(object sender, DccSendEventArgs e)
        {
            OnDccSendReceiveBlockEvent?.Invoke(this, e);
        }

        internal void HandleOnDccSendRequestEvent(object sender, DccSendRequestEventArgs e)
        {
            OnDccSendRequestEvent?.Invoke(this, e);
        }

        internal void HandleOnDccSendSentBlockEvent(object sender, DccSendEventArgs e)
        {
            OnDccSendSentBlockEvent?.Invoke(this, e);
        }

        internal void HandleOnDccSendStartEvent(object sender, DccEventArgs e)
        {
            OnDccSendStartEvent?.Invoke(this, e);
        }

        internal void HandleOnDccSendStopEvent(object sender, DccEventArgs e)
        {
            OnDccSendStopEvent?.Invoke(this, e);
        }

        internal void HandleOnDeadmin(object sender, DeadminEventArgs e)
        {
            OnDeadmin?.Invoke(this, e);
        }

        internal void HandleOnDehalfop(object sender, DehalfopEventArgs e)
        {
            OnDehalfop?.Invoke(this, e);
        }

        internal void HandleOnDeop(object sender, DeopEventArgs e)
        {
            OnDeop?.Invoke(this, e);
        }

        internal void HandleOnDeowner(object sender, DeownerEventArgs e)
        {
            OnDeowner?.Invoke(this, e);
        }

        internal void HandleOnDevoice(object sender, DevoiceEventArgs e)
        {
            OnDevoice?.Invoke(this, e);
        }

        internal void HandleOnDisconnected(object sender, EventArgs e)
        {
            OnDisconnected?.Invoke(this, e);
        }

        internal void HandleOnDisconnecting(object sender, EventArgs e)
        {
            OnDisconnecting?.Invoke(this, e);
        }

        internal void HandleOnError(object sender, ErrorEventArgs e)
        {
            OnError?.Invoke(this, e);
        }

        internal void HandleOnErrorMessage(object sender, IrcEventArgs e)
        {
            OnErrorMessage?.Invoke(this, e);
        }

        internal void HandleOnHalfop(object sender, HalfopEventArgs e)
        {
            OnHalfop?.Invoke(this, e);
        }

        internal void HandleOnInvite(object sender, InviteEventArgs e)
        {
            OnInvite?.Invoke(this, e);
        }

        internal void HandleOnJoin(object sender, JoinEventArgs e)
        {
            OnJoin?.Invoke(this, e);
        }

        internal void HandleOnKick(object sender, KickEventArgs e)
        {
            OnKick?.Invoke(this, e);
        }

        internal void HandleOnList(object sender, ListEventArgs e)
        {
            OnList?.Invoke(this, e);
        }

        internal void HandleOnModeChange(object sender, IrcEventArgs e)
        {
            OnModeChange?.Invoke(this, e);
        }

        internal void HandleOnMotd(object sender, MotdEventArgs e)
        {
            OnMotd?.Invoke(this, e);
        }

        internal void HandleOnNames(object sender, NamesEventArgs e)
        {
            OnNames?.Invoke(this, e);
        }

        internal void HandleOnNickChange(object sender, NickChangeEventArgs e)
        {
            OnNickChange?.Invoke(this, e);
        }

        internal void HandleOnNowAway(object sender, IrcEventArgs e)
        {
            OnNowAway?.Invoke(this, e);
        }

        internal void HandleOnOp(object sender, OpEventArgs e)
        {
            OnOp?.Invoke(this, e);
        }

        internal void HandleOnOwner(object sender, OwnerEventArgs e)
        {
            OnOwner?.Invoke(this, e);
        }

        internal void HandleOnPart(object sender, PartEventArgs e)
        {
            OnPart?.Invoke(this, e);
        }

        internal void HandleOnPing(object sender, PingEventArgs e)
        {
            OnPing?.Invoke(this, e);
        }

        internal void HandleOnPong(object sender, PongEventArgs e)
        {
            OnPong?.Invoke(this, e);
        }

        internal void HandleOnQueryAction(object sender, ActionEventArgs e)
        {
            OnQueryAction?.Invoke(this, e);
        }

        internal void HandleOnQueryMessage(object sender, IrcEventArgs e)
        {
            OnQueryMessage?.Invoke(this, e);
        }

        internal void HandleOnQueryNotice(object sender, IrcEventArgs e)
        {
            OnQueryNotice?.Invoke(this, e);
        }

        internal void HandleOnQuit(object sender, QuitEventArgs e)
        {
            OnQuit?.Invoke(this, e);
        }

        internal void HandleOnRawMessage(object sender, IrcEventArgs e)
        {
            OnRawMessage?.Invoke(this, e);
        }

        internal void HandleOnReadLine(object sender, ReadLineEventArgs e)
        {
            OnReadLine?.Invoke(this, e);
        }

        internal void HandleOnRegistered(object sender, EventArgs e)
        {
            OnRegistered?.Invoke(this, e);
        }

        internal void HandleOnTopic(object sender, TopicEventArgs e)
        {
            OnTopic?.Invoke(this, e);
        }

        internal void HandleOnTopicChange(object sender, TopicChangeEventArgs e)
        {
            OnTopicChange?.Invoke(this, e);
        }

        internal void HandleOnUnAway(object sender, IrcEventArgs e)
        {
            OnUnAway?.Invoke(this, e);
        }

        internal void HandleOnUnban(object sender, UnbanEventArgs e)
        {
            OnUnban?.Invoke(this, e);
        }

        internal void HandleOnUserModeChange(object sender, IrcEventArgs e)
        {
            OnUserModeChange?.Invoke(this, e);
        }

        internal void HandleOnVoice(object sender, VoiceEventArgs e)
        {
            OnVoice?.Invoke(this, e);
        }

        internal void HandleOnWho(object sender, WhoEventArgs e)
        {
            OnWho?.Invoke(this, e);
        }

        internal void HandleOnWriteLine(object sender, WriteLineEventArgs e)
        {
            OnWriteLine?.Invoke(this, e);
        }

        internal void HandleSupportNonRfcChanged(object sender, EventArgs e)
        {
            SupportNonRfcChanged?.Invoke(this, e);
        }
    }
}