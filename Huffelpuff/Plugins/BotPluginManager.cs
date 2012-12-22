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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Timers;
using apophis.SharpIRC;
using Huffelpuff.Utils;
using Timer = System.Timers.Timer;

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

            eventTimer = new Timer { Enabled = true, Interval = AbstractPlugin.MinTickIntervall * 1000 };
            eventTimer.Elapsed += EventTick;

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

        private readonly Dictionary<string, string> oldPlugs = new Dictionary<string, string>();

        public enum PluginLoadEventType
        {
            Failed,
            Update,
            Load,
            Reload,
            Remove
        }

        public class PluginLoadEventArgs : EventArgs
        {
            public PluginLoadEventArgs(string pluginName, PluginLoadEventType pluginLoadEventType, AbstractPlugin plugin)
            {
                PluginName = pluginName;
                EventType = pluginLoadEventType;
                Plugin = plugin;
            }

            public string PluginName { get; private set; }
            public PluginLoadEventType EventType { get; private set; }
            public AbstractPlugin Plugin { get; private set; }
        }

        public delegate void PluginEventHandler(object sender, PluginLoadEventArgs e);

        public event PluginEventHandler PluginLoadEvent;

        private void OnPluginLoadEvent(PluginLoadEventArgs e)
        {
            if (PluginLoadEvent != null)
                PluginLoadEvent(this, e);
        }

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
                    Log.Instance.Log(exception);
                    continue;
                }

                if (p.Ready)
                {
                    plugins.Add(p);
                }
                else
                {
                    OnPluginLoadEvent(new PluginLoadEventArgs(pluginName, PluginLoadEventType.Failed, null));
                }
            }

            foreach (var plugin in plugins)
            {
                var assemblyParts = GetAssemblyParts(plugin.AssemblyName);
                string assemblyVersion = assemblyParts.AssemblyVersion;
                string assemblyName = assemblyParts.AssemblyName;

                string newAssemblyVersion;
                PluginLoadEventType loadType;
                if (oldPlugs.TryGetValue(assemblyName, out newAssemblyVersion))
                {
                    loadType = assemblyVersion != newAssemblyVersion ? PluginLoadEventType.Update : PluginLoadEventType.Reload;
                    oldPlugs.Remove(assemblyName);
                }
                else
                {
                    loadType = PluginLoadEventType.Load;
                }
                OnPluginLoadEvent(new PluginLoadEventArgs(plugin.FullName, loadType, plugin));
            }

            foreach (var s in oldPlugs)
            {
                OnPluginLoadEvent(new PluginLoadEventArgs(s.Key, PluginLoadEventType.Remove, null));
            }

            oldPlugs.Clear();
            foreach (var assemblyParts in plugins.Select(plugin => GetAssemblyParts(plugin.AssemblyName)))
            {
                oldPlugs.Add(assemblyParts.AssemblyName, assemblyParts.AssemblyVersion);
            }


            foreach (var plugin in PersistentMemory.Instance.GetValues("plugin").SelectMany(pluginname => plugins.Where(p => pluginname == p.FullName)))
            {
                try
                {
                    plugin.Activate();
                }
                catch (Exception exception)
                {
                    Log.Instance.Log(exception);
                    continue;
                } 
            }
        }

        private class AssemblyParts
        {
            public string AssemblyName { get; set; }
            public string AssemblyVersion { get; set; }
        }

        private static AssemblyParts GetAssemblyParts(string fullAssemblyName)
        {
            var assemblyParts = fullAssemblyName.Split(new[] { ", " }, 4, StringSplitOptions.None);
            var assemblyName = assemblyParts[0];
            var assemblyVersion = assemblyParts[1];

            return new AssemblyParts { AssemblyName = assemblyName, AssemblyVersion = assemblyVersion };
        }
        private readonly Timer eventTimer;
        private readonly Stopwatch stopwatch = new Stopwatch();

        private void EventTick(object sender, ElapsedEventArgs args)
        {
            if (!bot.IsConnected)
                return;

            foreach (var plugin in Plugins)
            {
                try
                {
                    if (plugin.Active && plugin.ShallITick(AbstractPlugin.MinTickIntervall))
                    {
                        stopwatch.Restart();
                        plugin.OnTick();
                        Log.Instance.Log("Tick: " + plugin.FullName + " took " + stopwatch.ElapsedMilliseconds + "ms");
                    }
                }
                catch (Exception exception)
                {
                    Log.Instance.Log("Exception in Tick event of Plugin " + plugin.FullName, Level.Error, ConsoleColor.Red);
                    Log.Instance.Log(exception.Message, Level.Error, ConsoleColor.Red);
                }
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
