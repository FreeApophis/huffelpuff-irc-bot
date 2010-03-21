/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 03.07.2009 18:53
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
using System.Net;
using System.Xml;

using DotNetWikiBot;
using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace Wikipedia
{
	/// <summary>
	/// Description Wikipedia Plugin.
	/// </summary>
	public class WikipediaPlugin : AbstractPlugin
	{
		public WikipediaPlugin(IrcBot botInstance) : base(botInstance) {}
		
		public override void Init()
		{
			base.Init();
		}
		
		public override void Activate()
		{
			BotMethods.AddCommand(new Commandlet("!w", "!w <word> Returns the wikipedia desciption.", wikiHandler, this, CommandScope.Both));
			
			base.Activate();
		}
		
		public override void Deactivate()
		{
			BotMethods.RemoveCommand("!w");

			base.Deactivate();
		}
		
		public override string AboutHelp()
		{
			return "Wikipedia Plugin!!!";
		}
		
		private void wikiHandler(object sender, IrcEventArgs e) {
			
			var document = new XmlDocument();
			var request = WebRequest.Create("http://en.wikipedia.org/w/api.php?action=query&prop=revisions&rvprop=content&format=xml&titles=Sex") as HttpWebRequest;
			request.UserAgent = "Mozilla/5.0 (Huffelpuff)";
			if (request != null)
			{
				document.Load(request.GetResponse().GetResponseStream());
			}
			
			var node = document.DocumentElement.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild;
			BotMethods.SendMessage(SendType.Message, e.Data.Channel, node.InnerText.Substring(0, 200));
		}
	}
}