// Main.cs created with MonoDevelop
// User: apophis at 6:17 PM 10/5/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;
using System.Threading;

using Meebey.SmartIrc4net;

namespace Huffelpuff
{
	class Engine
	{
		public static void Main(string[] args)
		{
			Tools.RunOnMono();
			Tools.getMyIPs();
			IrcBot bot = new IrcBot();
			bot.Start();  /*blocking*/
		}	
	}
}
