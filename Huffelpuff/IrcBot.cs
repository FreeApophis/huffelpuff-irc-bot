/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 08.10.2008
 * Zeit: 01:50
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */

using Mono.Nat;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Threading;

using Huffelpuff.Plugins;
using Huffelpuff.Tools;

using Meebey.SmartIrc4net;

namespace Huffelpuff
{
    
    /// <summary>
    /// Description of IrcBot.
    /// </summary>
    [Serializable]
    public class IrcBot : IrcFeatures
    {
        private BotPluginManager plugManager;
        
        private AccessControlList acl;
        
        public AccessControlList Acl {
            get { return acl; }
        }
        
        private Dictionary<string, Commandlet>  commands = new Dictionary<string, Commandlet>(StringComparer.CurrentCultureIgnoreCase);
        
        private bool uPnPSupport;
        
        public bool UPnPSupport {
            get { return uPnPSupport; }
        }

        public const string channelconst = "channel";
        
        public IrcBot()
        {
            this.Encoding = System.Text.Encoding.UTF8;
            this.SendDelay = 3000;
            this.PingInterval = 120;
            this.ActiveChannelSyncing = true;
            this.OnRawMessage += RawMessageHandler;
            this.AutoRejoin = true;
            this.AutoRetry = true;
            this.AutoRetryDelay = 5;
            this.SupportNonRfc = true;
            this.OnChannelMessage += CommandDispatcher;
            this.OnQueryMessage +=  CommandDispatcher;
            this.OnConnected += OnBotConnected;
            
            this.CtcpVersion = this.VersionString + " (SmartIrc4Net " + Assembly.GetAssembly(typeof(IrcFeatures)).GetName(false).Version + ")";
            this.CtcpUrl = "http://huffelpuff-irc-bot.origo.ethz.ch/";
            this.CtcpSource = "https://svn.origo.ethz.ch/huffelpuff-irc-bot/";

            // DCC Setup
            
            /* todo: find NAT / or our IP */
            
            //Setting up Access Control
            acl = new AccessControlList(this);
            acl.Init();
            
            // Add Identify Plugin suitable (or all)
            acl.AddIdentifyPlugin(new NickServIdentify(this));
            acl.AddIdentifyPlugin(new HostIdentify(this));
            acl.AddIdentifyPlugin(new PasswordIdentify(this));
            acl.AddIdentifyPlugin(new NickIdentify(this));

            // Plugin needs the Handlers from IRC we load the plugins after we set everything up
            plugManager = new BotPluginManager(this, "plugins");
            
            //Basic Commands
            this.AddCommand(new Commandlet("!join", "The command !join <channel> lets the bot join Channel <channel>", this.JoinCommand, this, CommandScope.Both, "engine_join"));
            this.AddCommand(new Commandlet("!part", "The command !part <channel> lets the bot part Channel <channel>", this.PartCommand, this, CommandScope.Both,"engine_part") );
            this.AddCommand(new Commandlet("!channels", "The command !channels lists all channels where the bot resides", this.ListChannelCommand, this, CommandScope.Both) );
            this.AddCommand(new Commandlet("!quit", "The command !quit lets the bot quit himself", this.QuitCommand, this, CommandScope.Both, "engine_quit"));
            
            //Plugin Commands
            this.AddCommand(new Commandlet("!plugins", "The command !plugins lists all the plugins", this.PluginsCommand, this, CommandScope.Both));
            this.AddCommand(new Commandlet("!activate", "The command !activate <plugin> activates the Plugin <plugin>", this.ActivateCommand, this, CommandScope.Both, "engine_activate"));
            this.AddCommand(new Commandlet("!deactivate", "The command !deactivate <plugin> deactivates the Plugin <plugin>", this.DeactivateCommand, this, CommandScope.Both, "engine_deactivate"));
            
            //Helper Commands (!commands)
            this.AddCommand(new Commandlet("!help", "The command !help <topic> gives you help about <topic> (special topics: commands, more)", this.HelpCommand, this, CommandScope.Both));
            
            //Settings Commands
            new SettingCommands(this);
            
        }

