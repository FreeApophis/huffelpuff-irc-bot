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


using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Meebey.SmartIrc4net;

namespace Huffelpuff
{
    public sealed class ServiceEngine : ServiceBase
    {
        public static string HuffelpuffServiceName = "Huffelpuff IRC Bot";
        private Thread botThread;
        public IrcBot Bot { get; set; }

        public ServiceEngine()
        {
            ServiceName = HuffelpuffServiceName;

            CanHandlePowerEvent = false;
            CanPauseAndContinue = false;
            CanHandleSessionChangeEvent = false;
            CanShutdown = true;
            CanStop = true;
        }

        private void StartService()
        {
            Bot.Start();
        }

        protected override void OnStart(string[] args)
        {

            botThread = new Thread(StartService) { IsBackground = true };
            botThread.Start();

            base.OnStart(args);
        }


        protected override void OnStop()
        {
            Bot.RfcQuit("Service shut down", Priority.Low);
            while (Bot.IsConnected)
            {
                Thread.Sleep(100);
            }
            base.OnStop();
        }

        protected override void OnShutdown()
        {
            Bot.RfcQuit("Service shut down", Priority.Low);
            while (Bot.IsConnected)
            {
                Thread.Sleep(100);
            }
            base.OnShutdown();
        }

        public static void WriteToLog(string message, EventLogEntryType eventLogEntryType = EventLogEntryType.Information)
        {
            if (!EventLog.SourceExists(HuffelpuffServiceName))
            {
                EventLog.CreateEventSource(HuffelpuffServiceName, "Application");
            }

            var eventLog = new EventLog { Source = HuffelpuffServiceName };
            eventLog.WriteEntry(message, eventLogEntryType);
        }
    }
}