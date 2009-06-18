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
using System.Reflection;
using System.Threading;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace Huffelpuff
{
    
    /// <summary>
    /// Description of IrcBot.
    /// </summary>
    public class IrcBot : IrcFeatures
    {
        private BotPluginManager plugManager;
        
        private AccessControlList acl;
        private Dictionary<string, Commandlet>  commands = new Dictionary<string, Commandlet>(StringComparer.CurrentCultureIgnoreCase);
        
        private bool uPnPSupport;
        
        public bool UPnPSupport {
            get { return uPnPSupport; }
        }

        public IrcBot()
        {
            this.Encoding = System.Text.Encoding.UTF8;
            this.SendDelay = 2000;
            this.PingInterval = 120;
            this.ActiveChannelSyncing = true;
            this.OnRawMessage += new IrcEventHandler(RawMessageHandler);
            this.AutoRejoin = true;
            this.AutoRetry = true;
            this.AutoRetryDelay = 5;
            
            this.OnChannelMessage += new IrcEventHandler(CommandDispatcher);
            this.OnQueryMessage +=  new IrcEventHandler(CommandDispatcher);
            
            this.CtcpVersion = this.VersionString + " (SmartIrc4Net " + Assembly.GetAssembly(typeof(IrcFeatures)).GetName(false).Version + ")";
            this.CtcpUrl = "http://huffelpuff-irc-bot.origo.ethz.ch/";
            this.CtcpSource = "https://svn.origo.ethz.ch/huffelpuff-irc-bot/";

            // DCC Setup
            NatUtility.DeviceFound += delegate(object sender, DeviceEventArgs e) {
                INatDevice device = e.Device;
                Console.WriteLine("NAT Device found: " + e.Device.ToString());
                Console.WriteLine("External IP: " + e.Device.GetExternalIP());
                Console.WriteLine("LastSeen: " + e.Device.LastSeen);
            };
            
            NatUtility.DeviceLost += delegate(object sender, DeviceEventArgs e) {
                INatDevice device = e.Device;
                Console.WriteLine ("Device Lost");
                Console.WriteLine ("Type: {0}", device.GetType().Name);
            };

            
            /*
            this.uPnPSupport = UPnP.NAT.Discover();
            this.ExternalIpAdress = Tools.TryGetExternalIP();
            if(Tools.LocalIP != null) {
                Console.WriteLine("Int IP : " + Tools.LocalIP);
            }
            Console.WriteLine("Ext IP : " + this.ExternalIpAdress);
             */
            
            // Plugin needs the Handlers from IRC we load the plugins after we set everything up
            plugManager = new BotPluginManager(this, "plugins");
            
            //Setting up Access Control
            acl = new AccessControlList(this);
            acl.Init();
            
            // Add Identify Plugin suitable (or all)
            acl.AddIdentifyPlugin(new NickServIdentify(this));
            acl.AddIdentifyPlugin(new HostIdentify(this));
            acl.AddIdentifyPlugin(new PasswordIdentify(this));
            
            this.AddCommand(new Commandlet("!join", "The command !join <channel> lets the bot join Channel <channel>", this.JoinCommand, this, CommandScope.Both, "engine_join"));
            this.AddCommand(new Commandlet("!part", "The command !part <channel> lets the bot part Channel <channel>", this.PartCommand, this, CommandScope.Both,"engine_part") );
            this.AddCommand(new Commandlet("!quit", "The command !quit lets the bot quit himself", this.QuitCommand, this, CommandScope.Both, "engine_quit"));
            this.AddCommand(new Commandlet("!help", "The command !help <topic> gives you help about <topic> (special topics: commands, more)", this.HelpCommand, this, CommandScope.Both));
            this.AddCommand(new Commandlet("!plugins", "The command !plugins lists all the plugins", this.PluginsCommand, this, CommandScope.Both));
            this.AddCommand(new Commandlet("!activate", "The command !activate <plugin> activates the Plugin <plugin>", this.ActivateCommand, this, CommandScope.Both, "engine_activate"));
            this.AddCommand(new Commandlet("!deactivate", "The command !deactivate <plugin> deactivates the Plugin <plugin>", this.DeactivateCommand, this, CommandScope.Both, "engine_deactivate"));

        }

        
        internal void CleanPlugins()
        {
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
            Console.WriteLine(e.Data.RawMessage);
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
                //if(!string.IsNullOrEmpty(commands[e.Data.MessageArray[0]].AccessString) && !this.acl.Access(e.Data.Nick, commands[e.Data.MessageArray[0]].AccessString))
                //    return;
                
                if((commands[e.Data.MessageArray[0]].ChannelList != null) && (!commands[e.Data.MessageArray[0]].ChannelList.Contains(e.Data.Channel)))
                    return;
                
                if (commands[e.Data.MessageArray[0]].Handler != null) {
                    commands[e.Data.MessageArray[0]].Handler.Invoke(sender, e);
                } else {
                    foreach(AbstractPlugin plug in plugManager.Plugins) {
                        if (plug.FullName == (string)commands[e.Data.MessageArray[0]].Owner)
                        {
                            plug.InvokeHandler(commands[e.Data.MessageArray[0]].HandlerName, e);
                        }
                    }
                }
            }
        }
        
        private void PluginsCommand(object sender, IrcEventArgs e)
        {
            List<string> pluginsList = new List<string>();
            
            foreach(AbstractPlugin p in plugManager.Plugins) {
                pluginsList.Add(IrcConstants.IrcBold + p.FullName+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]"+ IrcConstants.IrcBold);
            }
            foreach(string line in ListToLines(pluginsList, 300, ", ", "Plugins Loaded: " , " END.")) {
                SendMessage(SendType.Notice, e.Data.Channel, line);
            }
        }
        
        private void ActivateCommand(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length < 2)
                return;
            foreach(AbstractPlugin p in plugManager.Plugins) {
                if (e.Data.MessageArray[1]==p.FullName) {
                    PersistentMemory.SetValue("plugin", p.FullName);
                    PersistentMemory.Flush();
                    p.Activate();
                    SendMessage(SendType.Notice, e.Data.Channel, "Plugin: "+IrcConstants.IrcBold+p.FullName+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]");
                }
            }
        }

        private void DeactivateCommand(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length < 2)
                return;
            foreach(AbstractPlugin p in plugManager.Plugins) {
                if (e.Data.MessageArray[1]==p.FullName) {
                    PersistentMemory.RemoveValue("plugin", p.FullName);
                    PersistentMemory.Flush();
                    p.Deactivate();
                    SendMessage(SendType.Notice, e.Data.Channel, "Plugin: "+IrcConstants.IrcBold+p.FullName+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]");
                }
            }
        }
        
        private void JoinCommand(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length < 2)
                return;
            this.RfcJoin(e.Data.MessageArray[1]);
            PersistentMemory.RemoveValue("channel", e.Data.MessageArray[1]);
            PersistentMemory.SetValue("channel", e.Data.MessageArray[1]);
            PersistentMemory.Flush();
        }
        
        private void PartCommand(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length < 2)
                return;
            this.RfcPart(e.Data.MessageArray[1]);
            PersistentMemory.RemoveValue("channel", e.Data.MessageArray[1]);
            PersistentMemory.Flush();
        }
        
        private void QuitCommand(object sender, IrcEventArgs e)
        {
            if(e.Data.MessageArray.Length > 1)
                this.RfcQuit(e.Data.MessageArray[1], Priority.Low);
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
                    } else if (this.acl.Access(nick, cmd.AccessString)) {
                        commandlist.Add("" + IrcConstants.IrcColor + (int)IrcColors.Green + "<" + scopeColor[cmd.Scope] + cmd.Command + IrcConstants.IrcColor + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.Green +  ">" + IrcConstants.IrcColor);
                    } else {
                        commandlist.Add("<" + scopeColor[cmd.Scope] + cmd.Command + IrcConstants.IrcColor + IrcConstants.IrcBold + ">");
                    }
                }
                
                foreach(string com in ListToLines(commandlist, 350, ", ", "Active Commands (" + scopeColor[CommandScope.Public] + "public" + IrcConstants.IrcColor + IrcConstants.IrcBold + ", " + scopeColor[CommandScope.Private] + "private" + IrcConstants.IrcColor + IrcConstants.IrcBold + ") <restricted>: ", null))
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

        public List<string> ListToLines(IEnumerable<string> list, int maxlinelength)
        {
            return ListToLines(list, maxlinelength, ", ");
        }


        public List<string> ListToLines(IEnumerable<string> list, int maxlinelength, string seperator)
        {
            return ListToLines(list, maxlinelength, ", ", null, null);
        }


        public List<string> ListToLines(IEnumerable<string> list, int maxlinelength, string seperator, string prefix, string postfix)
        {
            if (prefix==null) {
                prefix="";
            }
            bool noSeparator = true;
            
            List<string> result = new List<string>();
            result.Add(prefix);
            
            
            foreach(string s in list)
            {
                if(result[result.Count-1].Length + s.Length + seperator.Length > maxlinelength) {
                    if(!noSeparator) {
                        result[result.Count-1]=result[result.Count-1]+seperator;
                    }
                    result.Add("");
                    noSeparator = true;
                }
                if (noSeparator) {
                    result[result.Count-1]=result[result.Count-1]+s;
                    noSeparator = false;
                } else {
                    result[result.Count-1]=result[result.Count-1]+seperator+s;
                }
            }
            if (!string.IsNullOrEmpty(postfix)) {
                if(result[result.Count-1].Length + postfix.Length > maxlinelength) {
                    result.Add("");
                }
                result[result.Count-1]=result[result.Count-1]+postfix;
            }
            return result;
        }

        public void Exit()
        {
            PersistentMemory.Flush();
            
            plugManager.ShutDown();
            
            // we are done, lets exit...
            System.Console.WriteLine("Exiting...");
            #if DEBUG
            System.Threading.Thread.Sleep(60000);
            #endif
            
            System.Environment.Exit(0);
        }



        public void Start()
        {
            Thread.CurrentThread.Name = "Main";
            
            if (PersistentMemory.GetValue("ProxyServer") != null) {
                Console.WriteLine("Using Proxy Server: " + PersistentMemory.GetValue("ProxyServer"));
                this.ProxyType = Org.Mentalis.Network.ProxySocket.ProxyTypes.Socks5;
                this.ProxyEndPoint = new IPEndPoint(IPAddress.Parse(PersistentMemory.GetValue("ProxyServer").Split(new char[] {':'})[0]), int.Parse(PersistentMemory.GetValue("ProxyServer").Split(new char[] {':'})[1]));
                this.ProxyUser = PersistentMemory.GetValue("ProxyUser");
                this.ProxyPass = PersistentMemory.GetValue("ProxyPass");
            }

            // the server we want to connect to
            string[] serverlist = PersistentMemory.GetValues("ServerHost").ToArray();
            int port = int.Parse(PersistentMemory.GetValue("ServerPort"));
            try {
                // here we try to connect to the server and exceptions get handled
                this.Connect(serverlist, port);
                Console.WriteLine("successfull connected");
            } catch (ConnectionException e) {
                // something went wrong, the reason will be shown
                System.Console.WriteLine("couldn't connect! Reason: "+e.Message);
                Exit();
            }
            
            try {
                // here we logon and register our nickname and so on
                this.Login(PersistentMemory.GetValue("nick"), PersistentMemory.GetValue("realname"), 4, PersistentMemory.GetValue("username"), PersistentMemory.GetValue("serverpass"));
                
                // join the channels
                foreach(string channel in PersistentMemory.GetValues("channel"))
                {
                    this.RfcJoin(channel);
                }
                
                this.Listen();
                this.Disconnect();
            } catch (ConnectionException) {
                Exit();
            } catch (Exception e) {
                // this should not happen by just in case we handle it nicely
                System.Console.WriteLine("Error occurred! Message: "+e.Message);
                System.Console.WriteLine("Exception: "+e.StackTrace);
                Exit();
            }
        }
    }
}
