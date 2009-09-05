using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;

using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;
using System.Timers;

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
            base(botInstance) {}
        
        public override string Name {
            get {
                return Assembly.GetExecutingAssembly().FullName;
            }
        }
        
        public override void Init()
        {
            annoyInterval = new Timer();
            annoyInterval.Elapsed += annoyInterval_Elapsed;
            annoyInterval.Interval = 45 * 60 * 1000;
            base.Init();
        }
        
        public override void Activate() {
            
            annoyInterval.Enabled = true;
            BotEvents.OnJoin += BotEvents_OnJoin;
            
            base.Activate();
        }

        void BotEvents_OnJoin(object sender, JoinEventArgs e)
        {
            if (e.Data.Channel == "#piraten-schweiz") {
                BotMethods.SendMessage(SendType.Notice, e.Who, "Dieser Channel ist nicht mehr der offizielle Piratenpartei Schweiz Channel - Bitte joine #pps / #pps-de / #pps-fr auf irc.piraten-partei.ch");
            }
            else if (e.Data.Channel == "#pirates-suisse")  {
                BotMethods.SendMessage(SendType.Notice, e.Who, "Ce channel n'est plus le channel officiel du Parti Pirate Suisse - merci de rejoindre #pps / #pps-de / #pps-fr sur irc.partipirate.ch");
            }
            else {
                BotMethods.SendMessage(SendType.Notice, e.Who, "This channel is no longer the official Pirate Party Switzerland Channel - Please join #pps / #pps-de / #pps-fr irc.piratpartiet.se");
            }
        }

        void annoyInterval_Elapsed(object sender, ElapsedEventArgs e)
        {
            counter++;
            foreach(string channel in BotMethods.GetChannels()) {
                switch (counter%3) {
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

        
        public override void Deactivate() {

            annoyInterval.Enabled = false;
            BotEvents.OnJoin -= new JoinEventHandler(BotEvents_OnJoin);
            base.Deactivate();
        }
        
        public override string AboutHelp() {
            return "This is a very simple Plugin annoys you till you leave ;)";
        }
    }
}