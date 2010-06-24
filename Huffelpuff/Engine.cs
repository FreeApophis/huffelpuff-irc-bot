/*
 *  The Huffelpuff Irc Bot, versatile pluggable bot for IRC chats
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


using Huffelpuff.Utils;

namespace Huffelpuff
{
	#if !SERVICE
	class Engine
	{

		public static void Main(string[] args)
		{
			Tool.RunOnMono();
			var bot = new IrcBot();
			
			// check for basic settings
			PersistentMemory.Instance.GetValuesOrTodo("ServerHost");
			PersistentMemory.Instance.GetValuesOrTodo("ServerPort");
			PersistentMemory.Instance.GetValueOrTodo("nick");
			PersistentMemory.Instance.GetValueOrTodo("realname");
			PersistentMemory.Instance.GetValueOrTodo("username");
			
			if(PersistentMemory.Todo) {
				PersistentMemory.Instance.Flush();
				Log.Instance.Log("Edit your config file: there are some TODOs left.", Level.Fatal);
				bot.Exit();
			}
			bot.Start();  /*blocking*/
		}
	}
	#else
	
	public class ServiceEngine : ServiceBase
	{
		public static string HuffelpuffServiceName = "Huffelpuff IRC Bot";
		private Thread botThread;
		private IrcBot bot;
		
		public ServiceEngine()
		{
			this.ServiceName = HuffelpuffServiceName;
			this.EventLog.Log = "Application";

			this.CanHandlePowerEvent = false;
			this.CanPauseAndContinue = false;
			this.CanShutdown = true;
			this.CanStop = true;
		}

		static void Main()
		{
			ServiceBase.Run(new ServiceEngine());
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		protected override void OnStart(string[] args)
		{
			Tool.RunOnMono();
			bot = new IrcBot();
			
			// check for basic settings
			PersistentMemory.Instance.GetValuesOrTodo("ServerHost");
			PersistentMemory.Instance.GetValuesOrTodo("ServerPort");
			PersistentMemory.Instance.GetValueOrTodo("nick");
			PersistentMemory.Instance.GetValueOrTodo("realname");
			PersistentMemory.Instance.GetValueOrTodo("username");
			
			if(PersistentMemory.Todo) {
				PersistentMemory.Instance.Flush();
				Log.Instance.Log("Edit your config file: there are some TODOs left.", Level.Fatal);
				bot.Exit();
			}
			
			botThread = new Thread(bot.Start);
			botThread.Start();
			
			base.OnStart(args);
		}


		protected override void OnStop()
		{
			bot.RfcQuit("Service shut down", Priority.Low);
			while(bot.IsConnected) {
				Thread.Sleep(100);
			}
			base.OnStop();
		}

		protected override void OnShutdown()
		{
			bot.RfcQuit("Service shut down", Priority.Low);
			while(bot.IsConnected) {
				Thread.Sleep(100);
			}
			base.OnShutdown();
		}
	}
	#endif
}
