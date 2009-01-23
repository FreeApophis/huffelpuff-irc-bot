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
using System.Reflection;
using System.Collections.Generic;


namespace Huffelpuff.ComplexPlugins
{
	/// <summary>
	/// Description of ComplexPluginManager.
	/// </summary>
	public class ComplexPluginManager
	{
	    private PluginManager pluginManager;
        private IrcBot bot;
		private List<AbstractPlugin> plugins = new List<AbstractPlugin>();
		public List<AbstractPlugin> Plugins {
		    get {
		        return plugins;
		    }
		}
		
		public ComplexPluginManager(IrcBot bot, string relPluginPath)
		{
		    this.bot = bot;
		    
		    pluginManager = new PluginManager(relPluginPath);
            pluginManager.PluginsReloaded += new EventHandler(Plugins_PluginsReloaded);
            pluginManager.IgnoreErrors = false;
            pluginManager.PluginSources =  PluginSourceEnum.Both;

            pluginManager.Start();
		}
		

		private List<string> oldPlugs = new List<string>();
        private void Plugins_PluginsReloaded(object sender, EventArgs e)
        {
                        
            plugins.Clear();
            bot.CleanComplexPlugins();
            
            foreach(string pluginName in pluginManager.GetSubclasses("Huffelpuff.ComplexPlugins.AbstractPlugin"))
            {
                AbstractPlugin p = (AbstractPlugin)pluginManager.CreateInstance(pluginName, BindingFlags.CreateInstance, new object[] {bot});
                p.Init();
                
                if (p.Ready) {
                  plugins.Add(p);
                } else {
        			Console.WriteLine("Init Failed, Plugin not loaded");
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

            
            foreach(string pluginname in PersistentMemory.GetValues("plugin")) {
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
