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
		private IrcBot bot;
		private string pluginPath;
		private List<IPlugin> plugins = new List<IPlugin>();
		public List<IPlugin> Plugins {
			get {
				return plugins;
			}
		}
		
		public SimplePluginManager(IrcBot bot, string relPluginPath)
		{
			this.bot = bot;
						
			pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			pluginPath = pluginPath + Path.DirectorySeparatorChar + relPluginPath + Path.DirectorySeparatorChar;
			FileInfo pldir = new FileInfo(pluginPath + Path.DirectorySeparatorChar + relPluginPath + Path.DirectorySeparatorChar);
			if(!pldir.Exists) {
				Directory.CreateDirectory(pluginPath + Path.DirectorySeparatorChar + relPluginPath + Path.DirectorySeparatorChar);
			}		
				
			string[] pluginFiles = Directory.GetFiles(pluginPath, "*.dll");
			
			foreach(string filename in pluginFiles)
        	{
				Type[] objTypes = null;
				Assembly plugAssembly;
				try {
					plugAssembly = Assembly.LoadFile(filename);
					if (plugAssembly != null) objTypes = plugAssembly.GetExportedTypes();
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
			    foreach(string pluginname in PersistentMemory.GetValues("plugin")) {
				    foreach(IPlugin p in plugins) {
					    if (pluginname==p.GetType().ToString()) {
						    p.Activate();
					    }
				    }
                }
			}
		}
		
		public void ShutDown()
		{
			foreach(IPlugin p in plugins) {	
				p.Deactivate();
				p.DeInit();
			}
		}
	}
}
