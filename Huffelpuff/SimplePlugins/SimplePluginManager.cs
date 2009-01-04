/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 08.10.2008
 * Zeit: 00:42
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Meebey.SmartIrc4net;

namespace Huffelpuff.SimplePlugins
{
	/// <summary>
	/// Description of PluginManager.
	/// </summary>
	public class SimplePluginManager
	{
		private string pluginPath;
		private IrcBot bot;
		private List<IPlugin> plugins = new List<IPlugin>();
		public List<IPlugin> Plugins {
			get {
				return plugins;
			}
		}
		
		public SimplePluginManager(IrcBot bot)
		{
			this.bot = bot;
			
			bot.AddPublicCommand(new Commandlet("!plugins", "HELP ON PLUGINS", this.PluginsCommand, this));
			bot.AddPublicCommand(new Commandlet("!activate", "HELP ON ACTIVATE", this.ActivateCommand, this));
			bot.AddPublicCommand(new Commandlet("!deactivate", "HELP ON DEACTIVATE", this.DeactivateCommand, this));
			
			pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			pluginPath = pluginPath + Path.DirectorySeparatorChar + "plugins"+ Path.DirectorySeparatorChar;
			FileInfo pldir = new FileInfo(pluginPath + Path.DirectorySeparatorChar + "plugins"+ Path.DirectorySeparatorChar);
			if(!pldir.Exists) {
				Directory.CreateDirectory(pluginPath + Path.DirectorySeparatorChar + "plugins"+ Path.DirectorySeparatorChar);
			}		
				
			string[] pluginFiles = Directory.GetFiles(pluginPath, "*.dll");
			
			foreach(string filename in pluginFiles)
        	{
				Type[] objTypes = null;
				Assembly plugAssembly;
				try {
					plugAssembly = Assembly.LoadFile(filename);
					if (plugAssembly != null) objTypes = plugAssembly.GetTypes();
				} catch (Exception e) {
					Console.WriteLine(e.Message);
				}
				foreach (Type objType in objTypes) {
					try {
						object o = Activator.CreateInstance(objType);
						if(o is IPlugin) {
							Console.WriteLine("Plugin Found: " + objType.ToString());
							((IPlugin)o).Init(bot);
							if (((IPlugin)o).Ready) {
								plugins.Add((IPlugin)o);
								Console.WriteLine("Plugin Ready");
							} else {
								/* It should always be ready (take care with threading)*/
								Console.WriteLine("Init Failed, Plugin not loaded");
							}

						}
					} catch (MissingMethodException) {}
				}
			}
		}
			
		/// <summary>
		/// Activates the Plugins which have been activated the last time
		/// </summary>
		public void ActivatePlugins()
		{
	    	foreach(string pluginname in PersistentMemory.GetValues("plugin")) {
				foreach(IPlugin p in plugins) {
					if (pluginname==p.GetType().ToString()) {
						p.Activate();
					}
				}
            }
		}
		
		public void ShutDown()
		{
			foreach(IPlugin p in plugins) {	
				p.Deactivate();
				//p.DeInit();
			}

		}

		public void UpdatePlugins()
		{
			/* we only have one Appdomain, we need to restart the application to unload plugins */
			throw new NotImplementedException();
		}
		
		private void PluginsCommand(object sender, IrcEventArgs e)
		{
			foreach(IPlugin p in plugins) {	
				bot.SendMessage(SendType.Notice, e.Data.Channel, "Plugin: "+IrcConstants.IrcBold+p.GetType().ToString()+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]");
			}
		}
	
		private void ActivateCommand(object sender, IrcEventArgs e)
		{
			if (e.Data.MessageArray.Length < 2)
				return;
			foreach(IPlugin p in plugins) {
				if (e.Data.MessageArray[1]==p.GetType().ToString()) {
					PersistentMemory.SetValue("plugin", p.GetType().ToString());
					PersistentMemory.Flush();
					p.Activate();
					bot.SendMessage(SendType.Notice, e.Data.Channel, "Plugin: "+IrcConstants.IrcBold+p.GetType().ToString()+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]");
				}
			}
		}

		private void DeactivateCommand(object sender, IrcEventArgs e)
		{
			if (e.Data.MessageArray.Length < 2)
				return;
			foreach(IPlugin p in plugins) {
				if (e.Data.MessageArray[1]==p.GetType().ToString()) {
					PersistentMemory.RemoveValue("plugin", p.GetType().ToString());
					PersistentMemory.Flush();
					p.Deactivate();
					bot.SendMessage(SendType.Notice, e.Data.Channel, "Plugin: "+IrcConstants.IrcBold+p.GetType().ToString()+" ["+((p.Active?IrcConstants.IrcColor+""+(int)IrcColors.LightGreen+"ON":IrcConstants.IrcColor+""+(int)IrcColors.LightRed+"OFF"))+IrcConstants.IrcColor+"]");
				}
			}
		}
	}
}
