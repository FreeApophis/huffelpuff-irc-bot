/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 03.01.2009
 * Zeit: 12:38
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Runtime.Remoting;

using Meebey.SmartIrc4net;

namespace Huffelpuff.ComplexPlugins
{
	/// <summary>
	/// Description of SharedClientSide.
	/// </summary>
	public class SharedClientSide : MarshalByRefObject  
	{
        private SharedServerSide serverSide;
  
        public SharedClientSide(IrcBot botInstance)  
        {  
            this.serverSide = new SharedServerSide(botInstance);  
            this.serverSide.OnAdmin += new AdminEventHandler(HandleOnAdmin);
			this.serverSide.OnAutoConnectError += new AutoConnectErrorEventHandler(HandleOnAutoConnectError);
			this.serverSide.OnAway += new AwayEventHandler(HandleOnAway);
			this.serverSide.OnBan += new BanEventHandler(HandleOnBan);
			this.serverSide.OnChannelAction += new ActionEventHandler(HandleOnChannelAction);
			this.serverSide.OnChannelActiveSynced += new IrcEventHandler(HandleOnChannelActiveSynced);
			this.serverSide.OnChannelMessage += new IrcEventHandler(HandleOnChannelMessage);
			this.serverSide.OnChannelModeChange += new IrcEventHandler(HandleOnChannelModeChange);
			this.serverSide.OnChannelNotice += new IrcEventHandler(HandleOnChannelNotice);
			this.serverSide.OnChannelPassiveSynced += new IrcEventHandler(HandleOnChannelPassiveSynced);
			this.serverSide.OnConnected += new EventHandler(HandleOnConnected);
			this.serverSide.OnConnecting += new EventHandler(HandleOnConnecting);
			this.serverSide.OnConnectionError += new EventHandler(HandleOnConnectionError);
			this.serverSide.OnCtcpReply += new CtcpEventHandler(HandleOnCtcpReply);
			this.serverSide.OnCtcpRequest += new CtcpEventHandler(HandleOnCtcpRequest);
			this.serverSide.OnDccChatReceiveLineEvent += new DccChatLineHandler(HandleOnDccChatReceiveLineEvent);
			this.serverSide.OnDccChatRequestEvent += new DccConnectionHandler(HandleOnDccChatRequestEvent);
			this.serverSide.OnDccChatSentLineEvent += new DccChatLineHandler(HandleOnDccChatSentLineEvent);
			this.serverSide.OnDccChatStartEvent += new DccConnectionHandler(HandleOnDccChatStartEvent);
			this.serverSide.OnDccChatStopEvent += new DccConnectionHandler(HandleOnDccChatStopEvent);
			this.serverSide.OnDccSendReceiveBlockEvent += new DccSendPacketHandler(HandleOnDccSendReceiveBlockEvent);
			this.serverSide.OnDccSendRequestEvent += new DccSendRequestHandler(HandleOnDccSendRequestEvent);
			this.serverSide.OnDccSendSentBlockEvent += new DccSendPacketHandler(HandleOnDccSendSentBlockEvent);
			this.serverSide.OnDccSendStartEvent += new DccConnectionHandler(HandleOnDccSendStartEvent);
			this.serverSide.OnDccSendStopEvent += new DccConnectionHandler(HandleOnDccSendStopEvent);
			this.serverSide.OnDeadmin += new DeadminEventHandler(HandleOnDeadmin);
			this.serverSide.OnDehalfop += new DehalfopEventHandler(HandleOnDehalfop);
			this.serverSide.OnDeop += new DeopEventHandler(HandleOnDeop);
			this.serverSide.OnDeowner += new DeownerEventHandler(HandleOnDeowner);
			this.serverSide.OnDevoice += new DevoiceEventHandler(HandleOnDevoice);
			this.serverSide.OnDisconnected += new EventHandler(HandleOnDisconnected);
			this.serverSide.OnDisconnecting += new EventHandler(HandleOnDisconnecting);
			this.serverSide.OnError += new ErrorEventHandler(HandleOnError);
			this.serverSide.OnErrorMessage += new IrcEventHandler(HandleOnErrorMessage);
			this.serverSide.OnHalfop += new HalfopEventHandler(HandleOnHalfop);
			this.serverSide.OnInvite += new InviteEventHandler(HandleOnInvite);
			this.serverSide.OnJoin += new JoinEventHandler(HandleOnJoin);
			this.serverSide.OnKick += new KickEventHandler(HandleOnKick);
			this.serverSide.OnList += new ListEventHandler(HandleOnList);
			this.serverSide.OnModeChange += new IrcEventHandler(HandleOnModeChange);
			this.serverSide.OnMotd += new MotdEventHandler(HandleOnMotd);
			this.serverSide.OnNames += new NamesEventHandler(HandleOnNames);
			this.serverSide.OnNickChange += new NickChangeEventHandler(HandleOnNickChange);
			this.serverSide.OnNowAway += new IrcEventHandler(HandleOnNowAway);
			this.serverSide.OnOp += new OpEventHandler(HandleOnOp);
			this.serverSide.OnOwner += new OwnerEventHandler(HandleOnOwner);
			this.serverSide.OnPart += new PartEventHandler(HandleOnPart);
			this.serverSide.OnPing += new PingEventHandler(HandleOnPing);
			this.serverSide.OnPong += new PongEventHandler(HandleOnPong);
			this.serverSide.OnQueryAction += new ActionEventHandler(HandleOnQueryAction);
			this.serverSide.OnQueryMessage += new IrcEventHandler(HandleOnQueryMessage);
			this.serverSide.OnQueryNotice += new IrcEventHandler(HandleOnQueryNotice);
			this.serverSide.OnQuit += new QuitEventHandler(HandleOnQuit);
			this.serverSide.OnRawMessage += new IrcEventHandler(HandleOnRawMessage);
			this.serverSide.OnReadLine += new ReadLineEventHandler(HandleOnReadLine);
			this.serverSide.OnRegistered += new EventHandler(HandleOnRegistered);
			this.serverSide.OnTopic += new TopicEventHandler(HandleOnTopic);
			this.serverSide.OnTopicChange += new TopicChangeEventHandler(HandleOnTopicChange);
			this.serverSide.OnUnAway += new IrcEventHandler(HandleOnUnAway);
			this.serverSide.OnUnban += new UnbanEventHandler(HandleOnUnban);
			this.serverSide.OnUserModeChange += new IrcEventHandler(HandleOnUserModeChange);
			this.serverSide.OnVoice += new VoiceEventHandler(HandleOnVoice);
			this.serverSide.OnWho += new WhoEventHandler(HandleOnWho);
			this.serverSide.OnWriteLine += new WriteLineEventHandler(HandleOnWriteLine);
			this.serverSide.SupportNonRfcChanged += new EventHandler(HandleSupportNonRfcChanged);
        }
  
