/*
 *  The Seen Plugin tells you when a certain Nick was last used
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
using System.Reflection;
using System.Collections.Generic;

using Huffelpuff;
using Huffelpuff.SimplePlugins;

using Meebey.SmartIrc4net;

namespace Plugin
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
        
        public void DeInit()
        {
            ready = false;
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