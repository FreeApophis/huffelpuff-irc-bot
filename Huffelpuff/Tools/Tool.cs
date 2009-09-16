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
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Huffelpuff;

namespace Huffelpuff.Tools
{
    /// <summary>
    /// Description of Tools.
    /// </summary>
    public class Tool
    {
        public static bool RunOnMono() {
            Type t = Type.GetType ("Mono.Runtime");
            if (t != null)
                Log.Instance.Log("Runtime: Mono VM v" + System.Environment.Version);
            else
                Log.Instance.Log("Runtime: (unkown) MS.NET? v" + System.Environment.Version);
            
            Log.Instance.Log("OS     : " + System.Environment.OSVersion.VersionString);
            Log.Instance.Log("CWD    : " + System.Environment.CurrentDirectory);
            Log.Instance.Log("Machine: " + System.Environment.MachineName);
            Log.Instance.Log("CPUs   : " + System.Environment.ProcessorCount);
            Log.Instance.Log("User   : " + System.Environment.UserName);
            
            return (t != null);
            
        }
        
        private static IPAddress localIP;
        
        public static IPAddress LocalIP {
            get { return localIP; }
        }
        
        public static IPAddress TryGetExternalIP()
        {
            if (PersistentMemory.Instance.GetValue("external_ip") != null) {
                
                // external IP in settings overides any self detection
                return System.Net.IPAddress.Parse(PersistentMemory.Instance.GetValue("external_ip"));
            } else {
                IPAddress currentBest = null;
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach(NetworkInterface adapter in  nics)
                {
                    if(adapter.OperationalStatus != OperationalStatus.Up) {
                        continue;
                    }
                    
                    IPInterfaceProperties prop = adapter.GetIPProperties();
                    foreach(IPAddressInformation ip in prop.UnicastAddresses)
                    {
                        if(ip.Address.AddressFamily != AddressFamily.InterNetwork) {
                            continue;
                        }
                        
                        if(!IsLocalHostIP(ip.Address)) {
                            if (currentBest == null) {
                                currentBest = ip.Address;
                            } else if (!IsLocalIP(ip.Address)) {
                                currentBest = ip.Address;
                            }
                        }
                    }
                    
                }
                
                if(IsLocalIP(currentBest)) {
                    // we will use this for redirect ports;
                    localIP = currentBest;
                    if (UPnP.NAT.Discover()) {
                        currentBest = UPnP.NAT.GetExternalIP();
                    } else {
                        // TODO: no upnp support on a local adress :: ugly
                        // further ideas :: userip / whatismyip.com /manual port forward
                    }
                }
                return currentBest;
            }
        }

        /// <summary>
        /// Checks if a ipv4 address is a localhost address
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsLocalHostIP(IPAddress ip) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                byte[] ipBytes = ip.GetAddressBytes();
                if ((ipBytes[0] == 127) && (ipBytes[1] == 0) && (ipBytes[2] == 0)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a ipv4 address is a local network address / zeroconf
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsLocalIP(IPAddress ip) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                byte[] ipBytes = ip.GetAddressBytes();

                //    10.0.0.0 bis  10.255.255.255 :: 10.0.0.0/8       (1 Class C)
                if (ipBytes[0] == 10) {
                    return true;
                }
                
                //  172.16.0.0 bis  172.31.255.255 :: 172.16.0.0/12   (16 Class B)
                if ((ipBytes[0] == 172) && (ipBytes[1] > 15) && (ipBytes[1] < 32)) {
                    return true;
                }
                
                // 192.168.0.0 bis 192.168.255.255 :: 192.168/16     (256 Class C)
                if ((ipBytes[0] == 192) && (ipBytes[1] == 168)) {
                    return true;
                }

                // 169.254.0.0 bis 169.254.255.255 :: 169.254.0.0/16    (Zeroconf)
                if ((ipBytes[0] == 169) && (ipBytes[1] == 255)) {
                    return true;
                }
            }
            return false;
        }
    }
}