        public event AdminEventHandler          	OnAdmin;
        public event AutoConnectErrorEventHandler   OnAutoConnectError;
        public event AwayEventHandler           	OnAway;
        public event BanEventHandler            	OnBan;
        public event ActionEventHandler         	OnChannelAction;
        public event IrcEventHandler            	OnChannelActiveSynced;
        public event IrcEventHandler				OnChannelMessage;
        public event IrcEventHandler            	OnChannelModeChange;
        public event IrcEventHandler            	OnChannelNotice;
        public event IrcEventHandler            	OnChannelPassiveSynced;
        public event EventHandler           		OnConnected;
        public event EventHandler           		OnConnecting;
        public event EventHandler           		OnConnectionError;
        public event CtcpEventHandler           	OnCtcpReply;
        public event CtcpEventHandler           	OnCtcpRequest;
		public event DccChatLineHandler 			OnDccChatReceiveLineEvent;
        public event DccConnectionHandler 			OnDccChatRequestEvent;
		public event DccChatLineHandler 			OnDccChatSentLineEvent;
		public event DccConnectionHandler 			OnDccChatStartEvent;
		public event DccConnectionHandler 			OnDccChatStopEvent;
		public event DccSendPacketHandler 			OnDccSendReceiveBlockEvent;
		public event DccSendRequestHandler 			OnDccSendRequestEvent;
		public event DccSendPacketHandler 			OnDccSendSentBlockEvent;
		public event DccConnectionHandler 			OnDccSendStartEvent;
		public event DccConnectionHandler 			OnDccSendStopEvent;
        public event DeadminEventHandler        	OnDeadmin;
        public event DehalfopEventHandler       	OnDehalfop;
        public event DeopEventHandler           	OnDeop;
        public event DeownerEventHandler        	OnDeowner;
        public event DevoiceEventHandler        	OnDevoice;
        public event EventHandler           		OnDisconnected;
        public event EventHandler           		OnDisconnecting;
        public event ErrorEventHandler          	OnError;
        public event IrcEventHandler            	OnErrorMessage;
        public event HalfopEventHandler         	OnHalfop;
        public event InviteEventHandler         	OnInvite;
        public event JoinEventHandler           	OnJoin;        	
        public event KickEventHandler           	OnKick;
        public event ListEventHandler           	OnList;
        public event IrcEventHandler            	OnModeChange;
        public event MotdEventHandler           	OnMotd;
        public event NamesEventHandler          	OnNames;
        public event NickChangeEventHandler     	OnNickChange;
        public event IrcEventHandler            	OnNowAway;
        public event OpEventHandler             	OnOp;
        public event OwnerEventHandler          	OnOwner;
        public event PartEventHandler           	OnPart;
        public event PingEventHandler           	OnPing;
        public event PongEventHandler           	OnPong;
        public event IrcEventHandler            	OnRawMessage;
        public event ReadLineEventHandler   		OnReadLine;
		public event EventHandler               	OnRegistered;
        public event ActionEventHandler         	OnQueryAction;
        public event IrcEventHandler            	OnQueryMessage;
        public event IrcEventHandler            	OnQueryNotice;
        public event QuitEventHandler           	OnQuit;
        public event TopicEventHandler          	OnTopic;
        public event TopicChangeEventHandler    	OnTopicChange;
        public event UnbanEventHandler          	OnUnban;
        public event IrcEventHandler            	OnUnAway;
        public event IrcEventHandler            	OnUserModeChange;
        public event VoiceEventHandler          	OnVoice;
        public event WhoEventHandler            	OnWho;
        public event WriteLineEventHandler  		OnWriteLine;
        public event EventHandler 					SupportNonRfcChanged;
  
