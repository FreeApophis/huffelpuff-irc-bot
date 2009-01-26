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
    /// Description of SharedServerSide.
    /// </summary>
    public class SharedServerSide : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }       
        
        private IrcBot bot;
        public SharedServerSide(IrcBot botInstance)
        {
            this.bot = botInstance;
            
            botInstance.OnAdmin += new AdminEventHandler(PluginsOnAdmin);
            botInstance.OnAutoConnectError += new AutoConnectErrorEventHandler(PluginsOnAutoConnectError);
            botInstance.OnAway += new AwayEventHandler(PluginsOnAway);
            botInstance.OnBan += new BanEventHandler(PluginsOnBan);
            botInstance.OnChannelAction += new ActionEventHandler(PluginsOnChannelAction);
            botInstance.OnChannelActiveSynced += new IrcEventHandler(PluginsOnChannelActiveSynced);
            botInstance.OnChannelMessage += new IrcEventHandler(PluginsOnChannelMessage);
            botInstance.OnChannelModeChange += new IrcEventHandler(PluginsOnChannelModeChange);
            botInstance.OnChannelNotice += new IrcEventHandler(PluginsOnChannelNotice);
            botInstance.OnChannelPassiveSynced += new IrcEventHandler(PluginsOnChannelPassiveSynced);
            botInstance.OnConnected += new EventHandler(PluginsOnConnected);
            botInstance.OnConnecting += new EventHandler(PluginsOnConnecting);
            botInstance.OnConnectionError += new EventHandler(PluginsOnConnectionError);
            botInstance.OnCtcpReply += new CtcpEventHandler(PluginsOnCtcpReply);
            botInstance.OnCtcpRequest += new CtcpEventHandler(PluginsOnCtcpRequest);
            botInstance.OnDccChatReceiveLineEvent += new DccChatLineHandler(PluginsOnDccChatReceiveLineEvent);
            botInstance.OnDccChatRequestEvent += new DccConnectionHandler(PluginsOnDccChatRequestEvent);
            botInstance.OnDccChatSentLineEvent += new DccChatLineHandler(PluginsOnDccChatSentLineEvent);
            botInstance.OnDccChatStartEvent += new DccConnectionHandler(PluginsOnDccChatStartEvent);
            botInstance.OnDccChatStopEvent += new DccConnectionHandler(PluginsOnDccChatStopEvent);
            botInstance.OnDccSendReceiveBlockEvent += new DccSendPacketHandler(PluginsOnDccSendReceiveBlockEvent);
            botInstance.OnDccSendRequestEvent += new DccSendRequestHandler(PluginsOnDccSendRequestEvent);
            botInstance.OnDccSendSentBlockEvent += new DccSendPacketHandler(PluginsOnDccSendSentBlockEvent);
            botInstance.OnDccSendStartEvent += new DccConnectionHandler(PluginsOnDccSendStartEvent);
            botInstance.OnDccSendStopEvent += new DccConnectionHandler(PluginsOnDccSendStopEvent);
            botInstance.OnDeadmin += new DeadminEventHandler(PluginsOnDeadmin);
            botInstance.OnDehalfop += new DehalfopEventHandler(PluginsOnDehalfop);
            botInstance.OnDeop += new DeopEventHandler(PluginsOnDeop);
            botInstance.OnDeowner += new DeownerEventHandler(PluginsOnDeowner);
            botInstance.OnDevoice += new DevoiceEventHandler(PluginsOnDevoice);
            botInstance.OnDisconnected += new EventHandler(PluginsOnDisconnected);
            botInstance.OnDisconnecting += new EventHandler(PluginsOnDisconnecting);
            botInstance.OnError += new ErrorEventHandler(PluginsOnError);
            botInstance.OnErrorMessage += new IrcEventHandler(PluginsOnErrorMessage);
            botInstance.OnHalfop += new HalfopEventHandler(PluginsOnHalfop);
            botInstance.OnInvite += new InviteEventHandler(PluginsOnInvite);
            botInstance.OnJoin += new JoinEventHandler(PluginsOnJoin);
            botInstance.OnKick += new KickEventHandler(PluginsOnKick);
            botInstance.OnList += new ListEventHandler(PluginsOnList);
            botInstance.OnModeChange += new IrcEventHandler(PluginsOnModeChange);
            botInstance.OnMotd += new MotdEventHandler(PluginsOnMotd);
            botInstance.OnNames += new NamesEventHandler(PluginsOnNames);
            botInstance.OnNickChange += new NickChangeEventHandler(PluginsOnNickChange);
            botInstance.OnNowAway += new IrcEventHandler(PluginsOnNowAway);
            botInstance.OnOp += new OpEventHandler(PluginsOnOp);
            botInstance.OnOwner += new OwnerEventHandler(PluginsOnOwner);
            botInstance.OnPart += new PartEventHandler(PluginsOnPart);
            botInstance.OnPing += new PingEventHandler(PluginsOnPing);
            botInstance.OnPong += new PongEventHandler(PluginsOnPong);
            botInstance.OnQueryAction += new ActionEventHandler(PluginsOnQueryAction);
            botInstance.OnQueryMessage += new IrcEventHandler(PluginsOnQueryMessage);
            botInstance.OnQueryNotice += new IrcEventHandler(PluginsOnQueryNotice);
            botInstance.OnQuit += new QuitEventHandler(PluginsOnQuit);
            botInstance.OnRawMessage += new IrcEventHandler(PluginsOnRawMessage);
            botInstance.OnReadLine += new ReadLineEventHandler(PluginsOnReadLine);
            botInstance.OnRegistered += new EventHandler(PluginsOnRegistered);
            botInstance.OnTopic += new TopicEventHandler(PluginsOnTopic);
            botInstance.OnTopicChange += new TopicChangeEventHandler(PluginsOnTopicChange);
            botInstance.OnUnAway += new IrcEventHandler(PluginsOnUnAway);
            botInstance.OnUnban += new UnbanEventHandler(PluginsOnUnban);
            botInstance.OnUserModeChange += new IrcEventHandler(PluginsOnUserModeChange);
            botInstance.OnVoice += new VoiceEventHandler(PluginsOnVoice);
            botInstance.OnWho += new WhoEventHandler(PluginsOnWho);
            botInstance.OnWriteLine += new WriteLineEventHandler(PluginsOnWriteLine);
            botInstance.SupportNonRfcChanged += new EventHandler(PluginsSupportNonRfcChanged);
        }
        
        public void Unload()
        {
            bot.OnAdmin -= new AdminEventHandler(PluginsOnAdmin);
            bot.OnAutoConnectError -= new AutoConnectErrorEventHandler(PluginsOnAutoConnectError);
            bot.OnAway -= new AwayEventHandler(PluginsOnAway);
            bot.OnBan -= new BanEventHandler(PluginsOnBan);
            bot.OnChannelAction -= new ActionEventHandler(PluginsOnChannelAction);
            bot.OnChannelActiveSynced -= new IrcEventHandler(PluginsOnChannelActiveSynced);
            bot.OnChannelMessage -= new IrcEventHandler(PluginsOnChannelMessage);
            bot.OnChannelModeChange -= new IrcEventHandler(PluginsOnChannelModeChange);
            bot.OnChannelNotice -= new IrcEventHandler(PluginsOnChannelNotice);
            bot.OnChannelPassiveSynced -= new IrcEventHandler(PluginsOnChannelPassiveSynced);
            bot.OnConnected -= new EventHandler(PluginsOnConnected);
            bot.OnConnecting -= new EventHandler(PluginsOnConnecting);
            bot.OnConnectionError -= new EventHandler(PluginsOnConnectionError);
            bot.OnCtcpReply -= new CtcpEventHandler(PluginsOnCtcpReply);
            bot.OnCtcpRequest -= new CtcpEventHandler(PluginsOnCtcpRequest);
            bot.OnDccChatReceiveLineEvent -= new DccChatLineHandler(PluginsOnDccChatReceiveLineEvent);
            bot.OnDccChatRequestEvent -= new DccConnectionHandler(PluginsOnDccChatRequestEvent);
            bot.OnDccChatSentLineEvent -= new DccChatLineHandler(PluginsOnDccChatSentLineEvent);
            bot.OnDccChatStartEvent -= new DccConnectionHandler(PluginsOnDccChatStartEvent);
            bot.OnDccChatStopEvent -= new DccConnectionHandler(PluginsOnDccChatStopEvent);
            bot.OnDccSendReceiveBlockEvent -= new DccSendPacketHandler(PluginsOnDccSendReceiveBlockEvent);
            bot.OnDccSendRequestEvent -= new DccSendRequestHandler(PluginsOnDccSendRequestEvent);
            bot.OnDccSendSentBlockEvent -= new DccSendPacketHandler(PluginsOnDccSendSentBlockEvent);
            bot.OnDccSendStartEvent -= new DccConnectionHandler(PluginsOnDccSendStartEvent);
            bot.OnDccSendStopEvent -= new DccConnectionHandler(PluginsOnDccSendStopEvent);
            bot.OnDeadmin -= new DeadminEventHandler(PluginsOnDeadmin);
            bot.OnDehalfop -= new DehalfopEventHandler(PluginsOnDehalfop);
            bot.OnDeop -= new DeopEventHandler(PluginsOnDeop);
            bot.OnDeowner -= new DeownerEventHandler(PluginsOnDeowner);
            bot.OnDevoice -= new DevoiceEventHandler(PluginsOnDevoice);
            bot.OnDisconnected -= new EventHandler(PluginsOnDisconnected);
            bot.OnDisconnecting -= new EventHandler(PluginsOnDisconnecting);
            bot.OnError -= new ErrorEventHandler(PluginsOnError);
            bot.OnErrorMessage -= new IrcEventHandler(PluginsOnErrorMessage);
            bot.OnHalfop -= new HalfopEventHandler(PluginsOnHalfop);
            bot.OnInvite -= new InviteEventHandler(PluginsOnInvite);
            bot.OnJoin -= new JoinEventHandler(PluginsOnJoin);
            bot.OnKick -= new KickEventHandler(PluginsOnKick);
            bot.OnList -= new ListEventHandler(PluginsOnList);
            bot.OnModeChange -= new IrcEventHandler(PluginsOnModeChange);
            bot.OnMotd -= new MotdEventHandler(PluginsOnMotd);
            bot.OnNames -= new NamesEventHandler(PluginsOnNames);
            bot.OnNickChange -= new NickChangeEventHandler(PluginsOnNickChange);
            bot.OnNowAway -= new IrcEventHandler(PluginsOnNowAway);
            bot.OnOp -= new OpEventHandler(PluginsOnOp);
            bot.OnOwner -= new OwnerEventHandler(PluginsOnOwner);
            bot.OnPart -= new PartEventHandler(PluginsOnPart);
            bot.OnPing -= new PingEventHandler(PluginsOnPing);
            bot.OnPong -= new PongEventHandler(PluginsOnPong);
            bot.OnQueryAction -= new ActionEventHandler(PluginsOnQueryAction);
            bot.OnQueryMessage -= new IrcEventHandler(PluginsOnQueryMessage);
            bot.OnQueryNotice -= new IrcEventHandler(PluginsOnQueryNotice);
            bot.OnQuit -= new QuitEventHandler(PluginsOnQuit);
            bot.OnRawMessage -= new IrcEventHandler(PluginsOnRawMessage);
            bot.OnReadLine -= new ReadLineEventHandler(PluginsOnReadLine);
            bot.OnRegistered -= new EventHandler(PluginsOnRegistered);
            bot.OnTopic -= new TopicEventHandler(PluginsOnTopic);
            bot.OnTopicChange -= new TopicChangeEventHandler(PluginsOnTopicChange);
            bot.OnUnAway -= new IrcEventHandler(PluginsOnUnAway);
            bot.OnUnban -= new UnbanEventHandler(PluginsOnUnban);
            bot.OnUserModeChange -= new IrcEventHandler(PluginsOnUserModeChange);
            bot.OnVoice -= new VoiceEventHandler(PluginsOnVoice);
            bot.OnWho -= new WhoEventHandler(PluginsOnWho);
            bot.OnWriteLine -= new WriteLineEventHandler(PluginsOnWriteLine);
            bot.SupportNonRfcChanged -= new EventHandler(PluginsSupportNonRfcChanged);            
        }
        
        public event AdminEventHandler              OnAdmin;
        public event AutoConnectErrorEventHandler   OnAutoConnectError;
        public event AwayEventHandler               OnAway;
        public event BanEventHandler                OnBan;
        public event ActionEventHandler             OnChannelAction;
        public event IrcEventHandler                OnChannelActiveSynced;
        
        public event IrcEventHandler                OnChannelMessage;
        public event IrcEventHandler                OnChannelModeChange;
        public event IrcEventHandler                OnChannelNotice;
        public event IrcEventHandler                OnChannelPassiveSynced;
        public event EventHandler                   OnConnected;
        public event EventHandler                   OnConnecting;
        public event EventHandler                   OnConnectionError;
        public event CtcpEventHandler               OnCtcpReply;
        public event CtcpEventHandler               OnCtcpRequest;
        public event DccChatLineHandler             OnDccChatReceiveLineEvent;
        public event DccConnectionHandler             OnDccChatRequestEvent;
        public event DccChatLineHandler             OnDccChatSentLineEvent;
        public event DccConnectionHandler             OnDccChatStartEvent;
        public event DccConnectionHandler             OnDccChatStopEvent;
        public event DccSendPacketHandler             OnDccSendReceiveBlockEvent;
        public event DccSendRequestHandler             OnDccSendRequestEvent;
        public event DccSendPacketHandler             OnDccSendSentBlockEvent;
        public event DccConnectionHandler             OnDccSendStartEvent;
        public event DccConnectionHandler             OnDccSendStopEvent;
        public event DeadminEventHandler            OnDeadmin;
        public event DehalfopEventHandler           OnDehalfop;
        public event DeopEventHandler               OnDeop;
        public event DeownerEventHandler            OnDeowner;
        public event DevoiceEventHandler            OnDevoice;
        public event EventHandler                   OnDisconnected;
        public event EventHandler                   OnDisconnecting;
        public event ErrorEventHandler              OnError;
        public event IrcEventHandler                OnErrorMessage;
        public event HalfopEventHandler             OnHalfop;
        public event InviteEventHandler             OnInvite;
        public event JoinEventHandler               OnJoin;            
        public event KickEventHandler               OnKick;
        public event ListEventHandler               OnList;
        public event IrcEventHandler                OnModeChange;
        public event MotdEventHandler               OnMotd;
        public event NamesEventHandler              OnNames;
        public event NickChangeEventHandler         OnNickChange;
        public event IrcEventHandler                OnNowAway;
        public event OpEventHandler                 OnOp;
        public event OwnerEventHandler              OnOwner;
        public event PartEventHandler               OnPart;
        public event PingEventHandler               OnPing;
        public event PongEventHandler               OnPong;
        public event IrcEventHandler                OnRawMessage;
        public event ReadLineEventHandler           OnReadLine;
        public event EventHandler                   OnRegistered;
        public event ActionEventHandler             OnQueryAction;
        public event IrcEventHandler                OnQueryMessage;
        public event IrcEventHandler                OnQueryNotice;
        public event QuitEventHandler               OnQuit;
        public event TopicEventHandler              OnTopic;
        public event TopicChangeEventHandler        OnTopicChange;
        public event UnbanEventHandler              OnUnban;
        public event IrcEventHandler                OnUnAway;
        public event IrcEventHandler                OnUserModeChange;
        public event VoiceEventHandler              OnVoice;
        public event WhoEventHandler                OnWho;
        public event WriteLineEventHandler          OnWriteLine;
        public event EventHandler                     SupportNonRfcChanged;
                
        public void PluginsOnAdmin(object sender, AdminEventArgs e)  
        {  
            this.OnAdmin(this, e);  
        }
        
        public void PluginsOnAutoConnectError(object sender, AutoConnectErrorEventArgs e)
        {
            this.OnAutoConnectError(this, e);
        }
        
        public void PluginsOnAway(object sender, AwayEventArgs e)
        {
            this.OnAway(this, e);
        }
        
        public void PluginsOnBan(object sender, BanEventArgs e)
        {
            this.OnBan(this, e);
        }
        
        public void PluginsOnChannelAction(object sender, ActionEventArgs e)
        {
            this.OnChannelAction(this, e);
        }
        
        public void PluginsOnChannelActiveSynced(object sender, IrcEventArgs e)
        {
            this.OnChannelActiveSynced(this, e);
        }
        
        public void PluginsOnChannelMessage(object sender, IrcEventArgs e)  
        {  
               this.OnChannelMessage(this, e);
        } 

        public void PluginsOnChannelModeChange(object sender, IrcEventArgs e)  
        {  
               this.OnChannelModeChange(this, e);
        } 
        
        public void PluginsOnChannelNotice(object sender, IrcEventArgs e)  
        {  
               this.OnChannelNotice(this, e);
        } 

        public void PluginsOnChannelPassiveSynced(object sender, IrcEventArgs e)  
        {  
               this.OnChannelPassiveSynced(this, e);
        } 

        public void PluginsOnConnected(object sender, EventArgs e)  
        {  
               this.OnConnected(this, e);
        } 

        public void PluginsOnConnecting(object sender, EventArgs e)  
        {  
               this.OnConnecting(this, e);
        } 

        public void PluginsOnConnectionError(object sender, EventArgs e)  
        {  
               this.OnConnectionError(this, e);
        } 

        public void PluginsOnCtcpReply(object sender, CtcpEventArgs e)  
        {  
               this.OnCtcpReply(this, e);
        } 
       
        public void PluginsOnCtcpRequest(object sender, CtcpEventArgs e)  
        {  
               this.OnCtcpRequest(this, e);
        } 

        public void PluginsOnDccChatReceiveLineEvent(object sender, DccChatEventArgs e)  
        {  
               this.OnDccChatReceiveLineEvent(this, e);
        } 

        public void PluginsOnDccChatRequestEvent(object sender, DccEventArgs e)  
        {  
               this.OnDccChatRequestEvent(this, e);
        } 

        public void PluginsOnDccChatSentLineEvent(object sender, DccChatEventArgs e)  
        {  
               this.OnDccChatSentLineEvent(this, e);
        } 

        public void PluginsOnDccChatStartEvent(object sender, DccEventArgs e)  
        {  
               this.OnDccChatStartEvent(this, e);
        } 
        
        public void PluginsOnDccChatStopEvent(object sender, DccEventArgs e)  
        {  
               this.OnDccChatStopEvent(this, e);
        } 

        public void PluginsOnDccSendReceiveBlockEvent(object sender, DccSendEventArgs e)  
        {  
               this.OnDccSendReceiveBlockEvent(this, e);
        } 

        public void PluginsOnDccSendRequestEvent(object sender, DccSendRequestEventArgs e)  
        {  
               this.OnDccSendRequestEvent(this, e);
        } 

        public void PluginsOnDccSendSentBlockEvent(object sender, DccSendEventArgs e)  
        {  
               this.OnDccSendSentBlockEvent(this, e);
        } 

        public void PluginsOnDccSendStartEvent(object sender, DccEventArgs e)  
        {  
               this.OnDccSendStartEvent(this, e);
        } 

        public void PluginsOnDccSendStopEvent(object sender, DccEventArgs e)  
        {  
               this.OnDccSendStopEvent(this, e);
        } 

        public void PluginsOnDeadmin (object sender, DeadminEventArgs e)  
        {  
               this.OnDeadmin(this, e);
        } 

        public void PluginsOnDehalfop(object sender, DehalfopEventArgs e)  
        {  
               this.OnDehalfop(this, e);
        } 

        public void PluginsOnDeop(object sender, DeopEventArgs e)  
        {  
               this.OnDeop(this, e);
        } 

        public void PluginsOnDeowner(object sender, DeownerEventArgs e)  
        {  
               this.OnDeowner(this, e);
        } 

        public void PluginsOnDevoice(object sender, DevoiceEventArgs e)
        {
               this.OnDevoice(this, e);
        } 

        public void PluginsOnDisconnected(object sender, EventArgs e)  
        {  
               this.OnDisconnected(this, e);
        } 

        public void PluginsOnDisconnecting(object sender, EventArgs e)  
        {  
               this.OnDisconnecting(this, e);
        } 

       public void PluginsOnError(object sender, ErrorEventArgs e)  
        {  
               this.OnError(this, e);
        } 

        public void PluginsOnErrorMessage(object sender, IrcEventArgs e)  
        {  
               this.OnErrorMessage(this, e);
        } 

        public void PluginsOnHalfop(object sender, HalfopEventArgs e)  
        {  
               this.OnHalfop(this, e);
        }        

        public void PluginsOnInvite(object sender, InviteEventArgs e)  
        {  
               this.OnInvite(this, e);
        } 

        public void PluginsOnJoin(object sender, JoinEventArgs e)  
        {  
               this.OnJoin(this, e);
        } 

        public void PluginsOnKick(object sender, KickEventArgs e)  
        {  
               this.OnKick(this, e);
        } 
        
        public void PluginsOnList(object sender, ListEventArgs e)  
        {  
               this.OnList(this, e);
        } 

        public void PluginsOnModeChange(object sender, IrcEventArgs e)  
        {  
               this.OnModeChange(this, e);
        } 

        public void PluginsOnMotd(object sender, MotdEventArgs e)  
        {  
               this.OnMotd(this, e);
        } 

        public void PluginsOnNames(object sender, NamesEventArgs e)  
        {  
               this.OnNames(this, e);
        } 

        public void PluginsOnNickChange(object sender, NickChangeEventArgs e)  
        {  
               this.OnNickChange(this, e);
        } 

        public void PluginsOnNowAway(object sender,  IrcEventArgs e)  
        {  
               this.OnNowAway(this, e);
        } 

        public void PluginsOnOp(object sender, OpEventArgs e)  
        {  
               this.OnOp(this, e);
        } 

        public void PluginsOnOwner(object sender, OwnerEventArgs e)  
        {  
               this.OnOwner(this, e);
        } 

        public void PluginsOnPart(object sender, PartEventArgs e)  
        {  
               this.OnPart(this, e);
        } 
        
        public void PluginsOnPing(object sender, PingEventArgs e)  
        {  
               this.OnPing(this, e);
        } 
        
        public void PluginsOnPong(object sender, PongEventArgs e)  
        {  
               this.OnPong(this, e);
        } 

        public void PluginsOnQueryAction(object sender, ActionEventArgs e)  
        {  
               this.OnQueryAction(this, e);
        }     

        public void PluginsOnQueryMessage(object sender, IrcEventArgs e)  
        {  
               this.OnQueryMessage(this, e);
        } 

        public void PluginsOnQueryNotice(object sender, IrcEventArgs e)  
        {  
               this.OnQueryNotice(this, e);
        } 

        public void PluginsOnQuit(object sender, QuitEventArgs e)  
        {  
               this.OnQuit(this, e);
        } 

        public void PluginsOnRawMessage(object sender, IrcEventArgs e)
        {  
            this.OnRawMessage(this, e);
        } 

        public void PluginsOnReadLine(object sender, ReadLineEventArgs e)  
        {  
               this.OnReadLine(this, e);
        } 

        public void PluginsOnRegistered(object sender, EventArgs e)  
        {  
               this.OnRegistered(this, e);
        } 
       
        public void PluginsOnTopic(object sender, TopicEventArgs e)  
        {  
               this.OnTopic(this, e);
        } 

        public void PluginsOnTopicChange(object sender, TopicChangeEventArgs e)  
        {  
               this.OnTopicChange(this, e);
        } 

        public void PluginsOnUnAway(object sender, IrcEventArgs e)  
        {  
               this.OnUnAway(this, e);
        } 

        public void PluginsOnUnban(object sender, UnbanEventArgs e)
        {  
               this.OnUnban(this, e);
        } 

        public void PluginsOnUserModeChange(object sender, IrcEventArgs e)  
        {  
               this.OnUserModeChange(this, e);
        } 

        public void PluginsOnVoice(object sender, VoiceEventArgs e)  
        {  
               this.OnVoice(this, e);
        }   

        public void PluginsOnWho(object sender, WhoEventArgs e)  
        {  
               this.OnWho(this, e);
        } 

        public void PluginsOnWriteLine(object sender, WriteLineEventArgs e)  
        {  
               this.OnWriteLine(this, e);
        } 

        public void PluginsSupportNonRfcChanged(object sender, EventArgs e)  
        {  
               this.SupportNonRfcChanged(this, e);
        }                 
    }
}