        void OnBotConnected(object sender, EventArgs e)
        {
            if(PersistentMemory.Instance.GetValue("nickserv") != null) {
                this.SendMessage(SendType.Message, "nickserv", "identify {0}".Fill(PersistentMemory.Instance.GetValue("nickserv")), Priority.Critical);
            }
        }

        
        internal void CleanPlugins()
        {
            // try catch AppDomainUnloadedExceptions (somehow)
            List<string> del = new List<string>();
            foreach(KeyValuePair<string, Commandlet> p in commands) {
                if (p.Value.Handler==null) {
                    del.Add(p.Key);
                }
            }
            foreach(string s in del) {
                commands.Remove(s);
            }
        }
        
        private void RawMessageHandler(object sender, IrcEventArgs e)
        {
            Log.Instance.Log(e.Data.RawMessage, Level.Trace);
        }
        
        
        /// <summary>
        /// New API: obsoletes Public and Private Command, Commandlet has changed too
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool AddCommand(Commandlet cmd) {
            if (cmd.AccessString != null)
                this.acl.AddAccessString(cmd.AccessString);
            if(!commands.ContainsKey(cmd.Command)) {
                commands.Add(cmd.Command, cmd);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// New API: obsoletes Public and Private Command, Commandlet has changed too
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool RemoveCommand(string command) {
            if(commands.ContainsKey(command))
            {
                commands.Remove(command);
                return true;
            }
            return false;
        }
        
        private void CommandDispatcher(object sender, IrcEventArgs e)
        {
            bool pub = !string.IsNullOrEmpty(e.Data.Channel);
            if (commands.ContainsKey(e.Data.MessageArray[0])) {
                if((pub && (commands[e.Data.MessageArray[0]].Scope == CommandScope.Private)) ||
                   ((!pub) && (commands[e.Data.MessageArray[0]].Scope == CommandScope.Public)))
                    return;
                
                // check if access to function is allowed
                if(!string.IsNullOrEmpty(commands[e.Data.MessageArray[0]].AccessString) && !this.acl.Access(e.Data.Nick, commands[e.Data.MessageArray[0]].AccessString, true))
                    return;
                
                if((commands[e.Data.MessageArray[0]].ChannelList != null) && (!commands[e.Data.MessageArray[0]].ChannelList.Contains(e.Data.Channel)))
                    return;
                
                if (commands[e.Data.MessageArray[0]].Handler != null) {
                    commands[e.Data.MessageArray[0]].Handler.Invoke(sender, e);
                } else {
                    foreach(AbstractPlugin plug in plugManager.Plugins) {
                        if (plug.FullName == (string)commands[e.Data.MessageArray[0]].Owner)
                        {
                            plug.InvokeHandler(commands[e.Data.MessageArray[0]].HandlerName, e);
                            return;
                        }
                    }
                }
            }
        }
        
        private void PluginsCommand(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            List<string> pluginsList = new List<string>();
            
            foreach(AbstractPlugin p in plugManager.Plugins) {
                pluginsList.Add(IrcConstants.IrcBold + p.FullName+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]"+ IrcConstants.IrcBold);
            }
            foreach(string line in pluginsList.ToLines(300, ", ", "Plugins Loaded: " , " END.")) {
                SendMessage(SendType.Message, sendto, line);
            }
        }
        
        private void ActivateCommand(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            if (e.Data.MessageArray.Length < 2) {
                SendMessage(SendType.Message, sendto, "Too few arguments! Try !help.");
                return;
            }
            List<AbstractPlugin> calledPlugins = new List<AbstractPlugin>();
            foreach(string param in e.Data.MessageArray.Skip(1)) {
                foreach(AbstractPlugin plugin in plugManager.Plugins.Where(p => p.FullName == param || p.MainClass == param)) {
                    if (!plugin.Active) {
                        PersistentMemory.Instance.SetValue("plugin", plugin.FullName);
                        PersistentMemory.Instance.Flush();
                        plugin.Activate();
                    }
                    calledPlugins.Add(plugin);
                }
            }
            foreach(var line in calledPlugins.Select(plugin => "" + plugin.FullName + " ["+((plugin.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]").ToLines(350, ", ", "Plugins: ", " END.")) {
                SendMessage(SendType.Message, sendto, line);
            }
        }

        private void DeactivateCommand(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            if (e.Data.MessageArray.Length < 2) {
                SendMessage(SendType.Message, sendto, "Too few arguments! Try !help.");
                return;
            }
            List<AbstractPlugin> calledPlugins = new List<AbstractPlugin>();
            foreach(string param in e.Data.MessageArray.Skip(1)) {
                foreach(AbstractPlugin plugin in plugManager.Plugins.Where(p => p.FullName == param || p.MainClass == param)) {
                    if (plugin.Active) {
                        PersistentMemory.Instance.RemoveValue("plugin", plugin.FullName);
                        PersistentMemory.Instance.Flush();
                        plugin.Deactivate();
                    }
                    calledPlugins.Add(plugin);
                }
            }
            foreach(var line in calledPlugins.Select(plugin => "" + plugin.FullName + " ["+((plugin.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]").ToLines(350, ", ", "Plugins: ", " END.")) {
                SendMessage(SendType.Message, sendto, line);
            }
        }
        
        private void JoinCommand(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            if (e.Data.MessageArray.Length < 2) {
                SendMessage(SendType.Message, sendto, "Too few arguments! Try !help.");
                return;
            }
            foreach(var channel in e.Data.MessageArray.Skip(1)) {
                if (!channel.IsNullOrEmpty()) {
                    this.RfcJoin(channel);
                    PersistentMemory.Instance.RemoveValue("channel", channel);
                    PersistentMemory.Instance.SetValue("channel", channel);
                }
            }
            PersistentMemory.Instance.Flush();
        }
        
        private void ListChannelCommand(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            foreach(string line in PersistentMemory.Instance.GetValues(channelconst).ToLines(350, ", ", "I am in the following channels: " , " END.")) {
                SendMessage(SendType.Message, sendto, line);
            }
        }
        
        private void PartCommand(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            if (e.Data.MessageArray.Length < 2) {
                SendMessage(SendType.Message, sendto, "Too few arguments! Try !help.");
                return;
            }
            
            this.RfcPart(e.Data.Message.Substring(6));
            
            PersistentMemory.Instance.RemoveValue("channel", e.Data.MessageArray[1]);
            PersistentMemory.Instance.Flush();
        }
        
        private void QuitCommand(object sender, IrcEventArgs e)
        {
            if(e.Data.MessageArray.Length > 1)
                this.RfcQuit(e.Data.Message.Substring(6), Priority.Low);
            else
                this.RfcQuit(Priority.Low);
        }
        
        private void HelpCommand(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            if (e.Data.MessageArray.Length < 2) {
                this.SendMessage(SendType.Message, sendto, "You can use the help command to get information about Plugins and Commands:");
                this.SendMessage(SendType.Message, sendto, "Type !help commands (for commandlist), !plugins (for plugin list), !help <plugin> (for help about <plugin>), !help <command> (for help about <command>)");
            } else {
                sendHelp(e.Data.MessageArray[1], sendto, e.Data.Nick);
            }
        }
        
        private void sendHelp(string topic, string sendto, string nick)
        {
            Dictionary<CommandScope, string> scopeColor = new Dictionary<CommandScope, string>();
            scopeColor.Add(CommandScope.Private, ("" + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.LightRed));
            scopeColor.Add(CommandScope.Public, ("" + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.Blue));
            scopeColor.Add(CommandScope.Both, ("" + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.Purple));
            
            topic = topic.ToLower();
            bool helped = false;
            
            // List all commands
            if (topic == "commands") {
                List<string> commandlist = new List<string>();
                foreach(Commandlet cmd in commands.Values) {
                    if(string.IsNullOrEmpty(cmd.AccessString)) {
                        commandlist.Add(scopeColor[cmd.Scope] + cmd.Command + IrcConstants.IrcColor + IrcConstants.IrcBold);
                    } else if (this.acl.Access(nick, cmd.AccessString, false)) {
                        commandlist.Add(scopeColor[cmd.Scope] + "<" + cmd.Command + ">" + IrcConstants.IrcColor + IrcConstants.IrcBold);
                        //commandlist.Add("" + IrcConstants.IrcColor + (int)IrcColors.Green + "<" + scopeColor[cmd.Scope] + cmd.Command + IrcConstants.IrcColor + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.Green +  ">" + IrcConstants.IrcColor);
                    } else {
                        //commandlist.Add("<" + scopeColor[cmd.Scope] + cmd.Command + IrcConstants.IrcColor + IrcConstants.IrcBold + ">");
                    }
                }
                
                foreach(string com in commandlist.ToLines(350, ", ", "Active Commands (" + scopeColor[CommandScope.Public] + "public" + IrcConstants.IrcColor + IrcConstants.IrcBold + ", " + scopeColor[CommandScope.Private] + "private" + IrcConstants.IrcColor + IrcConstants.IrcBold + ") <restricted>: ", null))
                {
                    this.SendMessage(SendType.Message, sendto, com);
                }
                helped = true;
            }
            
            // debug list of currently loaded commands
            if(topic == "more") {
                foreach(Commandlet cmd in commands.Values)
                {
                    string owner = (cmd.Handler==null)?(string)cmd.Owner + "~":cmd.Owner.GetType().ToString();
                    this.SendMessage(SendType.Message, sendto, "Command (Scope:" + cmd.Scope.ToString() + "):" + cmd.Command + ", offered by " + owner + " and help provided: " + cmd.HelpText + ((string.IsNullOrEmpty(cmd.AccessString))?"":" (accessString=" + cmd.AccessString + ")"));
                }
                helped = true;
            }
            
            // maybe in some command?
            foreach(Commandlet cmd in commands.Values) {
                if ((cmd.Command==topic) || (cmd.Command.Substring(1)==topic) || (cmd.Command==topic.Substring(1))) {
                    this.SendMessage(SendType.Message, sendto, cmd.HelpText + ((string.IsNullOrEmpty(cmd.AccessString))?"":" (access restricted)"));
                    helped = true;
                }
            }

            // maybe a  plugin
            foreach(AbstractPlugin p in plugManager.Plugins) {
                bool plugHelp = false;
                if (topic==p.FullName.ToLower()) {
                    plugHelp = true;
                }
                foreach(string s in p.FullName.ToLower().Split(new char[] {'.'}))
                {
                    if((topic==s)&&(!helped)) {
                        plugHelp = true;
                    }
                    
                }
                if(plugHelp) {
                    this.SendMessage(SendType.Message, sendto, p.AboutHelp());
                    helped = true;
                }
            }

            
            if (!helped)
                this.SendMessage(SendType.Message, sendto, "Your Helptopic was not found");
        }

        public void Exit()
        {
            PersistentMemory.Instance.Flush();
            
            plugManager.ShutDown();
            
            // we are done, lets exit...
            Log.Instance.Log("Exiting...");
            #if DEBUG
            System.Threading.Thread.Sleep(60000);
            #endif
            
            System.Environment.Exit(0);
        }



        public void Start()
        {
            Thread.CurrentThread.Name = "Main";
            
            if (PersistentMemory.Instance.GetValue("ProxyServer") != null) {
                Log.Instance.Log("Using Proxy Server: " + PersistentMemory.Instance.GetValue("ProxyServer"));
                this.ProxyType = Org.Mentalis.Network.ProxySocket.ProxyTypes.Socks5;
                this.ProxyEndPoint = new IPEndPoint(IPAddress.Parse(PersistentMemory.Instance.GetValue("ProxyServer").Split(new char[] {':'})[0]), int.Parse(PersistentMemory.Instance.GetValue("ProxyServer").Split(new char[] {':'})[1]));
                this.ProxyUser = PersistentMemory.Instance.GetValue("ProxyUser");
                this.ProxyPass = PersistentMemory.Instance.GetValue("ProxyPass");
            }

            // the server we want to connect to
            string[] serverlist = PersistentMemory.Instance.GetValuesOrTodo("ServerHost").ToArray();
            int port = int.Parse(PersistentMemory.Instance.GetValueOrTodo("ServerPort"));
            try {
                // here we try to connect to the server and exceptions get handled
                this.Connect(serverlist, port);
                Log.Instance.Log("successfull connected");
            } catch (ConnectionException e) {
                // something went wrong, the reason will be shown
                Log.Instance.Log("couldn't connect! Reason: "+e.Message);
                Exit();
            }
            
            try {
                // here we logon and register our nickname and so on
                this.Login(PersistentMemory.Instance.GetValueOrTodo("nick"), PersistentMemory.Instance.GetValueOrTodo("realname"), 4, PersistentMemory.Instance.GetValueOrTodo("username"), PersistentMemory.Instance.GetValue("serverpass"));
                
                // join the channels
                foreach(string channel in PersistentMemory.Instance.GetValues(channelconst))
                {
                    this.RfcJoin(channel);
                }
                
                this.Listen();
                this.Disconnect();
            } catch (ConnectionException) {
                Exit();
            } catch (Exception e) {
                // this should not happen by just in case we handle it nicely
                Log.Instance.Log("Error occurred! Message: "+e.Message, Level.Error);
                Log.Instance.Log("Exception: "+e.StackTrace, Level.Error);
                Exit();
            }
        }
    }
}
