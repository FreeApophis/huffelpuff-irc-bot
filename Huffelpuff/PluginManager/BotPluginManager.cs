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

using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Huffelpuff.Plugins
{
    /// <summary>
    /// Description of BotPluginManager (was ComplexPluginManager).
    /// </summary>
    public class BotPluginManager
    {
        private PluginManager pluginManager;
        private IrcBot bot;
        private List<AbstractPlugin> plugins = new List<AbstractPlugin>();
        public List<AbstractPlugin> Plugins {
            get {
                return plugins;
            }
        }
        
        public BotPluginManager(IrcBot bot, string relPluginPath)
        {
            this.bot = bot;
            this.bot.AddCommand(new Commandlet("!reload", "!reload unloads and reloads all the plugins", reloadPlugins, this, CommandScope.Both, "pluginmanager_reload"));
            
            pluginManager = new PluginManager(relPluginPath);
            pluginManager.PluginsReloaded += new EventHandler(Plugins_PluginsReloaded);
            pluginManager.IgnoreErrors = false;
            pluginManager.PluginSources =  PluginSourceEnum.Both;
            
            pluginManager.AddReference("Huffelpuff.exe");
            
            pluginManager.Start();
        }

        private void reloadPlugins(object sender, IrcEventArgs e) {
            pluginManager.ReloadPlugins();
        }

        

        private List<string> oldPlugs = new List<string>();
        private void Plugins_PluginsReloaded(object sender, EventArgs e)
        {
            plugins.Clear();
            bot.CleanPlugins();
            
            foreach(string pluginName in pluginManager.GetSubclasses("Huffelpuff.Plugins.AbstractPlugin"))
            {
                AbstractPlugin p = (AbstractPlugin)pluginManager.CreateInstance(pluginName, BindingFlags.CreateInstance, new object[] {bot});
                p.Init();
                
                if (p.Ready) {
                    plugins.Add(p);
                } else {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" [FAILED] " + pluginName + " (Init failed)");
                }
            }

            foreach(AbstractPlugin ap in plugins)
            {
                if (oldPlugs.Contains(ap.AssemblyName)) {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(" [RELOAD] " + ap.FullName);
                    oldPlugs.Remove(ap.AssemblyName);
                } else {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  [LOAD]  " + ap.FullName);
                }
            }
            
            foreach (string s in oldPlugs) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" [REMOVE] " + s);
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            
            oldPlugs.Clear();
            foreach(AbstractPlugin ap in plugins)
            {
                oldPlugs.Add(ap.AssemblyName);
            }

            
            foreach(string pluginname in PersistentMemory.Instance.GetValues("plugin")) {
                foreach(AbstractPlugin p in plugins) {
                    if (pluginname==p.FullName) {
                        p.Activate();
                    }
                }
            }
        }
        
        public void ShutDown()
        {
            foreach(AbstractPlugin p in plugins) {
                try {
                    p.Deactivate();
                    p.DeInit();
                } catch (Exception) {
                    /* Plugins Domain does not Exist */
                }
                
            }

        }
    }
}
