/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 24.10.2008
 * Zeit: 20:42
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Reflection;
using System.Collections.Generic;

using Huffelpuff;
using Huffelpuff.SimplePlugins;

using Meebey.SmartIrc4net;

namespace Plugins
{
	/// <summary>
	/// Description of MyClass.
	/// </summary>
	public class SeenPlugin : IPlugin
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
		
		public void Init(IrcBot botInstance)
		{
			bot = botInstance;
			ready = true;
		}
		
		public void Activate()
		{
			bot.AddPublicCommand(new Commandlet("!seen", "The command !seen <nick> tells you when a certain nick was last seen in this channel ", seenCommand, this));
			bot.OnChannelMessage += new IrcEventHandler(messageHandler);
			bot.OnNickChange += new NickChangeEventHandler(nickChangeHandler);
			bot.OnJoin += new JoinEventHandler(joinHandler);
			bot.OnPart += new PartEventHandler(partHandler);
			bot.OnQuit += new QuitEventHandler(quitHandler);
			bot.OnKick += new KickEventHandler(kickHandler);
			
			/* People in this channel right now */
			
			active = true;
		}
		
		public void Deactivate()
		{
			bot.RemovePublicCommand("!seen");
			bot.OnChannelMessage -= new IrcEventHandler(messageHandler);
			bot.OnNickChange -= new NickChangeEventHandler(nickChangeHandler);
			bot.OnJoin -= new JoinEventHandler(joinHandler);
			bot.OnPart -= new PartEventHandler(partHandler);
			bot.OnQuit -= new QuitEventHandler(quitHandler);
			bot.OnKick -= new KickEventHandler(kickHandler);
			
			/* People in this channel right now ? */

			active = false;
		}
		
		public string AboutHelp()
		{
			return "This is the basic Last Seen Plugin";
		}
				
		private void seenCommand(object sender, IrcEventArgs e) {
			
		}
		
		private void messageHandler(object sender, IrcEventArgs e) {
		}
		
		private void nickChangeHandler(object sender, NickChangeEventArgs e) {
		}

		private void joinHandler(object sender, JoinEventArgs e) {
		}

		private void partHandler(object sender, PartEventArgs e) {
		}

		private void quitHandler(object sender, QuitEventArgs e) {
		}

		private void kickHandler(object sender, KickEventArgs e) {
		}

		
	}
}