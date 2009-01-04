/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 26.10.2008
 * Zeit: 01:02
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using Huffelpuff;
using Huffelpuff.SimplePlugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
	/// <summary>
	/// FileServer Plugin
	/// </summary>
	public class FileServer : IPlugin
	{
		
		public string Name {
			get {
				return Assembly.GetExecutingAssembly().FullName;
			}
		}
		
		private bool ready = false;
		public bool Ready {
			get {
				return ready;
			}
		}
		
		private bool active;
		public bool Active {
			get {
				return active;
			}
		}
		
		private IrcBot bot;
		
		public void Init(IrcBot botInstance) {
			bot = botInstance;
			ready = true;
		}
		
		public void Activate() {
			bot.AddPublicCommand(new Commandlet("!download", "Starts a DCC Filetransfer to you", SendFile, this));
			active = true;
			
		}
		
		public void Deactivate() {
			bot.RemovePublicCommand("!download");
			active = false;
		}
		
		public string AboutHelp() {
			return "File Plugin Help";
		}
		
		public void SendFile(object sender, IrcEventArgs e)
		{
			bot.SendFile(e.Data.Nick, "./Huffelpuff.exe");
		}
		

			
	}
}