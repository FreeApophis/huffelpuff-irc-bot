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
using SharpIrc;
using SharpIrc.IrcClient.EventArgs;
using SharpIrc.IrcConnection;
using SharpIrc.IrcFeatures.EventArgs;


namespace Huffelpuff.Plugins
{
    /// <summary>
    /// Description of SharedServerSide.
    /// </summary>
    public class SharedServerSide : MarshalByRefObject
    {
        private readonly IrcBot _bot;

        public SharedServerSide(IrcBot botInstance)
        {
            _bot = botInstance;

            botInstance.OnAdmin += PluginsOnAdmin;
            botInstance.OnAutoConnectError += PluginsOnAutoConnectError;
            botInstance.OnAway += PluginsOnAway;
            botInstance.OnBan += PluginsOnBan;
            botInstance.OnChannelAction += PluginsOnChannelAction;
            botInstance.OnChannelActiveSynced += PluginsOnChannelActiveSynced;
            botInstance.OnChannelMessage += PluginsOnChannelMessage;
            botInstance.OnChannelModeChange += PluginsOnChannelModeChange;
            botInstance.OnChannelNotice += PluginsOnChannelNotice;
            botInstance.OnChannelPassiveSynced += PluginsOnChannelPassiveSynced;
            botInstance.OnConnected += PluginsOnConnected;
            botInstance.OnConnecting += PluginsOnConnecting;
            botInstance.OnConnectionError += PluginsOnConnectionError;
            botInstance.OnCtcpReply += PluginsOnCtcpReply;
            botInstance.OnCtcpRequest += PluginsOnCtcpRequest;
            botInstance.OnDccChatReceiveLineEvent += PluginsOnDccChatReceiveLineEvent;
            botInstance.OnDccChatRequestEvent += PluginsOnDccChatRequestEvent;
            botInstance.OnDccChatSentLineEvent += PluginsOnDccChatSentLineEvent;
            botInstance.OnDccChatStartEvent += PluginsOnDccChatStartEvent;
            botInstance.OnDccChatStopEvent += PluginsOnDccChatStopEvent;
            botInstance.OnDccSendReceiveBlockEvent += PluginsOnDccSendReceiveBlockEvent;
            botInstance.OnDccSendRequestEvent += PluginsOnDccSendRequestEvent;
            botInstance.OnDccSendSentBlockEvent += PluginsOnDccSendSentBlockEvent;
            botInstance.OnDccSendStartEvent += PluginsOnDccSendStartEvent;
            botInstance.OnDccSendStopEvent += PluginsOnDccSendStopEvent;
            botInstance.OnDeadmin += PluginsOnDeadmin;
            botInstance.OnDehalfop += PluginsOnDehalfop;
            botInstance.OnDeop += PluginsOnDeop;
            botInstance.OnDeowner += PluginsOnDeowner;
            botInstance.OnDevoice += PluginsOnDevoice;
            botInstance.OnDisconnected += PluginsOnDisconnected;
            botInstance.OnDisconnecting += PluginsOnDisconnecting;
            botInstance.OnError += PluginsOnError;
            botInstance.OnErrorMessage += PluginsOnErrorMessage;
            botInstance.OnHalfop += PluginsOnHalfop;
            botInstance.OnInvite += PluginsOnInvite;
            botInstance.OnJoin += PluginsOnJoin;
            botInstance.OnKick += PluginsOnKick;
            botInstance.OnList += PluginsOnList;
            botInstance.OnModeChange += PluginsOnModeChange;
            botInstance.OnMotd += PluginsOnMotd;
            botInstance.OnNames += PluginsOnNames;
            botInstance.OnNickChange += PluginsOnNickChange;
            botInstance.OnNowAway += PluginsOnNowAway;
            botInstance.OnOp += PluginsOnOp;
            botInstance.OnOwner += PluginsOnOwner;
            botInstance.OnPart += PluginsOnPart;
            botInstance.OnPing += PluginsOnPing;
            botInstance.OnPong += PluginsOnPong;
            botInstance.OnQueryAction += PluginsOnQueryAction;
            botInstance.OnQueryMessage += PluginsOnQueryMessage;
            botInstance.OnQueryNotice += PluginsOnQueryNotice;
            botInstance.OnQuit += PluginsOnQuit;
            botInstance.OnRawMessage += PluginsOnRawMessage;
            botInstance.OnReadLine += PluginsOnReadLine;
            botInstance.OnRegistered += PluginsOnRegistered;
            botInstance.OnTopic += PluginsOnTopic;
            botInstance.OnTopicChange += PluginsOnTopicChange;
            botInstance.OnUnAway += PluginsOnUnAway;
            botInstance.OnUnban += PluginsOnUnban;
            botInstance.OnUserModeChange += PluginsOnUserModeChange;
            botInstance.OnVoice += PluginsOnVoice;
            botInstance.OnWho += PluginsOnWho;
            botInstance.OnWriteLine += PluginsOnWriteLine;
            botInstance.SupportNonRfcChanged += PluginsSupportNonRfcChanged;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Unload()
        {
            _bot.OnAdmin -= PluginsOnAdmin;
            _bot.OnAutoConnectError -= PluginsOnAutoConnectError;
            _bot.OnAway -= PluginsOnAway;
            _bot.OnBan -= PluginsOnBan;
            _bot.OnChannelAction -= PluginsOnChannelAction;
            _bot.OnChannelActiveSynced -= PluginsOnChannelActiveSynced;
            _bot.OnChannelMessage -= PluginsOnChannelMessage;
            _bot.OnChannelModeChange -= PluginsOnChannelModeChange;
            _bot.OnChannelNotice -= PluginsOnChannelNotice;
            _bot.OnChannelPassiveSynced -= PluginsOnChannelPassiveSynced;
            _bot.OnConnected -= PluginsOnConnected;
            _bot.OnConnecting -= PluginsOnConnecting;
            _bot.OnConnectionError -= PluginsOnConnectionError;
            _bot.OnCtcpReply -= PluginsOnCtcpReply;
            _bot.OnCtcpRequest -= PluginsOnCtcpRequest;
            _bot.OnDccChatReceiveLineEvent -= PluginsOnDccChatReceiveLineEvent;
            _bot.OnDccChatRequestEvent -= PluginsOnDccChatRequestEvent;
            _bot.OnDccChatSentLineEvent -= PluginsOnDccChatSentLineEvent;
            _bot.OnDccChatStartEvent -= PluginsOnDccChatStartEvent;
            _bot.OnDccChatStopEvent -= PluginsOnDccChatStopEvent;
            _bot.OnDccSendReceiveBlockEvent -= PluginsOnDccSendReceiveBlockEvent;
            _bot.OnDccSendRequestEvent -= PluginsOnDccSendRequestEvent;
            _bot.OnDccSendSentBlockEvent -= PluginsOnDccSendSentBlockEvent;
            _bot.OnDccSendStartEvent -= PluginsOnDccSendStartEvent;
            _bot.OnDccSendStopEvent -= PluginsOnDccSendStopEvent;
            _bot.OnDeadmin -= PluginsOnDeadmin;
            _bot.OnDehalfop -= PluginsOnDehalfop;
            _bot.OnDeop -= PluginsOnDeop;
            _bot.OnDeowner -= PluginsOnDeowner;
            _bot.OnDevoice -= PluginsOnDevoice;
            _bot.OnDisconnected -= PluginsOnDisconnected;
            _bot.OnDisconnecting -= PluginsOnDisconnecting;
            _bot.OnError -= PluginsOnError;
            _bot.OnErrorMessage -= PluginsOnErrorMessage;
            _bot.OnHalfop -= PluginsOnHalfop;
            _bot.OnInvite -= PluginsOnInvite;
            _bot.OnJoin -= PluginsOnJoin;
            _bot.OnKick -= PluginsOnKick;
            _bot.OnList -= PluginsOnList;
            _bot.OnModeChange -= PluginsOnModeChange;
            _bot.OnMotd -= PluginsOnMotd;
            _bot.OnNames -= PluginsOnNames;
            _bot.OnNickChange -= PluginsOnNickChange;
            _bot.OnNowAway -= PluginsOnNowAway;
            _bot.OnOp -= PluginsOnOp;
            _bot.OnOwner -= PluginsOnOwner;
            _bot.OnPart -= PluginsOnPart;
            _bot.OnPing -= PluginsOnPing;
            _bot.OnPong -= PluginsOnPong;
            _bot.OnQueryAction -= PluginsOnQueryAction;
            _bot.OnQueryMessage -= PluginsOnQueryMessage;
            _bot.OnQueryNotice -= PluginsOnQueryNotice;
            _bot.OnQuit -= PluginsOnQuit;
            _bot.OnRawMessage -= PluginsOnRawMessage;
            _bot.OnReadLine -= PluginsOnReadLine;
            _bot.OnRegistered -= PluginsOnRegistered;
            _bot.OnTopic -= PluginsOnTopic;
            _bot.OnTopicChange -= PluginsOnTopicChange;
            _bot.OnUnAway -= PluginsOnUnAway;
            _bot.OnUnban -= PluginsOnUnban;
            _bot.OnUserModeChange -= PluginsOnUserModeChange;
            _bot.OnVoice -= PluginsOnVoice;
            _bot.OnWho -= PluginsOnWho;
            _bot.OnWriteLine -= PluginsOnWriteLine;
            _bot.SupportNonRfcChanged -= PluginsSupportNonRfcChanged;
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

        public void PluginsOnAdmin(object sender, AdminEventArgs e)
        {
            OnAdmin?.Invoke(this, e);
        }

        public void PluginsOnAutoConnectError(object sender, AutoConnectErrorEventArgs e)
        {
            OnAutoConnectError?.Invoke(this, e);
        }

        public void PluginsOnAway(object sender, AwayEventArgs e)
        {
            OnAway?.Invoke(this, e);
        }

        public void PluginsOnBan(object sender, BanEventArgs e)
        {
            OnBan?.Invoke(this, e);
        }

        public void PluginsOnChannelAction(object sender, ActionEventArgs e)
        {
            OnChannelAction?.Invoke(this, e);
        }

        public void PluginsOnChannelActiveSynced(object sender, IrcEventArgs e)
        {
            OnChannelActiveSynced?.Invoke(this, e);
        }

        public void PluginsOnChannelMessage(object sender, IrcEventArgs e)
        {
            OnChannelMessage?.Invoke(this, e);
        }

        public void PluginsOnChannelModeChange(object sender, IrcEventArgs e)
        {
            OnChannelModeChange?.Invoke(this, e);
        }

        public void PluginsOnChannelNotice(object sender, IrcEventArgs e)
        {
            OnChannelNotice?.Invoke(this, e);
        }

        public void PluginsOnChannelPassiveSynced(object sender, IrcEventArgs e)
        {
            OnChannelPassiveSynced?.Invoke(this, e);
        }

        public void PluginsOnConnected(object sender, EventArgs e)
        {
            OnConnected?.Invoke(this, e);
        }

        public void PluginsOnConnecting(object sender, EventArgs e)
        {
            OnConnecting?.Invoke(this, e);
        }

        public void PluginsOnConnectionError(object sender, EventArgs e)
        {
            OnConnectionError?.Invoke(this, e);
        }

        public void PluginsOnCtcpReply(object sender, CtcpEventArgs e)
        {
            OnCtcpReply?.Invoke(this, e);
        }

        public void PluginsOnCtcpRequest(object sender, CtcpEventArgs e)
        {
            OnCtcpRequest?.Invoke(this, e);
        }

        public void PluginsOnDccChatReceiveLineEvent(object sender, DccChatEventArgs e)
        {
            OnDccChatReceiveLineEvent?.Invoke(this, e);
        }

        public void PluginsOnDccChatRequestEvent(object sender, DccEventArgs e)
        {
            OnDccChatRequestEvent?.Invoke(this, e);
        }

        public void PluginsOnDccChatSentLineEvent(object sender, DccChatEventArgs e)
        {
            OnDccChatSentLineEvent?.Invoke(this, e);
        }

        public void PluginsOnDccChatStartEvent(object sender, DccEventArgs e)
        {
            OnDccChatStartEvent?.Invoke(this, e);
        }

        public void PluginsOnDccChatStopEvent(object sender, DccEventArgs e)
        {
            OnDccChatStopEvent?.Invoke(this, e);
        }

        public void PluginsOnDccSendReceiveBlockEvent(object sender, DccSendEventArgs e)
        {
            OnDccSendReceiveBlockEvent?.Invoke(this, e);
        }

        public void PluginsOnDccSendRequestEvent(object sender, DccSendRequestEventArgs e)
        {
            OnDccSendRequestEvent?.Invoke(this, e);
        }

        public void PluginsOnDccSendSentBlockEvent(object sender, DccSendEventArgs e)
        {
            OnDccSendSentBlockEvent?.Invoke(this, e);
        }

        public void PluginsOnDccSendStartEvent(object sender, DccEventArgs e)
        {
            OnDccSendStartEvent?.Invoke(this, e);
        }

        public void PluginsOnDccSendStopEvent(object sender, DccEventArgs e)
        {
            OnDccSendStopEvent?.Invoke(this, e);
        }

        public void PluginsOnDeadmin(object sender, DeadminEventArgs e)
        {
            OnDeadmin?.Invoke(this, e);
        }

        public void PluginsOnDehalfop(object sender, DehalfopEventArgs e)
        {
            OnDehalfop?.Invoke(this, e);
        }

        public void PluginsOnDeop(object sender, DeopEventArgs e)
        {
            OnDeop?.Invoke(this, e);
        }

        public void PluginsOnDeowner(object sender, DeownerEventArgs e)
        {
            OnDeowner?.Invoke(this, e);
        }

        public void PluginsOnDevoice(object sender, DevoiceEventArgs e)
        {
            OnDevoice?.Invoke(this, e);
        }

        public void PluginsOnDisconnected(object sender, EventArgs e)
        {
            OnDisconnected?.Invoke(this, e);
        }

        public void PluginsOnDisconnecting(object sender, EventArgs e)
        {
            OnDisconnecting?.Invoke(this, e);
        }

        public void PluginsOnError(object sender, ErrorEventArgs e)
        {
            OnError?.Invoke(this, e);
        }

        public void PluginsOnErrorMessage(object sender, IrcEventArgs e)
        {
            OnErrorMessage?.Invoke(this, e);
        }

        public void PluginsOnHalfop(object sender, HalfopEventArgs e)
        {
            OnHalfop?.Invoke(this, e);
        }

        public void PluginsOnInvite(object sender, InviteEventArgs e)
        {
            OnInvite?.Invoke(this, e);
        }

        public void PluginsOnJoin(object sender, JoinEventArgs e)
        {
            OnJoin?.Invoke(this, e);
        }

        public void PluginsOnKick(object sender, KickEventArgs e)
        {
            OnKick?.Invoke(this, e);
        }

        public void PluginsOnList(object sender, ListEventArgs e)
        {
            OnList?.Invoke(this, e);
        }

        public void PluginsOnModeChange(object sender, IrcEventArgs e)
        {
            OnModeChange?.Invoke(this, e);
        }

        public void PluginsOnMotd(object sender, MotdEventArgs e)
        {
            OnMotd?.Invoke(this, e);
        }

        public void PluginsOnNames(object sender, NamesEventArgs e)
        {
            OnNames?.Invoke(this, e);
        }

        public void PluginsOnNickChange(object sender, NickChangeEventArgs e)
        {
            OnNickChange?.Invoke(this, e);
        }

        public void PluginsOnNowAway(object sender, IrcEventArgs e)
        {
            OnNowAway?.Invoke(this, e);
        }

        public void PluginsOnOp(object sender, OpEventArgs e)
        {
            OnOp?.Invoke(this, e);
        }

        public void PluginsOnOwner(object sender, OwnerEventArgs e)
        {
            OnOwner?.Invoke(this, e);
        }

        public void PluginsOnPart(object sender, PartEventArgs e)
        {
            OnPart?.Invoke(this, e);
        }

        public void PluginsOnPing(object sender, PingEventArgs e)
        {
            OnPing?.Invoke(this, e);
        }

        public void PluginsOnPong(object sender, PongEventArgs e)
        {
            OnPong?.Invoke(this, e);
        }

        public void PluginsOnQueryAction(object sender, ActionEventArgs e)
        {
            OnQueryAction?.Invoke(this, e);
        }

        public void PluginsOnQueryMessage(object sender, IrcEventArgs e)
        {
            OnQueryMessage?.Invoke(this, e);
        }

        public void PluginsOnQueryNotice(object sender, IrcEventArgs e)
        {
            OnQueryNotice?.Invoke(this, e);
        }

        public void PluginsOnQuit(object sender, QuitEventArgs e)
        {
            OnQuit?.Invoke(this, e);
        }

        public void PluginsOnRawMessage(object sender, IrcEventArgs e)
        {
            OnRawMessage?.Invoke(this, e);
        }

        public void PluginsOnReadLine(object sender, ReadLineEventArgs e)
        {
            OnReadLine?.Invoke(this, e);
        }

        public void PluginsOnRegistered(object sender, EventArgs e)
        {
            OnRegistered?.Invoke(this, e);
        }

        public void PluginsOnTopic(object sender, TopicEventArgs e)
        {
            OnTopic?.Invoke(this, e);
        }

        public void PluginsOnTopicChange(object sender, TopicChangeEventArgs e)
        {
            OnTopicChange?.Invoke(this, e);
        }

        public void PluginsOnUnAway(object sender, IrcEventArgs e)
        {
            OnUnAway?.Invoke(this, e);
        }

        public void PluginsOnUnban(object sender, UnbanEventArgs e)
        {
            OnUnban?.Invoke(this, e);
        }

        public void PluginsOnUserModeChange(object sender, IrcEventArgs e)
        {
            OnUserModeChange?.Invoke(this, e);
        }

        public void PluginsOnVoice(object sender, VoiceEventArgs e)
        {
            OnVoice?.Invoke(this, e);
        }

        public void PluginsOnWho(object sender, WhoEventArgs e)
        {
            OnWho?.Invoke(this, e);
        }

        public void PluginsOnWriteLine(object sender, WriteLineEventArgs e)
        {
            OnWriteLine?.Invoke(this, e);
        }

        public void PluginsSupportNonRfcChanged(object sender, EventArgs e)
        {
            SupportNonRfcChanged?.Invoke(this, e);
        }
    }
}