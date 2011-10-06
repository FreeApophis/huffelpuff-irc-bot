/*
 *  Wolfram Alpha Plugin
 *  ---------------------------------------------------------
 * 
 *  Copyright (c) 2011 Thomas Bruderer <apophis@apophis.ch>
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

using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
    public class WolframAlphaPlugin : AbstractPlugin
    {

        public WolframAlphaPlugin(IrcBot botInstance) :
            base(botInstance) { }
        public override string AboutHelp()
        {
            return "This is the help about the whole CommentedExamplePlugin";
        }


        public override void Init()
        {
            TickInterval = 60;

            base.Init();
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override void DeInit()
        {
            base.DeInit();
        }

        public override void OnTick()
        {

        }
    }
}