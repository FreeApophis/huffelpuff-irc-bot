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

using System;
using System.Net;

namespace Huffelpuff
{
	/// <summary>
	/// Description of Tools.
	/// </summary>
	public class Tools
	{
		public static bool RunOnMono() {
    		Type t = Type.GetType ("Mono.Runtime");
           	if (t != null)
            		Console.WriteLine ("Runtime: Mono VM v" + System.Environment.Version);
       		else
           		Console.WriteLine ("Runtime: (unkown) MS.NET? v" + System.Environment.Version);
       		       		
       		Console.WriteLine("OS     : " + System.Environment.OSVersion.VersionString);
       		Console.WriteLine("CWD    : " + System.Environment.CurrentDirectory);
       		Console.WriteLine("Machine: " + System.Environment.MachineName);
       		Console.WriteLine("CPUs   : " + System.Environment.ProcessorCount);
       		Console.WriteLine("User   : " + System.Environment.UserName);
       		
       		return (t != null);
       		
		}
		
		public static IPAddress[] getMyIPs()
		{
		    
			IPHostEntry IPHost = Dns.GetHostEntry(Dns.GetHostName());		
			foreach(IPAddress addr in IPHost.AddressList) {
				Console.WriteLine(addr.ToString());
			}
			return IPHost.AddressList;
		}

	
	}
}
