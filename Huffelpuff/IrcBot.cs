/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 08.10.2008
 * Zeit: 01:50
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */

using System;
using System.Net;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


using Meebey.SmartIrc4net;
using Huffelpuff.SimplePlugins;
using Huffelpuff.ComplexPlugins;

namespace Huffelpuff
{
    
    /// <summary>
    /// Description of IrcBot.
    /// </summary>
    public class IrcBot : IrcFeatures 
    {
        private SimplePluginManager simplePM;
        private ComplexPluginManager complexPM;
        
        private AccessControlList acl;
        private Dictionary<string, Commandlet>  _privateCommands = new Dictionary<string, Commandlet>(StringComparer.CurrentCultureIgnoreCase);
        private Dictionary<string, Commandlet>  _publicCommands = new Dictionary<string, Commandlet>(StringComparer.CurrentCultureIgnoreCase);

        public IrcBot()
        {           
            this.Encoding = System.Text.Encoding.UTF8;
            this.SendDelay = 2000;
            this.PingInterval = 120;
            this.ActiveChannelSyncing = true;        
            this.OnRawMessage += new IrcEventHandler(RawMessageHandler);
            
            
            this.OnChannelMessage += new IrcEventHandler(PublicCommandDispatcher);
            this.OnQueryMessage +=  new IrcEventHandler(PrivateCommandDispatcher);
            
            this.AddPublicCommand(new Commandlet("!join", "The command !join <channel> lets the bot join Channel <channel>", JoinCommand, this));
            this.AddPublicCommand(new Commandlet("!part", "The command !part <channel> lets the bot part Channel <channel>", PartCommand, this));
            this.AddPublicCommand(new Commandlet("!quit", "The command !quit lets the bot quit himself", QuitCommand, this));
            this.AddPublicCommand(new Commandlet("!help", "The command !help <topic> gives you help about <topic> (special topics: commands, more)", HelpCommand, this));
			this.AddPublicCommand(new Commandlet("!plugins", "The command !plugins lists all the plugins", this.PluginsCommand, this));
			this.AddPublicCommand(new Commandlet("!activate", "The command !activate <plugin> activates the Plugin <plugin>", this.ActivateCommand, this));
			this.AddPublicCommand(new Commandlet("!deactivate", "The command !deactivate <plugin> deactivates the Plugin <plugin>", this.DeactivateCommand, this));
                        
            this.CtcpVersion = "Huffelpuff Testing Bot: Based on SmartIRC4net 4.5.0svn + DCC";
            
            if (PersistentMemory.GetValue("external_ip")!=null)
                this.ExternalIpAdress = System.Net.IPAddress.Parse(PersistentMemory.GetValue("external_ip"));
            else 
                this.ExternalIpAdress = System.Net.IPAddress.Parse("127.0.0.1");
            
            // Plugin needs the Handlers from IRC we load the plugins after we set everything up
            simplePM = new SimplePluginManager(this);
            complexPM = new ComplexPluginManager(this);
           
            
            //Access Control
            acl = new AccessControlList();
        }

        private void RawMessageHandler(object sender, IrcEventArgs e)
        {
            Console.WriteLine(e.Data.RawMessage);
        }
        
        public bool AddPrivateCommand(Commandlet cmd)
        {
        	if(!_privateCommands.ContainsKey(cmd.Command))
            {
                   _privateCommands.Add(cmd.Command, cmd);
                   return true;
            }
            return false;
        }
        
        public bool RemovePrivateCommand(string command)
        {
            if(_privateCommands.ContainsKey(command))
            {
            	_privateCommands.Remove(command);
                   return true;
            }
            return false;
        }
        
        public bool AddPublicCommand(Commandlet cmd)
        {
            if(!_publicCommands.ContainsKey(cmd.Command))
            {
                _publicCommands.Add(cmd.Command, cmd);
                   return true;
            }
            return false;        }
        
