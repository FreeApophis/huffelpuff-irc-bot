/*
 *  UT3GlobalStatsPlugin, Access to the GameSpy Stats for UT3
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
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
using System.Collections.Generic;

using Huffelpuff;
using Huffelpuff.Plugins;

using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class UT3GlobalStatsPlugin : AbstractPlugin
    {
        public UT3GlobalStatsPlugin(IrcBot botInstance) : base(botInstance) {}

        private GameSpyClient gsClient;
        private StorageServer gsStorage;
        
        private string ticket;
        private DateTime lastused = DateTime.MinValue;
        
        private const int ut3gameID = 1727;
        private const string statsTable = "PlayerStats_v2";
        
        public override string Name {
            get {
                return "UT3 Global Player Stats (alpha 2)";
            }
        }
        
        public override void Init()
        {
            gsClient = new GameSpyClient();
            gsStorage = new StorageServer();
            base.Init();
        }
        
        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!top10", "The !top10 Command shows the UT3 Players with the highest ELO Ranking on Pure Servers", TopTenHandler, this));
            BotMethods.AddCommand(new Commandlet("!player", "The !player <nick> Command shows Stats about a Certain Player", PlayerHandler, this));
            base.Activate();
        }
        
        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!top10");
            BotMethods.RemoveCommand("!player");
            base.Deactivate();
        }
        
        
        public override void DeInit()
        {
            gsClient.logOut();
            base.DeInit();
        }
        
        public override string AboutHelp()
        {
            return "The UT3 Global Stats Plugin can be used to directly query the Global UT3 Stats from GameSpy";
        }
        
        private void PlayerHandler(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            if (e.Data.MessageArray.Length < 2)
                return;
            
            RecordValue[][] values = null;
            
            // *** hard coded query ***
            bool cacheFlag = false;
            List<int> ownerIds = new List<int>();
            List<string> queryFields = new List<string>();
            queryFields.Add("row");
            queryFields.Add("Nick");
            queryFields.Add("Pure_PlayerDM_ELO");
            queryFields.Add("Pure_PlayerDM_EVENT_KILLS");
            queryFields.Add("Pure_PlayerDM_EVENT_DEATHS");
            int surrounding = 0;
            int limit = 0;
            int offset = 0;
            string targetFilter = "";
            string orderBy = "Pure_PlayerDM_ELO desc";
            string filter = "Nick LIKE '" + e.Data.MessageArray[1] + "'";
            // *** end hard coded query ***
            
            ensureTicket();
            
            
            try
            {
                Result s = gsStorage.SearchForRecords(ut3gameID, ticket, statsTable, queryFields.ToArray(), filter, orderBy, offset, limit, targetFilter, surrounding, ownerIds.ToArray(), cacheFlag, out values);
                if (s != Result.Success) {
                    System.Console.WriteLine("Webservice returned '" + s.ToString() + "' instead of success.");
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return;
            }
            foreach (RecordValue[] perPlayerValues in values)
            {
                string msg = RecordValueToString(perPlayerValues[0]) + ". " + RecordValueToString(perPlayerValues[1]) +
                    " ELO: " + RecordValueToString(perPlayerValues[2]) +
                    " Kills: " + RecordValueToString(perPlayerValues[3]) +
                    " Deaths: " + RecordValueToString(perPlayerValues[4]);
                BotMethods.SendMessage(SendType.Notice, sendto, msg);
                foreach (RecordValue singleValue in perPlayerValues)
                {
                }
            }
        }
        
        private void TopTenHandler(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            RecordValue[][] values = null;
            
            // *** hard coded query ***
            bool cacheFlag = false;
            List<int> ownerIds = new List<int>();
            List<string> queryFields = new List<string>();
            queryFields.Add("row");
            queryFields.Add("Nick");
            queryFields.Add("Pure_PlayerDM_ELO");
            queryFields.Add("Pure_PlayerDM_EVENT_KILLS");
            queryFields.Add("Pure_PlayerDM_EVENT_DEATHS");
            int surrounding = 0;
            int limit = 10;
            int offset = 0;
            string targetFilter = "";
            string orderBy = "Pure_PlayerDM_ELO desc";
            string filter = "NUM_Pure_PlayerDM > 0";
            // *** end hard coded query ***
            
            ensureTicket();
            
            
            try
            {
                Result s = gsStorage.SearchForRecords(ut3gameID, ticket, statsTable, queryFields.ToArray(), filter, orderBy, offset, limit, targetFilter, surrounding, ownerIds.ToArray(), cacheFlag, out values);
                if (s != Result.Success) {
                    System.Console.WriteLine("Webservice returned '" + s.ToString() + "' instead of success.");
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return;
            }
            foreach (RecordValue[] perPlayerValues in values)
            {
                string msg = RecordValueToString(perPlayerValues[0]) + ". " + RecordValueToString(perPlayerValues[1]) +
                    " ELO: " + RecordValueToString(perPlayerValues[2]) +
                    " Kills: " + RecordValueToString(perPlayerValues[3]) +
                    " Deaths: " + RecordValueToString(perPlayerValues[4]);
                BotMethods.SendMessage(SendType.Notice, sendto, msg);
                foreach (RecordValue singleValue in perPlayerValues)
                {
                }
            }
        }

        private void ensureTicket()
        {
            // get new ticket after 30 minutes
            if ((lastused.AddMinutes(30)) < DateTime.Now) {
                ticket = gsClient.getTicket("Apophis", "khrut", false);
                lastused = DateTime.Now;
            }
        }
        
        private static string RecordValueToString(RecordValue singleValue)
        {
            if (singleValue.asciiStringValue != null)
                return singleValue.asciiStringValue.value;
            if (singleValue.intValue != null)
                return singleValue.intValue.value.ToString();
            if (singleValue.binaryDataValue != null)
                return "<binaryBlob>";
            if (singleValue.booleanValue != null)
                return singleValue.booleanValue.value.ToString();
            if (singleValue.byteValue != null)
                return singleValue.byteValue.value.ToString();
            if (singleValue.dateAndTimeValue != null)
                return singleValue.dateAndTimeValue.value.ToString();
            if (singleValue.floatValue != null)
                return singleValue.shortValue.value.ToString();
            if (singleValue.shortValue != null)
                return singleValue.shortValue.value.ToString();
            if (singleValue.unicodeStringValue != null)
                return singleValue.unicodeStringValue.value;
            return "<empty>";
        }

    }
}