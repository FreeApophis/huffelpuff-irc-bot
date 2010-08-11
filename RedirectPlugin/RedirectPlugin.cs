/*
 *  The Huffelpuff Irc Bot, versatile pluggable bot for IRC chats
 * 
 *  Copyright (c) 2008-2010 Thomas Bruderer <apophis@apophis.ch>
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

using System.Reflection;
using System.Timers;
using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// This is a very simple Plugin Example: The Echo Plugin
    /// </summary>
    public class RedirectPlugin : AbstractPlugin
    {

        private Timer annoyInterval;
        private int counter = 1;

        public RedirectPlugin(IrcBot botInstance) :
            base(botInstance) { }

        public override string Name
        {
            get
            {
                return Assembly.GetExecutingAssembly().FullName;
            }
        }

        public override void Init()
        {
            annoyInterval = new Timer();
            annoyInterval.Elapsed += AnnoyIntervalElapsed;
            annoyInterval.Interval = 45 * 60 * 1000;
            base.Init();
        }

        public override void Activate()
        {

            annoyInterval.Enabled = true;
            BotEvents.OnJoin += BotEvents_OnJoin;

            base.Activate();
        }

        void BotEvents_OnJoin(object sender, JoinEventArgs e)
        {
            if (e.Data.Channel == "#piraten-schweiz")
            {
                BotMethods.SendMessage(SendType.Notice, e.Who, "Dieser Channel ist nicht mehr der offizielle Piratenpartei Schweiz Channel - Bitte joine #pps / #pps-de / #pps-fr auf irc.piraten-partei.ch");
            }
            else if (e.Data.Channel == "#pirates-suisse")
            {
                BotMethods.SendMessage(SendType.Notice, e.Who, "Ce channel n'est plus le channel officiel du Parti Pirate Suisse - merci de rejoindre #pps / #pps-de / #pps-fr sur irc.partipirate.ch");
            }
            else
            {
                BotMethods.SendMessage(SendType.Notice, e.Who, "This channel is no longer the official Pirate Party Switzerland Channel - Please join #pps / #pps-de / #pps-fr irc.piratpartiet.se");
            }
        }

        void AnnoyIntervalElapsed(object sender, ElapsedEventArgs e)
        {
            counter++;
            foreach (var channel in BotMethods.GetChannels())
            {
                switch (counter % 3)
                {
                    case 0:
                        BotMethods.SendMessage(SendType.Notice, channel, "This channel is no longer the official Pirate Party Switzerland Channel - Please join #pps / #pps-de / #pps-fr on irc.piratpartiet.se");
                        break;
                    case 1:
                        BotMethods.SendMessage(SendType.Notice, channel, "Dieser Channel ist nicht mehr der offizielle Piratenpartei Schweiz Channel - Bitte joine #pps / #pps-de / #pps-fr auf irc.piraten-partei.ch");
                        break;
                    case 2:
                        BotMethods.SendMessage(SendType.Notice, channel, "Ce channel n'est plus le channel officiel du Parti Pirate Suisse - merci de rejoindre #pps / #pps-de / #pps-fr sur irc.partipirate.ch");
                        break;
                }
            }
        }


        public override void Deactivate()
        {
            annoyInterval.Enabled = false;
            BotEvents.OnJoin -= BotEvents_OnJoin;
            base.Deactivate();
        }

        public override string AboutHelp()
        {
            return "This is a very simple Plugin annoys you till you leave ;)";
        }
    }
}