        public bool RemovePublicCommand(string command)
        {
            if(_publicCommands.ContainsKey(command))
            {
                _publicCommands.Remove(command);
                   return true;
            }
            return false;
        }
        
        private void PrivateCommandDispatcher(object sender, IrcEventArgs e)
        {
            if (_privateCommands.ContainsKey(e.Data.MessageArray[0])) {
                if (_privateCommands[e.Data.MessageArray[0]].Handler != null) {
                    _privateCommands[e.Data.MessageArray[0]].Handler.Invoke(sender, e);
                } else {
                    foreach(AbstractPlugin complexPlug in complexPM.Plugins) {
                        Console.WriteLine(complexPlug.FullName + " == " + (string)_privateCommands[e.Data.MessageArray[0]].Owner);
                        if (complexPlug.FullName == (string)_privateCommands[e.Data.MessageArray[0]].Owner)
                        {
                            complexPlug.InvokeHandler(_privateCommands[e.Data.MessageArray[0]].HandlerName, e);
                        }
                    }
                }
                
            }
        }


        private void PublicCommandDispatcher(object sender, IrcEventArgs e)
        {
            if (_publicCommands.ContainsKey(e.Data.MessageArray[0])) {
                if (_publicCommands[e.Data.MessageArray[0]].Handler != null) {
                    _publicCommands[e.Data.MessageArray[0]].Handler.Invoke(sender, e);
                } else {
                    foreach(AbstractPlugin complexPlug in complexPM.Plugins) {
                        Console.WriteLine(complexPlug.FullName + " == " + (string)_publicCommands[e.Data.MessageArray[0]].Owner);
                        if (complexPlug.FullName == (string)_publicCommands[e.Data.MessageArray[0]].Owner)
                        {
                            complexPlug.InvokeHandler(_publicCommands[e.Data.MessageArray[0]].HandlerName, e);
                        }
                    }
                }
                
            }
        }
        
        private void PluginsCommand(object sender, IrcEventArgs e)
		{
			foreach(IPlugin p in simplePM.Plugins) {	
				SendMessage(SendType.Notice, e.Data.Channel, "Simple Plugin: "+IrcConstants.IrcBold+p.GetType().ToString()+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]");
			}
			foreach(AbstractPlugin p in complexPM.Plugins) {	
				SendMessage(SendType.Notice, e.Data.Channel, "Complex Plugin: "+IrcConstants.IrcBold+p.FullName+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]");
			}
		}
	
		private void ActivateCommand(object sender, IrcEventArgs e)
		{
			if (e.Data.MessageArray.Length < 2)
				return;
			foreach(IPlugin p in simplePM.Plugins) {
				if (e.Data.MessageArray[1]==p.GetType().ToString()) {
					PersistentMemory.SetValue("plugin", p.GetType().ToString());
					PersistentMemory.Flush();
					p.Activate();
					SendMessage(SendType.Notice, e.Data.Channel, "Simple Plugin: "+IrcConstants.IrcBold+p.GetType().ToString()+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]");
				}
			}
			foreach(AbstractPlugin p in complexPM.Plugins) {
			    if (e.Data.MessageArray[1]==p.FullName) {
					PersistentMemory.SetValue("plugin", p.FullName);
					PersistentMemory.Flush();
					p.Activate();
					SendMessage(SendType.Notice, e.Data.Channel, "Complex Plugin: "+IrcConstants.IrcBold+p.FullName+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]");
				}
			}
		}

