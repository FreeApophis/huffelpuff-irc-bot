/*
 *  The File Plugin Demonstrates the Use of DCC Send to transfer files 
 *  to another IRC Client.
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