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
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Threading;
using apophis.SharpIRC;
using apophis.SharpIRC.IrcFeatures;
using Huffelpuff.AccessControl;
using Huffelpuff.Plugins;
using Huffelpuff.Properties;
using Huffelpuff.Utils;
using Plugin.Database.Huffelpuff;

namespace Huffelpuff
{

    /// <summary>
    /// Description of IrcBot.
    /// </summary>
    [Serializable]
    public class IrcBot : IrcFeatures
    {
        protected BotPluginManager PlugManager { get; private set; }

        public AccessControlList Acl { get; private set; }

        private readonly Dictionary<string, Commandlet> commands = new Dictionary<string, Commandlet>(StringComparer.CurrentCultureIgnoreCase);
        private readonly Dictionary<string, Commandlet> exported = new Dictionary<string, Commandlet>(StringComparer.CurrentCultureIgnoreCase);

        private bool isSetup;

        private readonly Dictionary<CommandScope, string> scopeColor = new Dictionary<CommandScope, string>
                                                                           { 
            {CommandScope.Private, "" + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.LightRed} ,
            {CommandScope.Public, "" + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.Blue} ,
            {CommandScope.Both, "" + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.Purple} 
        };

        public Main MainBotData { get; private set; }

        private void SetupOnce()
        {
            if (isSetup) return;
            isSetup = true;

            MainBotData = new Main(DatabaseConnection.Create("Huffelpuff"));

            if (MainBotData.Channels.Count() == 0 && !Settings.Default.Channel.IsNullOrEmpty())
            {
                foreach (var channelName in Settings.Default.Channel.Split(','))
                {
                    var channel = new Channel { ChannelName = channelName };
                    MainBotData.Channels.InsertOnSubmit(channel);
                }
            }
            MainBotData.SubmitChanges();

            Encoding = System.Text.Encoding.UTF8;
            SendDelay = 3000;
            PingInterval = 120;
            ActiveChannelSyncing = true;
            OnRawMessage += RawMessageHandler;
            AutoRejoin = true;
            AutoRetry = true;
            AutoRetryDelay = 5;
            SupportNonRfc = true;
            OnChannelMessage += CommandDispatcher;
            OnQueryMessage += CommandDispatcher;
            OnConnected += OnBotConnected;

            CtcpVersion = VersionString + " (SharpIRC " + Assembly.GetAssembly(typeof(IrcFeatures)).GetName(false).Version + ")";
            CtcpUrl = "https://github.com/FreeApophis/huffelpuff-irc-bot";
            CtcpSource = "https://github.com/FreeApophis/huffelpuff-irc-bot";

            // DCC Setup

            /* todo: find NAT / or our IP / sharp-IRC should be able to do that -> implement there */

            //Setting up Access Control
            Acl = new AccessControlList(this);
            Acl.Init();

            // Add Identify Plugin suitable (or all)
            Acl.AddIdentifyPlugin(new NickServIdentify(this));
            Acl.AddIdentifyPlugin(new HostIdentify(this));
            Acl.AddIdentifyPlugin(new PasswordIdentify(this));
            Acl.AddIdentifyPlugin(new NickIdentify(this));

            // Plugin needs the Handlers from IRC we load the plugins after we set everything up
            PlugManager = new BotPluginManager(this, "plugins");
            PlugManager.PluginLoadEvent += PlugManagerPluginLoadEvent;
            PlugManager.StartUp();

            //Basic Commands
            AddCommand(new Commandlet("!join", "The command !join <channel> lets the bot join Channel <channel>", JoinCommand, this, CommandScope.Both, "engine_join"));
            AddCommand(new Commandlet("!part", "The command !part <channel> lets the bot part Channel <channel>", PartCommand, this, CommandScope.Both, "engine_part"));
            AddCommand(new Commandlet("!channels", "The command !channels lists all channels where the bot resides", ListChannelCommand, this));
            AddCommand(new Commandlet("!quit", "The command !quit lets the bot quit himself", QuitCommand, this, CommandScope.Both, "engine_quit"));

            //Plugin Commands
            AddCommand(new Commandlet("!plugins", "The command !plugins lists all the plugins", PluginsCommand, this));
            AddCommand(new Commandlet("!activate", "The command !activate <plugin> activates the Plugin <plugin>", ActivateCommand, this, CommandScope.Both, "engine_activate"));
            AddCommand(new Commandlet("!deactivate", "The command !deactivate <plugin> deactivates the Plugin <plugin>", DeactivateCommand, this, CommandScope.Both, "engine_deactivate"));

            //Helper Commands (!commands)
            AddCommand(new Commandlet("!help", "The command !help <topic> gives you help about <topic> (special topics: commands, more)", HelpCommand, this));

            //Settings Commands
            new SettingCommands(this);

        }

