/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 04.12.2008
 * Zeit: 23:27
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Collections.Generic;

using Huffelpuff;
using Huffelpuff.SimplePlugins;
using Meebey.SmartIrc4net;
using AIMLbot;

namespace Plugin
{
	/// <summary>
	/// Description of MyClass.
	/// </summary>
	public class AIMLPlugin : IPlugin
	{
		
		
		public string Name {
			get {
				return "Artificial Intelligence Markup Language Plugin";
			}
		}
		
		private IrcBot bot = null;
		private bool ready = false;
		public bool Ready {
			get {
				return ready;
			}
		}
		
		private bool active = false;	
		public bool Active {
			get {
				return active;
			}
		}		

		Bot myBot;
		Dictionary<string, User> myUsers = new Dictionary<string, User>();
		
		public void Init(IrcBot botInstance)
		{
			bot = botInstance;
			
			myBot = new Bot();
            myBot.loadSettings();
            myBot.isAcceptingUserInput = false;
            myBot.loadAIMLFromFiles();
            myBot.isAcceptingUserInput = true;
            
			ready = true;
		}
		
		public void Activate()
		{
			bot.OnChannelMessage += new IrcEventHandler(messageHandler);
			active = true;
		}
		
		public void Deactivate()
		{
			bot.OnChannelMessage -= new IrcEventHandler(messageHandler);
			active = false;
		}
		
		public string AboutHelp()
		{
			return "Artificial Intelligence Markup Language Plugin";
		}
		
		public List<KeyValuePair<string, string>> Commands()
		{
			return new List<KeyValuePair<string, string>>();
		}
		
		private void messageHandler(object sender, IrcEventArgs e) {
			
			
			string msg;
			if(e.Data.Message.ToLower().Contains(bot.Nickname.ToLower()))
			{
				if (e.Data.Message.ToLower().Trim().StartsWith(bot.Nickname.ToLower()))
					msg = e.Data.Message.Trim().Substring(bot.Nickname.Length+1);
				else 
					msg = e.Data.Message.Trim();
				User myUser = null;
				if (myUsers.ContainsKey(e.Data.Nick)) {
					myUser = myUsers[e.Data.Nick];
				} else {
					myUser = new User(e.Data.Nick, myBot);
					myUser.Predicates.addSetting("name", e.Data.Nick);
					
					myUsers.Add(e.Data.Nick, myUser);
				}
				Request r = new Request(msg, myUser, myBot);
            	Result res = myBot.Chat(r);
            	bot.SendMessage(SendType.Message, e.Data.Channel, res.Output);
			}
		}
	}

}