        internal void HandleOnAdmin(object sender, AdminEventArgs e)  
        {  
        	if (this.OnAdmin != null)
        		this.OnAdmin(this, e);
        } 

        internal void HandleOnAutoConnectError(object sender, AutoConnectErrorEventArgs e)  
        {  
        	if (this.OnAutoConnectError != null)
        		this.OnAutoConnectError(this, e);
        } 

        internal void HandleOnAway(object sender, AwayEventArgs e)  
        {  
        	if (this.OnAway != null)
        		this.OnAway(this, e);
        } 
        internal void HandleOnBan(object sender, BanEventArgs e)  
        {  
        	if (this.OnBan != null)
        		this.OnBan(this, e);
        } 

        internal void HandleOnChannelAction(object sender, ActionEventArgs e)  
        {  
        	if (this.OnChannelAction != null)
        		this.OnChannelAction(this, e);
        } 
        
        internal void HandleOnChannelActiveSynced(object sender, IrcEventArgs e)  
        {  
        	if (this.OnChannelActiveSynced != null)
        		this.OnChannelActiveSynced(this, e);
        } 

        internal void HandleOnChannelMessage(object sender, IrcEventArgs e)  
        {  
        	if (this.OnChannelMessage != null)
        		this.OnChannelMessage(this, e);
        } 

        internal void HandleOnChannelModeChange(object sender, IrcEventArgs e)  
        {  
        	if (this.OnChannelModeChange != null)
        		this.OnChannelModeChange(this, e);
        } 
        
        internal void HandleOnChannelNotice(object sender, IrcEventArgs e)  
        {  
        	if (this.OnChannelNotice != null)
        		this.OnChannelNotice(this, e);
        } 

        internal void HandleOnChannelPassiveSynced(object sender, IrcEventArgs e)  
        {  
        	if (this.OnChannelPassiveSynced != null)
        		this.OnChannelPassiveSynced(this, e);
        } 

        internal void HandleOnConnected(object sender, EventArgs e)  
        {  
        	if (this.OnConnected != null)
        		this.OnConnected(this, e);
        } 

        internal void HandleOnConnecting(object sender, EventArgs e)  
        {  
        	if (this.OnConnecting != null)
        		this.OnConnecting(this, e);
        } 

        internal void HandleOnConnectionError(object sender, EventArgs e)  
        {  
        	if (this.OnConnectionError != null)
        		this.OnConnectionError(this, e);
        } 

        internal void HandleOnCtcpReply(object sender, CtcpEventArgs e)  
        {  
        	if (this.OnCtcpReply != null)
        		this.OnCtcpReply(this, e);
        } 
       
        internal void HandleOnCtcpRequest(object sender, CtcpEventArgs e)  
        {  
        	if (this.OnCtcpRequest != null)
        		this.OnCtcpRequest(this, e);
        } 

        internal void HandleOnDccChatReceiveLineEvent(object sender, DccChatEventArgs e)  
        {  
        	if (this.OnDccChatReceiveLineEvent != null)
        		this.OnDccChatReceiveLineEvent(this, e);
        } 

