/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 18.06.2009 13:57
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
using System.Net;
using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;
using System.Text.RegularExpressions;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class UrlToTitlePlugin : AbstractPlugin
    {
        public UrlToTitlePlugin(IrcBot botInstance) : base(botInstance) {}
        
        
        public override string AboutHelp()
        {
            return "Translates any discussion";
        }

        public override void Init()
        {
            base.Init();
        }
        
        
        
        public override void Activate()
        {
            if (!this.active) {
                BotEvents.OnChannelMessage += new IrcEventHandler(BotEvents_OnChannelMessage);
            }

            base.Activate();
        }
        
        public override void Deactivate()
        {
            BotEvents.OnChannelMessage -= new IrcEventHandler(BotEvents_OnChannelMessage);

            base.Deactivate();
        }
        
        private Regex titleMatch = new Regex("(?<=<title>)[^<]*(?=</title>)");
        private void BotEvents_OnChannelMessage(object sender, IrcEventArgs e)
        {
            try {
                if (e.Data.MessageArray.Length == 1) {
                    if (e.Data.Message.StartsWith("http://")) {
                        WebClient client = new WebClient();
                        string page = client.DownloadString(e.Data.Message);
                        Match m = titleMatch.Match(page);
                        BotMethods.SendMessage(SendType.Message, e.Data.Channel, m.Value);
                    }
                }
            } catch {}
            
        }
    }
}
