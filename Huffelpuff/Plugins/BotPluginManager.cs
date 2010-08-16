/*
 *  The Huffelpuff Irc Bot, versatile pluggable bot for IRC chats
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
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

using System.Linq;
using System.Threading;
using Huffelpuff.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using Meebey.SmartIrc4net;

namespace Huffelpuff.Plugins
{
    /// <summary>
    /// Description of BotPluginManager (was ComplexPluginManager).
    /// </summary>
    public class BotPluginManager
    {
        private readonly PluginManager pluginManager;
        private readonly IrcBot bot;
        private readonly List<AbstractPlugin> plugins = new List<AbstractPlugin>();
        public List<AbstractPlugin> Plugins
        {
            get
            {
                return plugins;
            }
        }

        public BotPluginManager(IrcBot bot, string relPluginPath)
        {
            this.bot = bot;
            this.bot.AddCommand(new Commandlet("!reload", "!reload unloads and reloads all the plugins", ReloadPlugins, this, CommandScope.Both, "pluginmanager_reload"));

            pluginManager = new PluginManager(relPluginPath);
            pluginManager.PluginsReloaded += PluginsPluginsReloaded;
            pluginManager.IgnoreErrors = false;
            pluginManager.PluginSources = PluginSourceEnum.Both;

            pluginManager.AddReference(Assembly.GetEntryAssembly().Location);

            pluginManager.Start();
        }

        private void ReloadPlugins(object sender, IrcEventArgs e)
        {
            pluginManager.ReloadPlugins();

            // The following call prevents the OnChannelMessage/OnQueryMessage Event to be processed further, 
            // cause at this points its not guaranteed that the other Event Handlers still exists. 
            // Because all objects out of this AppDomain are destroyed now. However we are still in the 
            // Event Handling. We stop that here. 
            // This means that "!reload" gets special treating and cannot be used for anything else.
            Thread.CurrentThread.Abort();
        }



        private readonly List<string> oldPlugs = new List<string>();
        private void PluginsPluginsReloaded(object sender, EventArgs e)
        {
            plugins.Clear();
            bot.CleanPlugins();

            foreach (var pluginName in pluginManager.GetSubclasses("Huffelpuff.Plugins.AbstractPlugin"))
            {
                var p = (AbstractPlugin)pluginManager.CreateInstance(pluginName, BindingFlags.CreateInstance, new object[] { bot });
                try
                {
                    p.Init();
                }
                catch (Exception exception)
                {
                    Log.Instance.Log(" [Exception] " + pluginName + " (Exception: " + exception.Message + ")", Level.Info, ConsoleColor.Red);
                    continue;
                }

                if (p.Ready)
                {
                    plugins.Add(p);
                }
                else
                {
                    Log.Instance.Log(" [FAILED] " + pluginName + " (Init failed)", Level.Info, ConsoleColor.Red);
                }
            }

            foreach (var plugin in plugins)
            {
                if (oldPlugs.Contains(plugin.AssemblyName))
                {
                    Log.Instance.Log(" [RELOAD] " + plugin.FullName, Level.Info, ConsoleColor.DarkGreen);
                    oldPlugs.Remove(plugin.AssemblyName);
                }
                else
                {
                    Log.Instance.Log("  [LOAD]  " + plugin.FullName, Level.Info, ConsoleColor.Green);
                }
            }

            foreach (var s in oldPlugs)
            {
                Log.Instance.Log(" [REMOVE] " + s, Level.Info, ConsoleColor.Red);
            }

            oldPlugs.Clear();
            foreach (var plugin in plugins)
            {
                oldPlugs.Add(plugin.AssemblyName);
            }


            foreach (var plugin in PersistentMemory.Instance.GetValues("plugin").SelectMany(pluginname => plugins.Where(p => pluginname == p.FullName)))
            {
                plugin.Activate();
            }
        }

        public void ShutDown()
        {
            foreach (var p in plugins)
            {
                try
                {
                    p.Deactivate();
                    p.DeInit();
                }
                catch (Exception exception)
                {
                    Log.Instance.Log(exception.Message, Level.Info, ConsoleColor.Red);
                }
            }
        }
    }
}
