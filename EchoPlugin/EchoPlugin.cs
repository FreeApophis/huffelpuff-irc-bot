/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 07.10.2008
 * Zeit: 23:48
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Remoting;

using Huffelpuff;
using Huffelpuff.ComplexPlugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
	/// <summary>
	/// This is a very simple Plugin Example: The Echo Plugin
	/// </summary>
	public class EchoPlugin : AbstractPlugin
	{
		
		public EchoPlugin(IrcBot botInstance) 
			: base(botInstance) {}
		
		private bool ready = false;
		public override bool Ready {
			get {
				return ready;
			}
		}
		
		private bool active = false;	
		public override bool Active {
			get {
				return active;
			}
		}		
		public override string Name {
			get {
				return Assembly.GetExecutingAssembly().FullName;
			}
		}
			
		public override void Init() {
			ready = true;
		}
		
		
		
		public override void Activate() {
			//this.AddPublicCommand(new Commandlet("!say", "!say <your text>, says whatever text you want it to say", sayHandler, this));
			//this.RegisterEvent(bot.OnJoin, this.sayHandler);
			BotEvents.OnChannelMessage  +=  new IrcEventHandler(sayHandler);
			active = true;
		}

		
		public override void Deactivate() {
			//this.RemovePublicCommand("!say");
			active = false;
		}
		
		public override string AboutHelp() {
			return "This is a very simple Plugin which repeats the message you said";
		}
				
		private void sayHandler(object sender, IrcEventArgs e) {
			BotMethods.SendMessage(SendType.Message, e.Data.Channel, e.Data.Message.Substring(5));
		}
	}
}