        static void PlugManagerPluginLoadEvent(object sender, BotPluginManager.PluginLoadEventArgs e)
        {
            switch (e.EventType)
            {
                case BotPluginManager.PluginLoadEventType.Failed:
                    Log.Instance.Log(" [FAILED] " + e.PluginName + " (Init failed)", Level.Info, ConsoleColor.Red);
                    break;
                case BotPluginManager.PluginLoadEventType.Reload:
                    Log.Instance.Log(" [RELOAD] " + e.PluginName, Level.Info, ConsoleColor.DarkGreen);
                    break;
                case BotPluginManager.PluginLoadEventType.Update:
                    Log.Instance.Log(" [UPDATE] " + e.PluginName, Level.Info, ConsoleColor.DarkGreen);
                    break;
                case BotPluginManager.PluginLoadEventType.Load:
                    Log.Instance.Log("  [LOAD]  " + e.PluginName, Level.Info, ConsoleColor.Green);
                    break;
                case BotPluginManager.PluginLoadEventType.Remove:
                    Log.Instance.Log(" [REMOVE] " + e.PluginName, Level.Info, ConsoleColor.Red);
                    break;
            }
        }

        void OnBotConnected(object sender, EventArgs e)
        {
            if (!Settings.Default.NickServPassword.IsNullOrEmpty())
            {
                SendMessage(SendType.Message, "nickserv", "identify {0}".Fill(Settings.Default.NickServPassword), Priority.Critical);
            }
        }


        /// <summary>
        /// This command makes sure that no command is bound to an unloaded appdomain.
        /// If there still are commands which are bound the wrong appdomain, these will be unbound and reported as unclean.
        /// Each Plugin must remove all commands from the commands list on deactivate!
        /// </summary>
        internal void CleanPlugins()
        {
            var toRemove = new List<string>();

            foreach (var commandlet in commands)
            {
                try
                {
                    if (commandlet.Value.Handler == null)
                    {
                        toRemove.Add(commandlet.Key);
                    }
                }
                catch (AppDomainUnloadedException)
                {
                    Log.Instance.Log("Plugin with command '{0}' was not cleaned up".Fill(commandlet.Key), Level.Warning);

                    if (!toRemove.Contains(commandlet.Key))
                    {
                        toRemove.Add(commandlet.Key);
                    }
                }
            }

            foreach (var command in toRemove)
            {
                commands.Remove(command);
            }

            exported.Clear();
        }

        private static void RawMessageHandler(object sender, IrcEventArgs e)
        {
            Log.Instance.Log(e.Data.RawMessage, Level.Trace);
        }


        /// <summary>
        /// New API: obsoletes Public and Private Command, Commandlet has changed too
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool AddCommand(Commandlet cmd)
        {
            if (cmd.AccessString != null && Acl != null)
            {
                Acl.AddAccessString(cmd.AccessString);
            }
            if (!commands.ContainsKey(cmd.Command))
            {
                commands.Add(cmd.Command, cmd);
                return true;
            }
            return false;
        }

        public void AddExportedCommand(Commandlet cmd)
        {
            exported.Add(cmd.Command, cmd);
        }

        public void CallExportedCommand(string command, BotPluginManager pluginManager = null, object sender = null, string parameters = null)
        {
            var botPluginManager = pluginManager ?? PlugManager;

            if (!exported.ContainsKey(command)) { return; }

            IrcEventArgs e = null;

            if (exported[command].Handler != null)
            {
                exported[command].Handler.Invoke(sender, e);
            }
            else
            {
                foreach (var plug in botPluginManager.Plugins.Where(plug => plug.FullName == (string)exported[command].Owner))
                {
                    plug.InvokeHandler(exported[command].HandlerName, e);
                    return;
                }
            }
        }

        /// <summary>
        /// New API: obsoletes Public and Private Command, Commandlet has changed too
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool RemoveCommand(string command)
        {
            if (commands.ContainsKey(command))
            {
                commands.Remove(command);
                return true;
            }
            return false;
        }