        internal void HandleOnDccChatRequestEvent(object sender, DccEventArgs e)  
        {  
        	if (this.OnDccChatRequestEvent != null)
        		this.OnDccChatRequestEvent(this, e);
        } 

        internal void HandleOnDccChatSentLineEvent(object sender, DccChatEventArgs e)  
        {  
        	if (this.OnDccChatSentLineEvent != null)
        		this.OnDccChatSentLineEvent(this, e);
        } 

        internal void HandleOnDccChatStartEvent(object sender, DccEventArgs e)  
        {  
        	if (this.OnDccChatStartEvent != null)
        		this.OnDccChatStartEvent(this, e);
        } 
        
        internal void HandleOnDccChatStopEvent(object sender, DccEventArgs e)  
        {  
        	if (this.OnDccChatStopEvent != null)
        		this.OnDccChatStopEvent(this, e);
        } 

        internal void HandleOnDccSendReceiveBlockEvent(object sender, DccSendEventArgs e)  
        {  
        	if (this.OnDccSendReceiveBlockEvent != null)
        		this.OnDccSendReceiveBlockEvent(this, e);
        } 

        internal void HandleOnDccSendRequestEvent(object sender, DccSendRequestEventArgs e)  
        {  
        	if (this.OnDccSendRequestEvent != null)
        		this.OnDccSendRequestEvent(this, e);
        } 

        internal void HandleOnDccSendSentBlockEvent(object sender, DccSendEventArgs e)  
        {  
        	if (this.OnDccSendSentBlockEvent != null)
        		this.OnDccSendSentBlockEvent(this, e);
        } 

        internal void HandleOnDccSendStartEvent(object sender, DccEventArgs e)  
        {  
        	if (this.OnDccSendStartEvent != null)
        		this.OnDccSendStartEvent(this, e);
        } 

        internal void HandleOnDccSendStopEvent(object sender, DccEventArgs e)  
        {  
        	if (this.OnDccSendStopEvent != null)
        		this.OnDccSendStopEvent(this, e);
        } 

        internal void HandleOnDeadmin (object sender, DeadminEventArgs e)  
        {  
        	if (this.OnDeadmin != null)
        		this.OnDeadmin(this, e);
        } 

        internal void HandleOnDehalfop(object sender, DehalfopEventArgs e)  
        {  
        	if (this.OnDehalfop != null)
        		this.OnDehalfop(this, e);
        } 

        internal void HandleOnDeop(object sender, DeopEventArgs e)  
        {  
        	if (this.OnDeop != null)
        		this.OnDeop(this, e);
        } 

        internal void HandleOnDeowner(object sender, DeownerEventArgs e)  
        {  
        	if (this.OnDeowner != null)
        		this.OnDeowner(this, e);
        } 

        internal void HandleOnDevoice(object sender, DevoiceEventArgs e)
        {
        	if (this.OnDevoice != null)
        		this.OnDevoice(this, e);
        } 

        internal void HandleOnDisconnected(object sender, EventArgs e)  
        {  
        	if (this.OnDisconnected != null)
        		this.OnDisconnected(this, e);
        } 

        internal void HandleOnDisconnecting(object sender, EventArgs e)  
        {  
        	if (this.OnDisconnecting != null)
        		this.OnDisconnecting(this, e);
        } 

       internal void HandleOnError(object sender, ErrorEventArgs e)  
        {  
        	if (this.OnError != null)
        		this.OnError(this, e);
        } 

        internal void HandleOnErrorMessage(object sender, IrcEventArgs e)  
        {  
        	if (this.OnErrorMessage != null)
        		this.OnErrorMessage(this, e);
        } 

        internal void HandleOnHalfop(object sender, HalfopEventArgs e)  
        {  
        	if (this.OnHalfop != null)
        		this.OnHalfop(this, e);
        }        

        internal void HandleOnInvite(object sender, InviteEventArgs e)  
        {  
        	if (this.OnInvite != null)
        		this.OnInvite(this, e);
        } 

        internal void HandleOnJoin(object sender, JoinEventArgs e)  
        {  
        	if (this.OnJoin != null)
        		this.OnJoin(this, e);
        } 

        internal void HandleOnKick(object sender, KickEventArgs e)  
        {  
        	if (this.OnKick != null)
        		this.OnKick(this, e);
        } 
        
        internal void HandleOnList(object sender, ListEventArgs e)  
        {  
        	if (this.OnList != null)
        		this.OnList(this, e);
        } 

