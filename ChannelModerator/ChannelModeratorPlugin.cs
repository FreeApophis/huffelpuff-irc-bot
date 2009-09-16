/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 14.09.2009 19:02
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
using Huffelpuff;
using Huffelpuff.Plugins;

namespace ChannelModerator
{
    /// <summary>
    /// Ready for kick, op and war ;)
    /// </summary>
    public class ChannelModeratorPlugin : AbstractPlugin
    {
        public ChannelModeratorPlugin(IrcBot botInstance) :
            base(botInstance) {}
        
        public override void Init()
        {
            base.Init();
        }
        
        public override void Activate ()
        {
            BotMethods.AddCommand(new Commandlet("!kick", "kicks an annyoing user", kickUser, this));
            
            base.Activate();
        }
        
        public override void Deactivate ()
        {
            BotMethods.RemoveCommand("!kick");
            
            base.Deactivate();
        }
        
        public override string AboutHelp()
        {
            return "The channel moderator plugins, manages your channel...";
        }
        
        private void kickUser(object sender, IrcEventArgs e) 
        {
            
        }
    }
}