        public bool RemoveCommand(Commandlet cmd)
        {
            return RemoveCommand(cmd.Command);
        }

        private void CommandDispatcher(object sender, IrcEventArgs e)
        {
            var pub = !string.IsNullOrEmpty(e.Data.Channel);

            if (!commands.ContainsKey(e.Data.MessageArray[0]))
                return;

            if (pub && (commands[e.Data.MessageArray[0]].Scope == CommandScope.Private))
            {
                SendMessage(SendType.Message, e.Data.Nick, "This command (" + commands[e.Data.MessageArray[0]].Command + ") can only be invoked privatly in a query.");
                return;
            }
            if ((!pub) && (commands[e.Data.MessageArray[0]].Scope == CommandScope.Public))
            {
                SendMessage(SendType.Message, e.Data.Nick, "This command (" + commands[e.Data.MessageArray[0]].Command + ") can only be invoked publicly in a channel.");
                return;
            }

            // check if access to function is allowed
            if (!string.IsNullOrEmpty(commands[e.Data.MessageArray[0]].AccessString) && !Acl.Access(e.Data.Nick, commands[e.Data.MessageArray[0]].AccessString, true))
                return;

            if ((commands[e.Data.MessageArray[0]].ChannelList != null) && (!commands[e.Data.MessageArray[0]].ChannelList.Contains(e.Data.Channel)))
                return;

            if (commands[e.Data.MessageArray[0]].Handler != null)
            {
                commands[e.Data.MessageArray[0]].Handler.Invoke(sender, e);
            }
            else
            {
                foreach (var plug in PlugManager.Plugins.Where(plug => plug.FullName == (string)commands[e.Data.MessageArray[0]].Owner))
                {
                    plug.InvokeHandler(commands[e.Data.MessageArray[0]].HandlerName, e);
                    return;
                }
            }
        }

        private void PluginsCommand(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            var pluginsList = PlugManager.Plugins.Select(p => IrcConstants.IrcBold + p.FullName + " [" + ((p.Active ? IrcConstants.IrcColor + "" + (int)IrcColors.LightGreen + "ON" : IrcConstants.IrcColor + "" + (int)IrcColors.LightRed + "OFF")) + IrcConstants.IrcColor + "]" + IrcConstants.IrcBold);

            foreach (var line in pluginsList.ToLines(300, ", ", "Plugins Loaded: ", " END."))
            {
                SendMessage(SendType.Message, sendto, line);
            }
        }

        private void ActivateCommand(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            if (e.Data.MessageArray.Length < 2)
            {
                SendMessage(SendType.Message, sendto, "Too few arguments! Try !help.");
                return;
            }

            var calledPlugins = new List<AbstractPlugin>();
            foreach (var plugin in e.Data.MessageArray.Skip(1).Select(p => new Wildcard(p)).SelectMany(wildcard => PlugManager.Plugins.Where(p => wildcard.IsMatch(p.FullName) || wildcard.IsMatch(p.MainClass))))
            {
                if (!plugin.Active)
                {
                    var plug = new Plugin.Database.Huffelpuff.Plugin { PluginName = plugin.FullName };
                    MainBotData.Plugin.InsertOnSubmit(plug);
                    MainBotData.SubmitChanges();

                    plugin.Activate();
                }
                calledPlugins.Add(plugin);
            }
            foreach (var line in calledPlugins.Select(plugin => "" + plugin.FullName + " [" + ((plugin.Active ? IrcConstants.IrcColor + "" + (int)IrcColors.LightGreen + "ON" : IrcConstants.IrcColor + "" + (int)IrcColors.LightRed + "OFF")) + IrcConstants.IrcColor + "]").ToLines(350, ", ", "Plugins: ", " END."))
            {
                SendMessage(SendType.Message, sendto, line);
            }
        }