        internal void HandleOnModeChange(object sender, IrcEventArgs e)  
        {  
        	if (this.OnModeChange != null)
        		this.OnModeChange(this, e);
        } 

        internal void HandleOnMotd(object sender, MotdEventArgs e)  
        {  
        	if (this.OnMotd != null)
        		this.OnMotd(this, e);
        } 

        internal void HandleOnNames(object sender, NamesEventArgs e)  
        {  
        	if (this.OnNames != null)
        		this.OnNames(this, e);
        } 

        internal void HandleOnNickChange(object sender, NickChangeEventArgs e)  
        {  
        	if (this.OnNickChange != null)
        		this.OnNickChange(this, e);
        } 

        internal void HandleOnNowAway(object sender,  IrcEventArgs e)  
        {  
        	if (this.OnNowAway != null)
        		this.OnNowAway(this, e);
        } 

        internal void HandleOnOp(object sender, OpEventArgs e)  
        {  
        	if (this.OnOp != null)
        		this.OnOp(this, e);
        } 

        internal void HandleOnOwner(object sender, OwnerEventArgs e)  
        {  
        	if (this.OnOwner != null)
        		this.OnOwner(this, e);
        } 

        internal void HandleOnPart(object sender, PartEventArgs e)  
        {  
        	if (this.OnPart != null)
        		this.OnPart(this, e);
        } 
        
        internal void HandleOnPing(object sender, PingEventArgs e)  
        {  
        	if (this.OnPing != null)
        		this.OnPing(this, e);
        } 
        
        internal void HandleOnPong(object sender, PongEventArgs e)  
        {  
        	if (this.OnPong != null)
        		this.OnPong(this, e);
        } 

        internal void HandleOnQueryAction(object sender, ActionEventArgs e)  
        {  
        	if (this.OnQueryAction != null)
        		this.OnQueryAction(this, e);
        }     

        internal void HandleOnQueryMessage(object sender, IrcEventArgs e)  
        {  
        	if (this.OnQueryMessage != null)
        		this.OnQueryMessage(this, e);
        } 

        internal void HandleOnQueryNotice(object sender, IrcEventArgs e)  
        {  
        	if (this.OnQueryNotice != null)
        		this.OnQueryNotice(this, e);
        } 

        internal void HandleOnQuit(object sender, QuitEventArgs e)  
        {  
        	if (this.OnQuit != null)
        		this.OnQuit(this, e);
        } 

        internal void HandleOnRawMessage(object sender, IrcEventArgs e)
        {  
        	if (this.OnRawMessage != null)
        		this.OnRawMessage(this, e);
        } 

        internal void HandleOnReadLine(object sender, ReadLineEventArgs e)  
        {  
        	if (this.OnReadLine != null)
        		this.OnReadLine(this, e);
        } 

        internal void HandleOnRegistered(object sender, EventArgs e)  
        {  
        	if (this.OnRegistered != null)
        		this.OnRegistered(this, e);
        } 
       
        internal void HandleOnTopic(object sender, TopicEventArgs e)  
        {  
        	if (this.OnTopic != null)
        		this.OnTopic(this, e);
        } 

        internal void HandleOnTopicChange(object sender, TopicChangeEventArgs e)  
        {  
        	if (this.OnTopicChange != null)
        		this.OnTopicChange(this, e);
        } 

        internal void HandleOnUnAway(object sender, IrcEventArgs e)  
        {  
        	if (this.OnUnAway != null)
        		this.OnUnAway(this, e);
        } 
        internal void HandleOnUnban(object sender, UnbanEventArgs e)  
        {  
        	if (this.OnUnban != null)
        		this.OnUnban(this, e);
        } 

        internal void HandleOnUserModeChange(object sender, IrcEventArgs e)  
        {  
        	if (this.OnUserModeChange != null)
        		this.OnUserModeChange(this, e);
        } 

        internal void HandleOnVoice(object sender, VoiceEventArgs e)  
        {  
        	if (this.OnVoice != null)
        		this.OnVoice(this, e);
        }   

        internal void HandleOnWho(object sender, WhoEventArgs e)  
        {  
        	if (this.OnWho != null)
        		this.OnWho(this, e);
        } 

        internal void HandleOnWriteLine(object sender, WriteLineEventArgs e)  
        {  
        	if (this.OnWriteLine != null)
        		this.OnWriteLine(this, e);
        } 

        internal void HandleSupportNonRfcChanged(object sender, EventArgs e)  
        {  
        	if (this.SupportNonRfcChanged != null)
        		this.SupportNonRfcChanged(this, e);
        }    
	}
}
