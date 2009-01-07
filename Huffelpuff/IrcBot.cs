/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 08.10.2008
 * Zeit: 01:50
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */

using System;
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

        private PluginManager nextGenPlugins;
        public IrcBot()
        {           
            this.Encoding = System.Text.Encoding.UTF8;
            this.SendDelay = 2000;
            this.PingInterval = 120;
            this.ActiveChannelSyncing = true;        
            this.OnRawMessage += new IrcEventHandler(RawMessageHandler);
            
            
            this.OnChannelMessage += new IrcEventHandler(PublicCommandDispatcher);
            this.OnQueryMessage +=  new IrcEventHandler(PrivateCommandDispatcher);
            
            this.AddPublicCommand(new Commandlet("!join", "HELP ON JOIN", JoinCommand, this));
            this.AddPublicCommand(new Commandlet("!part", "HELP ON PART", PartCommand, this));
            this.AddPublicCommand(new Commandlet("!quit", "HELP ON QUIT", QuitCommand, this));
            this.AddPublicCommand(new Commandlet("!help", "HELP ON HELP", HelpCommand, this));
                        
            this.CtcpVersion = "Huffelpuff Testing Bot: Based on SmartIRC4net 4.5.0svn + DCC";
            
            if (PersistentMemory.GetValue("external_ip")!=null)
                this.ExternalIpAdress = System.Net.IPAddress.Parse(PersistentMemory.GetValue("external_ip"));
            else 
                this.ExternalIpAdress = System.Net.IPAddress.Parse("127.0.0.1");
            
            // Plugin needs the Handlers from IRC we load the plugins after we set everything up
            //plugins = new PluginManager(this);
            //plugins.ActivatePlugins();
            
            //New Plugins
            nextGenPlugins = new PluginManager();    
            nextGenPlugins.PluginsReloaded += new EventHandler(Plugins_PluginsReloaded);
            nextGenPlugins.IgnoreErrors = true;
            nextGenPlugins.PluginSources =  PluginSourceEnum.Both;

            nextGenPlugins.Start();
            
            
            //Access Control
            acl = new AccessControlList();
        }

        
        private List<AbstractPlugin> newPlug = new List<AbstractPlugin>();
        private void Plugins_PluginsReloaded(object sender, EventArgs e)
        {
        
            foreach(string pluginName in nextGenPlugins.GetSubclasses("Huffelpuff.ComplexPlugins.AbstractPlugin"))
            {
                
                Console.Write(pluginName + " ... ");
                AbstractPlugin o = (AbstractPlugin)nextGenPlugins.CreateInstance(pluginName, BindingFlags.CreateInstance, new object[] {this});
                newPlug.Add(o);
                Console.Write("(" + o.Name + ") ");
                Console.Write("Created ... ");
                o.Init();
                Console.Write("Initialized ... ");                
                o.Activate();
                Console.WriteLine("Activated");                
            }            
            return;
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
                _privateCommands[e.Data.MessageArray[0]].Handler.Invoke(sender, e);
            }
        }


        private void PublicCommandDispatcher(object sender, IrcEventArgs e)
        {
            if (_publicCommands.ContainsKey(e.Data.MessageArray[0])) {
                _publicCommands[e.Data.MessageArray[0]].Handler.Invoke(sender, e);
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
                    this.SendMessage(SendType.Message, channel, "Public Command:" + cmd.Value.Command + ", offered by " + cmd.Value.Owner.GetType().ToString() + " and help provided: " + cmd.Value.HelpText);
                }
                
                foreach(KeyValuePair<string, Commandlet> cmd in _privateCommands)
                {
                    this.SendMessage(SendType.Message, channel, "Private Command:" + cmd.Value.Command + ", offered by " + cmd.Value.Owner.GetType().ToString() + " and help provided: " + cmd.Value.HelpText);
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
            
            // we are done, lets exit...
            System.Console.WriteLine("Exiting...");
            System.Threading.Thread.Sleep(60000);
            System.Environment.Exit(0);
        }
        
        
        
        public void Start()
        {
            Thread.CurrentThread.Name = "Main";
                        
            Console.WriteLine("Start...");

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
