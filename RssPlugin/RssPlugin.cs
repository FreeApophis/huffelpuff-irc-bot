/*
 *  UT3GlobalStatsPlugin, Access to the GameSpy Stats for UT3
 * 
 *  Copyright (c) 2007-2009 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 2 of the License, or
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
using System.Xml;
using System.Collections.Generic;

using Huffelpuff;
using Huffelpuff.ComplexPlugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class RssPlugin : AbstractPlugin
    {
        public RssPlugin(IrcBot botInstance) :
            base(botInstance) {}
        
        public override void Activate()
        {
            throw new NotImplementedException();
        }
        
        public override void Deactivate()
        {
            throw new NotImplementedException();
        }
        
        public override string AboutHelp()
        {
            throw new NotImplementedException();
        }
        
        private void getRss() {
            XmlReader feed = XmlReader.Create("http://forum.vis.ethz.ch/external.php?type=RSS2");
            while(feed.MoveToElement()){
                
            }
        }
    }
}