/*
 *  This is the Factoid Plugin, the usage is very similar to ubottu.
 *  You can generate arbitrary !commands to reply with prefabricated text.
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
using System.Linq;

using Huffelpuff;
using Huffelpuff.Database;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace FactoidPlugin
{
	/// <summary>
	/// Supports !factoid, !factoid > nick, !factoid | nick
	/// </summary>
	public class FactoidPlugin : AbstractPlugin
	{
		public FactoidPlugin(IrcBot botInstance) : base(botInstance) {}
		
		public override void Init()
		{
			
			base.Init();
		}

		public override void Activate()
		{
			BotMethods.AddCommand(new Commandlet("!facts", "Lists all the facts.", listFacts, this, CommandScope.Private));
			BotMethods.AddCommand(new Commandlet("!+fact", "With the command !+fact <fact> <descriptive text> you can add a fact with its description, or you can link a new fact to the same description with !+fact <newfact> <oldfact>.", addFact, this, CommandScope.Both, "fact_admin"));
			BotMethods.AddCommand(new Commandlet("!-fact", "With the command !-fact <fact> you can remove a fact.", removeFact, this, CommandScope.Both, "fact_admin"));
			BotEvents.OnQueryMessage += BotEvents_OnQueryMessage;
			BotEvents.OnChannelMessage += BotEvents_OnQueryMessage;
			
			base.Activate();
		}

		public override void Deactivate()
		{
			BotMethods.RemoveCommand("!+fact");
			BotMethods.RemoveCommand("!-fact");
			
			BotEvents.OnQueryMessage -= BotEvents_OnQueryMessage;
			BotEvents.OnChannelMessage -= BotEvents_OnQueryMessage;
			
			base.Deactivate();
		}
		
		public override string AboutHelp()
		{
			return "This is the Factoid Plugin, the usage is very similar to ubottu. You can generate arbitrary !commands to reply with prefabricated text.";
		}
		
		void BotEvents_OnQueryMessage(object sender, IrcEventArgs e)
		{
			if (e.Data.MessageArray.Length > 0)
			{
				if (e.Data.MessageArray[0].StartsWith("!"))
				{
					
				}
				Db.FactKeys.Where(facts => facts.Key ==  e.Data.MessageArray[0].Substring(1)).Select( fact => fact.ID);
			}
			
			
			if(e.Data.MessageArray[0].StartsWith("!"))
			{
				
			}
		}
		
		private void listFacts(object sender, IrcEventArgs e)
		{
			foreach(var fact in Db.FactKeys)
			{
				
			}
		}
		
		private void addFact(object sender, IrcEventArgs e)
		{
			var fact = new FactKey();
			Db.FactKeys.InsertOnSubmit(fact);			
			Db.SubmitChanges();
		}
		private void removeFact(object sender, IrcEventArgs e)
		{
			
		}		
	}
}