        private void DeactivateCommand(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            if (e.Data.MessageArray.Length < 2)
            {
                SendMessage(SendType.Message, sendto, "Too few arguments! Try !help.");
                return;
            }
            var calledPlugins = new List<AbstractPlugin>();
            foreach (AbstractPlugin plugin in e.Data.MessageArray.Skip(1).Select(p => new Wildcard(p)).SelectMany(wildcard => PlugManager.Plugins.Where(p => wildcard.IsMatch(p.FullName) || wildcard.IsMatch(p.MainClass))))
            {
                if (plugin.Active)
                {
                    var plugs = MainBotData.Plugin.Where(p => p.PluginName == plugin.FullName).ToArray();
                    MainBotData.Plugin.DeleteAllOnSubmit(plugs);

                    plugin.Deactivate();

                    var plug = plugin;
                    foreach (var command in (from commandlet in commands
                                             let abstractPlugin = commandlet.Value.Owner as string
                                             let command = commandlet.Key
                                             where abstractPlugin != null && abstractPlugin == plug.FullName
                                             select command).ToList())
                    {
                        commands.Remove(command);
                        Log.Instance.Log("BUG in Plugin: Forcefully deactivated Command '{0}' in Plugin {1}.".Fill(command, plugin.FullName), Level.Warning);
                    }


                }
                calledPlugins.Add(plugin);
            }
            MainBotData.SubmitChanges();

            foreach (var line in calledPlugins.Select(plugin => "" + plugin.FullName + " [" + ((plugin.Active ? IrcConstants.IrcColor + "" + (int)IrcColors.LightGreen + "ON" : IrcConstants.IrcColor + "" + (int)IrcColors.LightRed + "OFF")) + IrcConstants.IrcColor + "]").ToLines(350, ", ", "Plugins: ", " END."))
            {
                SendMessage(SendType.Message, sendto, line);
            }
        }

        private void JoinCommand(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            if (e.Data.MessageArray.Length < 2)
            {
                SendMessage(SendType.Message, sendto, "Too few arguments! Try !help.");
                return;
            }

            foreach (var channel in e.Data.MessageArray.Skip(1).Where(channel => !channel.IsNullOrEmpty()))
            {
                RfcJoin(channel);
                var chan = new Channel { ChannelName = channel };
                MainBotData.Channels.InsertOnSubmit(chan);
                MainBotData.SubmitChanges();
            }
            Settings.Default.Save();
        }

        private void ListChannelCommand(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            foreach (var line in Settings.Default.Channel.Cast<string>().ToLines(350, ", ", "I am in the following channels: ", " END."))
            {
                SendMessage(SendType.Message, sendto, line);
            }
        }

        private void PartCommand(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            if (e.Data.MessageArray.Length < 2)
            {
                SendMessage(SendType.Message, sendto, "Too few arguments! Try !help.");
                return;
            }

            RfcPart(e.Data.Message.Substring(6));


            var channel = MainBotData.Channels.Where(c => c.ChannelName == e.Data.MessageArray[1]).FirstOrDefault();
            if (channel != null)
            {
                MainBotData.Channels.DeleteOnSubmit(channel);
                MainBotData.SubmitChanges();
            }
        }

        private void QuitCommand(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length > 1)
            {
                RfcQuit(e.Data.Message.Substring(6), Priority.Low);
            }
            else
            {
                RfcQuit(Priority.Low);
            }
        }

        private void HelpCommand(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            if (e.Data.MessageArray.Length < 2)
            {
                SendMessage(SendType.Message, sendto, "You can use the help command to get information about Plugins and Commands:");
                SendMessage(SendType.Message, sendto, "Type !help commands (for commandlist), !plugins (for plugin list), !help <plugin> (for help about <plugin>), !help <command> (for help about <command>)");
            }
            else
            {
                SendHelp(e.Data.MessageArray[1], sendto, e.Data.Nick);
            }
        }

