/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 08.10.2008
 * Zeit: 00:59
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
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
            		Console.WriteLine ("You are running with the Mono VM");
       		else
           		Console.WriteLine ("You are running something else");
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
