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
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Huffelpuff.AccessControl;
using Huffelpuff.Plugins;
using Huffelpuff.Utils;
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

        public AccessControlList Acl { get; private set; }

        private readonly Dictionary<string, Commandlet> commands = new Dictionary<string, Commandlet>(StringComparer.CurrentCultureIgnoreCase);

        //public bool UPNPSupport { get; private set; }

        public const string Channelconst = "channel";

        private bool isSetup;

        private void SetupOnce()
        {
            if (isSetup) return;
            isSetup = true;

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

            CtcpVersion = VersionString + " (SmartIrc4Net " + Assembly.GetAssembly(typeof(IrcFeatures)).GetName(false).Version + ")";
            CtcpUrl = "http://huffelpuff-irc-bot.origo.ethz.ch/";
            CtcpSource = "https://svn.origo.ethz.ch/huffelpuff-irc-bot/";

            // DCC Setup

            /* todo: find NAT / or our IP */

            //Setting up Access Control
            Acl = new AccessControlList(this);
            Acl.Init();

            // Add Identify Plugin suitable (or all)
            Acl.AddIdentifyPlugin(new NickServIdentify(this));
            Acl.AddIdentifyPlugin(new HostIdentify(this));
            Acl.AddIdentifyPlugin(new PasswordIdentify(this));
            Acl.AddIdentifyPlugin(new NickIdentify(this));

            // Plugin needs the Handlers from IRC we load the plugins after we set everything up
            plugManager = new BotPluginManager(this, "plugins");

            //Basic Commands
            AddCommand(new Commandlet("!join", "The command !join <channel> lets the bot join Channel <channel>", JoinCommand, this, CommandScope.Both, "engine_join"));
            AddCommand(new Commandlet("!part", "The command !part <channel> lets the bot part Channel <channel>", PartCommand, this, CommandScope.Both, "engine_part"));
            AddCommand(new Commandlet("!channels", "The command !channels lists all channels where the bot resides", ListChannelCommand, this, CommandScope.Both));
            AddCommand(new Commandlet("!quit", "The command !quit lets the bot quit himself", QuitCommand, this, CommandScope.Both, "engine_quit"));

            //Plugin Commands
            AddCommand(new Commandlet("!plugins", "The command !plugins lists all the plugins", PluginsCommand, this, CommandScope.Both));
            AddCommand(new Commandlet("!activate", "The command !activate <plugin> activates the Plugin <plugin>", ActivateCommand, this, CommandScope.Both, "engine_activate"));
            AddCommand(new Commandlet("!deactivate", "The command !deactivate <plugin> deactivates the Plugin <plugin>", DeactivateCommand, this, CommandScope.Both, "engine_deactivate"));

            //Helper Commands (!commands)
            AddCommand(new Commandlet("!help", "The command !help <topic> gives you help about <topic> (special topics: commands, more)", HelpCommand, this, CommandScope.Both));

            //Settings Commands
            new SettingCommands(this);

        }

        void OnBotConnected(object sender, EventArgs e)
        {
            if (PersistentMemory.Instance.GetValue("nickserv") != null)
            {
                SendMessage(SendType.Message, "nickserv", "identify {0}".Fill(PersistentMemory.Instance.GetValue("nickserv")), Priority.Critical);
            }
        }


        internal void CleanPlugins()
        {
            // try catch AppDomainUnloadedExceptions (somehow)
            foreach (var s in (from p in commands where p.Value.Handler == null select p.Key).ToList())
            {
                commands.Remove(s);
            }
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
            if (cmd.AccessString != null)
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

        private void CommandDispatcher(object sender, IrcEventArgs e)
        {
            var pub = !string.IsNullOrEmpty(e.Data.Channel);

            if (!commands.ContainsKey(e.Data.MessageArray[0]))
                return;

            if ((pub && (commands[e.Data.MessageArray[0]].Scope == CommandScope.Private)) || ((!pub) && (commands[e.Data.MessageArray[0]].Scope == CommandScope.Public)))
                return;

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
                foreach (var plug in plugManager.Plugins.Where(plug => plug.FullName == (string)commands[e.Data.MessageArray[0]].Owner))
                {
                    plug.InvokeHandler(commands[e.Data.MessageArray[0]].HandlerName, e);
                    return;
                }
            }
        }

        private void PluginsCommand(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            var pluginsList = plugManager.Plugins.Select(p => IrcConstants.IrcBold + p.FullName + " [" + ((p.Active ? IrcConstants.IrcColor + "" + (int)IrcColors.LightGreen + "ON" : IrcConstants.IrcColor + "" + (int)IrcColors.LightRed + "OFF")) + IrcConstants.IrcColor + "]" + IrcConstants.IrcBold);

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
            foreach (var plugin in e.Data.MessageArray.Skip(1).Select(p => new Wildcard(p)).SelectMany(wildcard => plugManager.Plugins.Where(p => wildcard.IsMatch(p.FullName) || wildcard.IsMatch(p.MainClass))))
            {
                if (!plugin.Active)
                {
                    PersistentMemory.Instance.SetValue("plugin", plugin.FullName);
                    PersistentMemory.Instance.Flush();
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
            foreach (AbstractPlugin plugin in e.Data.MessageArray.Skip(1).Select(p => new Wildcard(p)).SelectMany(wildcard => plugManager.Plugins.Where(p => wildcard.IsMatch(p.FullName) || wildcard.IsMatch(p.MainClass))))
            {
                if (plugin.Active)
                {
                    PersistentMemory.Instance.RemoveValue("plugin", plugin.FullName);
                    PersistentMemory.Instance.Flush();
                    plugin.Deactivate();
                }
                calledPlugins.Add(plugin);
            }

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
                PersistentMemory.Instance.ReplaceValue("channel", channel);
            }

            PersistentMemory.Instance.Flush();
        }

        private void ListChannelCommand(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            foreach (var line in PersistentMemory.Instance.GetValues(Channelconst).ToLines(350, ", ", "I am in the following channels: ", " END."))
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

            PersistentMemory.Instance.RemoveValue("channel", e.Data.MessageArray[1]);
            PersistentMemory.Instance.Flush();
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
            var scopeColor = new Dictionary<CommandScope, string>();
            scopeColor.Add(CommandScope.Private, ("" + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.LightRed));
            scopeColor.Add(CommandScope.Public, ("" + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.Blue));
            scopeColor.Add(CommandScope.Both, ("" + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.Purple));

            topic = topic.ToLower();
            bool helped = false;

            // List all commands
            if (topic == "commands")
            {
                var commandlist = new List<string>();
                foreach (var commandlet in commands.Values)
                {
                    if (string.IsNullOrEmpty(commandlet.AccessString))
                    {
                        commandlist.Add(scopeColor[commandlet.Scope] + commandlet.Command + IrcConstants.IrcColor + IrcConstants.IrcBold);
                    }
                    else if (Acl.Access(nick, commandlet.AccessString, false))
                    {
                        commandlist.Add(scopeColor[commandlet.Scope] + "<" + commandlet.Command + ">" + IrcConstants.IrcColor + IrcConstants.IrcBold);
                        //commandlist.Add("" + IrcConstants.IrcColor + (int)IrcColors.Green + "<" + scopeColor[cmd.Scope] + cmd.Command + IrcConstants.IrcColor + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.Green +  ">" + IrcConstants.IrcColor);
                    }
                    //else
                    //{
                    //    commandlist.Add("<" + scopeColor[cmd.Scope] + cmd.Command + IrcConstants.IrcColor + IrcConstants.IrcBold + ">");
                    //}
                }

                foreach (string com in commandlist.ToLines(350, ", ", "Active Commands (" + scopeColor[CommandScope.Public] + "public" + IrcConstants.IrcColor + IrcConstants.IrcBold + ", " + scopeColor[CommandScope.Private] + "private" + IrcConstants.IrcColor + IrcConstants.IrcBold + ") <restricted>: ", null))
                {
                    SendMessage(SendType.Message, sendto, com);
                }
                helped = true;
            }

            // debug list of currently loaded commands
            if (topic == "more")
            {
                foreach (var commandlet in commands.Values)
                {
                    var owner = (commandlet.Handler == null) ? (string)commandlet.Owner + "~" : commandlet.Owner.GetType().ToString();

                    SendMessage(SendType.Message, sendto, "Command (Scope:" + commandlet.Scope + "):" + commandlet.Command + ", offered by " + owner + " and help provided: " + commandlet.HelpText + ((string.IsNullOrEmpty(commandlet.AccessString)) ? "" : " (accessString=" + commandlet.AccessString + ")"));
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
            foreach (AbstractPlugin p in plugManager.Plugins)
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
                    helped = true;
                }
            }


            if (!helped)
                SendMessage(SendType.Message, sendto, "Your Helptopic was not found");
        }

        public void Exit()
        {
            PersistentMemory.Instance.Flush();

            // we are done, lets exit...
            Log.Instance.Log("Exiting...");
#if DEBUG
            Thread.Sleep(60000);
#endif

            Environment.Exit(0);
        }



        public void Start()
        {
            Thread.CurrentThread.Name = "Main";

            SetupOnce();

            if (PersistentMemory.Instance.GetValue("ProxyServer") != null)
            {
                Log.Instance.Log("Using Proxy Server: " + PersistentMemory.Instance.GetValue("ProxyServer"));
                ProxyType = Org.Mentalis.Network.ProxySocket.ProxyTypes.Socks5;
                var ip = IPAddress.Parse(PersistentMemory.Instance.GetValue("ProxyServer").Split(new[] { ':' })[0]);
                if (ip != null)
                {
                    ProxyEndPoint = new IPEndPoint(ip, int.Parse(PersistentMemory.Instance.GetValue("ProxyServer").Split(new[] { ':' })[1]));
                }
                ProxyUser = PersistentMemory.Instance.GetValue("ProxyUser");
                ProxyPass = PersistentMemory.Instance.GetValue("ProxyPass");
            }

            // the server we want to connect to
            string[] serverlist = PersistentMemory.Instance.GetValuesOrTodo("ServerHost").ToArray();
            int port = int.Parse(PersistentMemory.Instance.GetValueOrTodo("ServerPort"));
            try
            {
                // here we try to connect to the server and exceptions get handled
                Connect(serverlist, port);
                Log.Instance.Log("successfull connected");
            }
            catch (ConnectionException e)
            {
                // something went wrong, the reason will be shown
                Log.Instance.Log("couldn't connect! Reason: " + e.Message);
                Exit();
            }

            try
            {
                // here we logon and register our nickname and so on
                Login(PersistentMemory.Instance.GetValueOrTodo("nick"), PersistentMemory.Instance.GetValueOrTodo("realname"), 4, PersistentMemory.Instance.GetValueOrTodo("username"), PersistentMemory.Instance.GetValue("serverpass"));

                // join the channels
                foreach (var channel in PersistentMemory.Instance.GetValues(Channelconst))
                {
                    RfcJoin(channel);
                }

                Listen();
                Disconnect();
            }
            catch (ConnectionException)
            {
                Exit();
            }
            catch (Exception e)
            {
                // this should not happen by just in case we handle it nicely
                Log.Instance.Log("Error occurred! Message: " + e.Message, Level.Error);
                Log.Instance.Log("Exception: " + e.StackTrace, Level.Error);
                Exit();
            }
        }
    }
}