        private void SendHelp(string topic, string sendto, string nick)
        {
            topic = topic.ToLower();
            bool helped = false;

            // List all commands
            if (topic == "commands")
            {
                SendCommandList(sendto, nick, commands.Values);
                helped = true;
            }

            // debug list of currently loaded commands
            if (topic == "more")
            {
                foreach (var commandlet in commands.Values)
                {
                    var owner = (commandlet.Handler == null) ? (string)commandlet.Owner + "~" : commandlet.Owner.GetType().ToString();

                    SendMessage(SendType.Message, sendto, "Command (Scope:" + commandlet.Scope + "):" + commandlet.Command + ", offered by " + commandlet.SourcePlugin + "(" + owner + ")" + " and help provided: " + commandlet.HelpText + ((string.IsNullOrEmpty(commandlet.AccessString)) ? "" : " (accessString=" + commandlet.AccessString + ")"));
                }
                helped = true;
            }

            // maybe in some command?
            foreach (var cmd in commands.Values.Where(cmd => (cmd.Command == topic) || (cmd.Command.Substring(1) == topic) || (cmd.Command == topic.Substring(1))))
            {
                SendMessage(SendType.Message, sendto, cmd.HelpText + ((string.IsNullOrEmpty(cmd.AccessString)) ? "" : " (access restricted)"));
                helped = true;
            }

            // maybe a  plugin
            foreach (AbstractPlugin p in PlugManager.Plugins)
            {
                bool plugHelp = false;
                if (topic == p.FullName.ToLower())
                {
                    plugHelp = true;
                }
                foreach (string s in p.FullName.ToLower().Split(new[] { '.' }))
                {
                    if ((topic == s) && (!helped))
                    {
                        plugHelp = true;
                    }

                }
                if (plugHelp)
                {
                    SendMessage(SendType.Message, sendto, p.AboutHelp());
                    var pluginCommands = commands.Where(c => c.Value.SourcePlugin == p.FullName).Select(c => c.Value);
                    SendCommandList(sendto, nick, pluginCommands);
                    helped = true;
                    if (!pluginCommands.Any())
                    {
                        SendMessage(SendType.Message, sendto, p.Active ? "This plugin has no !commands registered." : "This plugin is currently deactivated.");
                    }
                }
            }


            if (!helped)
            {
                SendMessage(SendType.Message, sendto, "Your Helptopic was not found");
            }
        }

        private void SendCommandList(string sendto, string nick, IEnumerable<Commandlet> commandList)
        {
            if (!commandList.Any()) return;

            var commandStrings = new List<string>();
            foreach (var commandlet in commandList)
            {
                if (string.IsNullOrEmpty(commandlet.AccessString))
                {
                    commandStrings.Add(scopeColor[commandlet.Scope] + commandlet.Command + IrcConstants.IrcColor + IrcConstants.IrcBold);
                }
                else if (Acl.Access(nick, commandlet.AccessString, false))
                {
                    commandStrings.Add(scopeColor[commandlet.Scope] + "<" + commandlet.Command + ">" + IrcConstants.IrcColor + IrcConstants.IrcBold);
                }
            }

            foreach (string com in commandStrings.ToLines(350, ", ", "Active Commands (" + scopeColor[CommandScope.Public] + "public" + IrcConstants.IrcColor + IrcConstants.IrcBold + ", " + scopeColor[CommandScope.Private] + "private" + IrcConstants.IrcColor + IrcConstants.IrcBold + ") <restricted>: ", null))
            {
                SendMessage(SendType.Message, sendto, com);
            }
        }

        public bool Start()
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "Main";
            }

            SetupOnce();

            if (!Settings.Default.ProxyServer.IsNullOrEmpty())
            {
                Log.Instance.Log("Using Proxy Server: " + Settings.Default.ProxyServer, Level.Trace);
                ProxyType = apophis.SharpIRC.IrcConnection.ProxyType.Socks5;
                ProxyHost = Settings.Default.ProxyServer;
                ProxyPort = Settings.Default.ProxyPort;
                ProxyUsername = Settings.Default.ProxyUser;
                ProxyPassword = Settings.Default.ProxyPass;
            }

            // the server we want to connect to
            string[] serverlist = Settings.Default.Server.Split(',');
            int port = Settings.Default.Port;
            try
            {
                // here we try to connect to the server and exceptions get handled
                Connect(serverlist, port);
                Log.Instance.Log("successfull connected", Level.Info);
            }
            catch (ConnectionException exception)
            {
                // something went wrong, the reason will be shown
                Log.Instance.Log(exception);

                return true;
            }

            try
            {
                // here we logon and register our nickname and so on
                Login(Settings.Default.Nick, Settings.Default.Realname, 4, Settings.Default.Username, Settings.Default.ServerPass);

                // join the channels
                foreach (var channel in MainBotData.Channels.Select(c => c.ChannelName))
                {
                    RfcJoin(channel);
                }

                Listen();
                Disconnect();
            }
            catch (ConnectionException)
            {
                return true;
            }
            catch (Exception exception)
            {
                // this should not happen by just in case we handle it nicely
                Log.Instance.Log(exception);
                return true;
            }
            return true;
        }
    }
}
