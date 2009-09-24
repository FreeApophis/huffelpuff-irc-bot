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
using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using DotNetWikiBot;
using Huffelpuff;
using Huffelpuff.Plugins;

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
            BotMethods.AddCommand(new Commandlet("!wiki", "wiki wiki", wikiHandler, this, CommandScope.Both));
            
            base.Activate();
        }
        
        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!wiki");

            base.Deactivate();
        }
        
        public override string AboutHelp()
        {
            return "wiki wiki!!!";
        }
        
        private void wikiHandler(object sender, IrcEventArgs e) {
            Site enWiki = new Site("http://en.wikipedia.org", "", "");
            Page page = new Page(enWiki, "Api");
            page.Load();
            Console.WriteLine(page.ToString());
        }
    }
}