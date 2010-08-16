/*
 *  UT3GlobalStatsPlugin, Access to the GameSpy Stats for UT3
 * 
 *  Copyright (c) 2008-2010 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
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

using System.Linq;
using Huffelpuff.Utils;
using System;
using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;
using Plugin.ServiceReference;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class Ut3GlobalStatsPlugin : AbstractPlugin
    {
        public Ut3GlobalStatsPlugin(IrcBot botInstance) : base(botInstance) { }

        private GameSpyClient gsClient;
        private StorageServerSoap gsStorage;

        private string ticket;
        private DateTime lastused = DateTime.MinValue;

        private const int Ut3GameId = 1727;
        private const string StatsTable = "PlayerStats_v2";

        public override string Name
        {
            get
            {
                return "UT3 Global Player Stats (alpha 2)";
            }
        }

        public override void Init()
        {
            gsClient = new GameSpyClient();
            gsStorage = new StorageServerSoapClient();
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
            gsClient.LogOut();
            base.DeInit();
        }

        public override string AboutHelp()
        {
            return "The UT3 Global Stats Plugin can be used to directly query the Global UT3 Stats from GameSpy";
        }

        private void PlayerHandler(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            if (e.Data.MessageArray.Length < 2)
                return;

            RecordValue[][] values;

            // *** hard coded query ***
            const bool cacheFlag = false;
            var ownerIds = new ArrayOfInt();
            var queryFields = new ArrayOfString { "row", "Nick", "Pure_PlayerDM_ELO", "Pure_PlayerDM_EVENT_KILLS", "Pure_PlayerDM_EVENT_DEATHS" };
            const int surrounding = 0;
            const int limit = 0;
            const int offset = 0;
            const string targetFilter = "";
            const string orderBy = "Pure_PlayerDM_ELO desc";
            var filter = "Nick LIKE '" + e.Data.MessageArray[1] + "'";
            // *** end hard coded query ***

            EnsureTicket();


            try
            {
                var searchForRecordsRequest = new SearchForRecordsRequest(new SearchForRecordsRequestBody(Ut3GameId, ticket, StatsTable, queryFields, filter,
                                                      orderBy, offset, limit, targetFilter, surrounding, ownerIds, cacheFlag));
                var recordsResponse = gsStorage.SearchForRecords(searchForRecordsRequest);
                values = recordsResponse.Body.values;

                if (recordsResponse.Body.SearchForRecordsResult == Result.Success)
                {
                    Log.Instance.Log("Webservice returned '" + recordsResponse + "' instead of success.", Level.Fail);
                    return;
                }
            }
            catch (Exception exception)
            {
                Log.Instance.Log(exception);
                return;
            }

            foreach (string msg in values.Select(perPlayerValues => RecordValueToString(perPlayerValues[0]) + ". " + RecordValueToString(perPlayerValues[1]) + " ELO: " + RecordValueToString(perPlayerValues[2]) + " Kills: " + RecordValueToString(perPlayerValues[3]) + " Deaths: " + RecordValueToString(perPlayerValues[4])))
            {
                BotMethods.SendMessage(SendType.Notice, sendto, msg);
            }
        }

        private void TopTenHandler(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            RecordValue[][] values;

            // *** hard coded query ***
            const bool cacheFlag = false;
            var ownerIds = new ArrayOfInt();
            var queryFields = new ArrayOfString { "row", "Nick", "Pure_PlayerDM_ELO", "Pure_PlayerDM_EVENT_KILLS", "Pure_PlayerDM_EVENT_DEATHS" };
            const int surrounding = 0;
            const int limit = 10;
            const int offset = 0;
            const string targetFilter = "";
            const string orderBy = "Pure_PlayerDM_ELO desc";
            const string filter = "NUM_Pure_PlayerDM > 0";
            // *** end hard coded query ***

            EnsureTicket();


            try
            {
                var searchForRecordsRequest = new SearchForRecordsRequest(new SearchForRecordsRequestBody(Ut3GameId, ticket, StatsTable, queryFields, filter,
                                                      orderBy, offset, limit, targetFilter, surrounding, ownerIds, cacheFlag));
                var recordsResponse = gsStorage.SearchForRecords(searchForRecordsRequest);
                values = recordsResponse.Body.values;
                if (recordsResponse.Body.SearchForRecordsResult == Result.Success)
                {
                    Log.Instance.Log("Webservice returned '" + recordsResponse.Body.SearchForRecordsResult + "' instead of success.");
                    return;
                }
            }
            catch (Exception exception)
            {
                Log.Instance.Log(exception);
                return;
            }
            foreach (var msg in values.Select(perPlayerValues => RecordValueToString(perPlayerValues[0]) + ". " + RecordValueToString(perPlayerValues[1]) + " ELO: " + RecordValueToString(perPlayerValues[2]) + " Kills: " + RecordValueToString(perPlayerValues[3]) + " Deaths: " + RecordValueToString(perPlayerValues[4])))
            {
                BotMethods.SendMessage(SendType.Notice, sendto, msg);
            }
        }

        private void EnsureTicket()
        {
            // get new ticket after 30 minutes
            if ((lastused.AddMinutes(30)) >= DateTime.Now) return;

            ticket = gsClient.GetTicket("Apophis", "khrut", false);
            lastused = DateTime.Now;
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