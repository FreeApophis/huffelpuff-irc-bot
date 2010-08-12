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
    /// Description of SharedServerSide.
    /// </summary>
    public class SharedServerSide : MarshalByRefObject
    {
        private readonly IrcBot bot;

        public SharedServerSide(IrcBot botInstance)
        {
            bot = botInstance;

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
            bot.OnAdmin -= PluginsOnAdmin;
            bot.OnAutoConnectError -= PluginsOnAutoConnectError;
            bot.OnAway -= PluginsOnAway;
            bot.OnBan -= PluginsOnBan;
            bot.OnChannelAction -= PluginsOnChannelAction;
            bot.OnChannelActiveSynced -= PluginsOnChannelActiveSynced;
            bot.OnChannelMessage -= PluginsOnChannelMessage;
            bot.OnChannelModeChange -= PluginsOnChannelModeChange;
            bot.OnChannelNotice -= PluginsOnChannelNotice;
            bot.OnChannelPassiveSynced -= PluginsOnChannelPassiveSynced;
            bot.OnConnected -= PluginsOnConnected;
            bot.OnConnecting -= PluginsOnConnecting;
            bot.OnConnectionError -= PluginsOnConnectionError;
            bot.OnCtcpReply -= PluginsOnCtcpReply;
            bot.OnCtcpRequest -= PluginsOnCtcpRequest;
            bot.OnDccChatReceiveLineEvent -= PluginsOnDccChatReceiveLineEvent;
            bot.OnDccChatRequestEvent -= PluginsOnDccChatRequestEvent;
            bot.OnDccChatSentLineEvent -= PluginsOnDccChatSentLineEvent;
            bot.OnDccChatStartEvent -= PluginsOnDccChatStartEvent;
            bot.OnDccChatStopEvent -= PluginsOnDccChatStopEvent;
            bot.OnDccSendReceiveBlockEvent -= PluginsOnDccSendReceiveBlockEvent;
            bot.OnDccSendRequestEvent -= PluginsOnDccSendRequestEvent;
            bot.OnDccSendSentBlockEvent -= PluginsOnDccSendSentBlockEvent;
            bot.OnDccSendStartEvent -= PluginsOnDccSendStartEvent;
            bot.OnDccSendStopEvent -= PluginsOnDccSendStopEvent;
            bot.OnDeadmin -= PluginsOnDeadmin;
            bot.OnDehalfop -= PluginsOnDehalfop;
            bot.OnDeop -= PluginsOnDeop;
            bot.OnDeowner -= PluginsOnDeowner;
            bot.OnDevoice -= PluginsOnDevoice;
            bot.OnDisconnected -= PluginsOnDisconnected;
            bot.OnDisconnecting -= PluginsOnDisconnecting;
            bot.OnError -= PluginsOnError;
            bot.OnErrorMessage -= PluginsOnErrorMessage;
            bot.OnHalfop -= PluginsOnHalfop;
            bot.OnInvite -= PluginsOnInvite;
            bot.OnJoin -= PluginsOnJoin;
            bot.OnKick -= PluginsOnKick;
            bot.OnList -= PluginsOnList;
            bot.OnModeChange -= PluginsOnModeChange;
            bot.OnMotd -= PluginsOnMotd;
            bot.OnNames -= PluginsOnNames;
            bot.OnNickChange -= PluginsOnNickChange;
            bot.OnNowAway -= PluginsOnNowAway;
            bot.OnOp -= PluginsOnOp;
            bot.OnOwner -= PluginsOnOwner;
            bot.OnPart -= PluginsOnPart;
            bot.OnPing -= PluginsOnPing;
            bot.OnPong -= PluginsOnPong;
            bot.OnQueryAction -= PluginsOnQueryAction;
            bot.OnQueryMessage -= PluginsOnQueryMessage;
            bot.OnQueryNotice -= PluginsOnQueryNotice;
            bot.OnQuit -= PluginsOnQuit;
            bot.OnRawMessage -= PluginsOnRawMessage;
            bot.OnReadLine -= PluginsOnReadLine;
            bot.OnRegistered -= PluginsOnRegistered;
            bot.OnTopic -= PluginsOnTopic;
            bot.OnTopicChange -= PluginsOnTopicChange;
            bot.OnUnAway -= PluginsOnUnAway;
            bot.OnUnban -= PluginsOnUnban;
            bot.OnUserModeChange -= PluginsOnUserModeChange;
            bot.OnVoice -= PluginsOnVoice;
            bot.OnWho -= PluginsOnWho;
            bot.OnWriteLine -= PluginsOnWriteLine;
            bot.SupportNonRfcChanged -= PluginsSupportNonRfcChanged;
        }

        public event AdminEventHandler OnAdmin;
        public event AutoConnectErrorEventHandler OnAutoConnectError;
        public event AwayEventHandler OnAway;
        public event BanEventHandler OnBan;
        public event ActionEventHandler OnChannelAction;
        public event IrcEventHandler OnChannelActiveSynced;

        public event IrcEventHandler OnChannelMessage;
        public event IrcEventHandler OnChannelModeChange;
        public event IrcEventHandler OnChannelNotice;
        public event IrcEventHandler OnChannelPassiveSynced;
        public event EventHandler OnConnected;
        public event EventHandler OnConnecting;
        public event EventHandler OnConnectionError;
        public event CtcpEventHandler OnCtcpReply;
        public event CtcpEventHandler OnCtcpRequest;
        public event DccChatLineHandler OnDccChatReceiveLineEvent;
        public event DccConnectionHandler OnDccChatRequestEvent;
        public event DccChatLineHandler OnDccChatSentLineEvent;
        public event DccConnectionHandler OnDccChatStartEvent;
        public event DccConnectionHandler OnDccChatStopEvent;
        public event DccSendPacketHandler OnDccSendReceiveBlockEvent;
        public event DccSendRequestHandler OnDccSendRequestEvent;
        public event DccSendPacketHandler OnDccSendSentBlockEvent;
        public event DccConnectionHandler OnDccSendStartEvent;
        public event DccConnectionHandler OnDccSendStopEvent;
        public event DeadminEventHandler OnDeadmin;
        public event DehalfopEventHandler OnDehalfop;
        public event DeopEventHandler OnDeop;
        public event DeownerEventHandler OnDeowner;
        public event DevoiceEventHandler OnDevoice;
        public event EventHandler OnDisconnected;
        public event EventHandler OnDisconnecting;
        public event ErrorEventHandler OnError;
        public event IrcEventHandler OnErrorMessage;
        public event HalfopEventHandler OnHalfop;
        public event InviteEventHandler OnInvite;
        public event JoinEventHandler OnJoin;
        public event KickEventHandler OnKick;
        public event ListEventHandler OnList;
        public event IrcEventHandler OnModeChange;
        public event MotdEventHandler OnMotd;
        public event NamesEventHandler OnNames;
        public event NickChangeEventHandler OnNickChange;
        public event IrcEventHandler OnNowAway;
        public event OpEventHandler OnOp;
        public event OwnerEventHandler OnOwner;
        public event PartEventHandler OnPart;
        public event PingEventHandler OnPing;
        public event PongEventHandler OnPong;
        public event IrcEventHandler OnRawMessage;
        public event ReadLineEventHandler OnReadLine;
        public event EventHandler OnRegistered;
        public event ActionEventHandler OnQueryAction;
        public event IrcEventHandler OnQueryMessage;
        public event IrcEventHandler OnQueryNotice;
        public event QuitEventHandler OnQuit;
        public event TopicEventHandler OnTopic;
        public event TopicChangeEventHandler OnTopicChange;
        public event UnbanEventHandler OnUnban;
        public event IrcEventHandler OnUnAway;
        public event IrcEventHandler OnUserModeChange;
        public event VoiceEventHandler OnVoice;
        public event WhoEventHandler OnWho;
        public event WriteLineEventHandler OnWriteLine;
        public event EventHandler SupportNonRfcChanged;

        public void PluginsOnAdmin(object sender, AdminEventArgs e)
        {
            OnAdmin(this, e);
        }

        public void PluginsOnAutoConnectError(object sender, AutoConnectErrorEventArgs e)
        {
            OnAutoConnectError(this, e);
        }

        public void PluginsOnAway(object sender, AwayEventArgs e)
        {
            OnAway(this, e);
        }

        public void PluginsOnBan(object sender, BanEventArgs e)
        {
            OnBan(this, e);
        }

        public void PluginsOnChannelAction(object sender, ActionEventArgs e)
        {
            OnChannelAction(this, e);
        }

        public void PluginsOnChannelActiveSynced(object sender, IrcEventArgs e)
        {
            OnChannelActiveSynced(this, e);
        }

        public void PluginsOnChannelMessage(object sender, IrcEventArgs e)
        {
            OnChannelMessage(this, e);
        }

        public void PluginsOnChannelModeChange(object sender, IrcEventArgs e)
        {
            OnChannelModeChange(this, e);
        }

        public void PluginsOnChannelNotice(object sender, IrcEventArgs e)
        {
            OnChannelNotice(this, e);
        }

        public void PluginsOnChannelPassiveSynced(object sender, IrcEventArgs e)
        {
            OnChannelPassiveSynced(this, e);
        }

        public void PluginsOnConnected(object sender, EventArgs e)
        {
            OnConnected(this, e);
        }

        public void PluginsOnConnecting(object sender, EventArgs e)
        {
            OnConnecting(this, e);
        }

        public void PluginsOnConnectionError(object sender, EventArgs e)
        {
            OnConnectionError(this, e);
        }

        public void PluginsOnCtcpReply(object sender, CtcpEventArgs e)
        {
            OnCtcpReply(this, e);
        }

        public void PluginsOnCtcpRequest(object sender, CtcpEventArgs e)
        {
            OnCtcpRequest(this, e);
        }

        public void PluginsOnDccChatReceiveLineEvent(object sender, DccChatEventArgs e)
        {
            OnDccChatReceiveLineEvent(this, e);
        }

        public void PluginsOnDccChatRequestEvent(object sender, DccEventArgs e)
        {
            OnDccChatRequestEvent(this, e);
        }

        public void PluginsOnDccChatSentLineEvent(object sender, DccChatEventArgs e)
        {
            OnDccChatSentLineEvent(this, e);
        }

        public void PluginsOnDccChatStartEvent(object sender, DccEventArgs e)
        {
            OnDccChatStartEvent(this, e);
        }

        public void PluginsOnDccChatStopEvent(object sender, DccEventArgs e)
        {
            OnDccChatStopEvent(this, e);
        }

        public void PluginsOnDccSendReceiveBlockEvent(object sender, DccSendEventArgs e)
        {
            OnDccSendReceiveBlockEvent(this, e);
        }

        public void PluginsOnDccSendRequestEvent(object sender, DccSendRequestEventArgs e)
        {
            OnDccSendRequestEvent(this, e);
        }

        public void PluginsOnDccSendSentBlockEvent(object sender, DccSendEventArgs e)
        {
            OnDccSendSentBlockEvent(this, e);
        }

        public void PluginsOnDccSendStartEvent(object sender, DccEventArgs e)
        {
            OnDccSendStartEvent(this, e);
        }

        public void PluginsOnDccSendStopEvent(object sender, DccEventArgs e)
        {
            OnDccSendStopEvent(this, e);
        }

        public void PluginsOnDeadmin(object sender, DeadminEventArgs e)
        {
            OnDeadmin(this, e);
        }

        public void PluginsOnDehalfop(object sender, DehalfopEventArgs e)
        {
            OnDehalfop(this, e);
        }

        public void PluginsOnDeop(object sender, DeopEventArgs e)
        {
            OnDeop(this, e);
        }

        public void PluginsOnDeowner(object sender, DeownerEventArgs e)
        {
            OnDeowner(this, e);
        }

        public void PluginsOnDevoice(object sender, DevoiceEventArgs e)
        {
            OnDevoice(this, e);
        }

        public void PluginsOnDisconnected(object sender, EventArgs e)
        {
            OnDisconnected(this, e);
        }

        public void PluginsOnDisconnecting(object sender, EventArgs e)
        {
            OnDisconnecting(this, e);
        }

        public void PluginsOnError(object sender, ErrorEventArgs e)
        {
            OnError(this, e);
        }

        public void PluginsOnErrorMessage(object sender, IrcEventArgs e)
        {
            OnErrorMessage(this, e);
        }

        public void PluginsOnHalfop(object sender, HalfopEventArgs e)
        {
            OnHalfop(this, e);
        }

        public void PluginsOnInvite(object sender, InviteEventArgs e)
        {
            OnInvite(this, e);
        }

        public void PluginsOnJoin(object sender, JoinEventArgs e)
        {
            OnJoin(this, e);
        }

        public void PluginsOnKick(object sender, KickEventArgs e)
        {
            OnKick(this, e);
        }

        public void PluginsOnList(object sender, ListEventArgs e)
        {
            OnList(this, e);
        }

        public void PluginsOnModeChange(object sender, IrcEventArgs e)
        {
            OnModeChange(this, e);
        }

        public void PluginsOnMotd(object sender, MotdEventArgs e)
        {
            OnMotd(this, e);
        }

        public void PluginsOnNames(object sender, NamesEventArgs e)
        {
            OnNames(this, e);
        }

        public void PluginsOnNickChange(object sender, NickChangeEventArgs e)
        {
            OnNickChange(this, e);
        }

        public void PluginsOnNowAway(object sender, IrcEventArgs e)
        {
            OnNowAway(this, e);
        }

        public void PluginsOnOp(object sender, OpEventArgs e)
        {
            OnOp(this, e);
        }

        public void PluginsOnOwner(object sender, OwnerEventArgs e)
        {
            OnOwner(this, e);
        }

        public void PluginsOnPart(object sender, PartEventArgs e)
        {
            OnPart(this, e);
        }

        public void PluginsOnPing(object sender, PingEventArgs e)
        {
            OnPing(this, e);
        }

        public void PluginsOnPong(object sender, PongEventArgs e)
        {
            OnPong(this, e);
        }

        public void PluginsOnQueryAction(object sender, ActionEventArgs e)
        {
            OnQueryAction(this, e);
        }

        public void PluginsOnQueryMessage(object sender, IrcEventArgs e)
        {
            OnQueryMessage(this, e);
        }

        public void PluginsOnQueryNotice(object sender, IrcEventArgs e)
        {
            OnQueryNotice(this, e);
        }

        public void PluginsOnQuit(object sender, QuitEventArgs e)
        {
            OnQuit(this, e);
        }

        public void PluginsOnRawMessage(object sender, IrcEventArgs e)
        {
            OnRawMessage(this, e);
        }

        public void PluginsOnReadLine(object sender, ReadLineEventArgs e)
        {
            OnReadLine(this, e);
        }

        public void PluginsOnRegistered(object sender, EventArgs e)
        {
            OnRegistered(this, e);
        }

        public void PluginsOnTopic(object sender, TopicEventArgs e)
        {
            OnTopic(this, e);
        }

        public void PluginsOnTopicChange(object sender, TopicChangeEventArgs e)
        {
            OnTopicChange(this, e);
        }

        public void PluginsOnUnAway(object sender, IrcEventArgs e)
        {
            OnUnAway(this, e);
        }

        public void PluginsOnUnban(object sender, UnbanEventArgs e)
        {
            OnUnban(this, e);
        }

        public void PluginsOnUserModeChange(object sender, IrcEventArgs e)
        {
            OnUserModeChange(this, e);
        }

        public void PluginsOnVoice(object sender, VoiceEventArgs e)
        {
            OnVoice(this, e);
        }

        public void PluginsOnWho(object sender, WhoEventArgs e)
        {
            OnWho(this, e);
        }

        public void PluginsOnWriteLine(object sender, WriteLineEventArgs e)
        {
            OnWriteLine(this, e);
        }

        public void PluginsSupportNonRfcChanged(object sender, EventArgs e)
        {
            SupportNonRfcChanged(this, e);
        }
    }
}