		private void DeactivateCommand(object sender, IrcEventArgs e)
		{
			if (e.Data.MessageArray.Length < 2)
				return;
			foreach(IPlugin p in simplePM.Plugins) {
				if (e.Data.MessageArray[1]==p.GetType().ToString()) {
					PersistentMemory.RemoveValue("plugin", p.GetType().ToString());
					PersistentMemory.Flush();
					p.Deactivate();
					SendMessage(SendType.Notice, e.Data.Channel, "Simple Plugin: "+IrcConstants.IrcBold+p.GetType().ToString()+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]");
				}
			}
			foreach(AbstractPlugin p in complexPM.Plugins) {
			    if (e.Data.MessageArray[1]==p.FullName) {
					PersistentMemory.RemoveValue("plugin", p.FullName);
					PersistentMemory.Flush();
					p.Deactivate();
					SendMessage(SendType.Notice, e.Data.Channel, "Complex Plugin: "+IrcConstants.IrcBold+p.FullName+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]");
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
            if (e.Data.MessageArray.Length < 2) {
                this.SendMessage(SendType.Message, e.Data.Channel, "You can use the help command to get information about Plugins and Commands:");
                this.SendMessage(SendType.Message, e.Data.Channel, "Type !help commands (for commandlist), !plugins (for plugin list), !help <plugin> (for help about <plugin>), !help <command> (for help about <command>)");
            } else {
                sendHelp(e.Data.MessageArray[1], e.Data.Channel);
            }
        }
    
        private void sendHelp(string topic, string channel)
        {    
            topic = topic.ToLower();
            bool helped = false;
            
            if (topic == "test")
            {

            }
            
            if (topic == "commands") {
                List<string> commands = new List<string>();
                commands.AddRange(_publicCommands.Keys);
                foreach(string com in ListToLines(commands, 100, ", ", "Active Commands: ", null))
                {
                    this.SendMessage(SendType.Message, channel, com);
                }
                helped = true;
            }
            
            if(topic == "more") {
                foreach(KeyValuePair<string, Commandlet> cmd in _publicCommands)
                {
                    string owner = (cmd.Value.Handler==null)?(string)cmd.Value.Owner + "~":cmd.Value.Owner.GetType().ToString();
                    this.SendMessage(SendType.Message, channel, "Public Command:" + cmd.Value.Command + ", offered by " + owner + " and help provided: " + cmd.Value.HelpText);
                }
                
                foreach(KeyValuePair<string, Commandlet> cmd in _privateCommands)
                {
                    string owner = (cmd.Value.Handler==null)?(string)cmd.Value.Owner:cmd.Value.Owner.GetType().ToString();
                    this.SendMessage(SendType.Message, channel, "Private Command:" + cmd.Value.Command + ", offered by " + owner + " and help provided: " + cmd.Value.HelpText);
                }
                helped = true;                        
            }
            
            // maybe in some command?
            foreach(Commandlet cmd in _publicCommands.Values) {
                if ((cmd.Command==topic) || (cmd.Command.Substring(1)==topic) || (cmd.Command==topic.Substring(1))) {
                    this.SendMessage(SendType.Message, channel, cmd.HelpText);
                    helped = true;
                }
            }
            
            // maybe a plugin
            foreach(IPlugin p in simplePM.Plugins) {
                bool plugHelp = false;
                if (topic==p.GetType().ToString().ToLower()) {
                    plugHelp = true;    
                }
                foreach(string s in p.GetType().ToString().ToLower().Split(new char[] {'.'}))
                {
                    if((topic==s)&&(!helped)) {
                        plugHelp = true;
                    }
                        
                }
                if(plugHelp) {
                    this.SendMessage(SendType.Message, channel, p.AboutHelp());
                    helped = true;
                }
            }
            // TODO: same for complex Plugin
            
            if (!helped)
                this.SendMessage(SendType.Message, channel, "Your Helptopic was not found");
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
            //this.plugins.ShutDown();
            PersistentMemory.Flush();
            simplePM.ShutDown();
            complexPM.ShutDown();
            
            // we are done, lets exit...
            System.Console.WriteLine("Exiting...");
            System.Threading.Thread.Sleep(60000);
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
                Console.WriteLine("Connecting...");
                this.Connect(serverlist, port);
                Console.WriteLine("Connected.");
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
