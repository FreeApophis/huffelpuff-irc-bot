/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 04.06.2009 14:29
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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Huffelpuff.UPnP
{
    /// <summary>
    /// Description of UPnPClient.
    /// </summary>
    public class UPnPClient
    {
        private List<UPnPDevice> devices = new List<UPnPDevice>();
        
        public List<UPnPDevice> Devices {
            get { return devices; }
        }
        
        private TimeSpan _timeout = new TimeSpan(0, 0, 0, 3); 
        
        private UPnPDeviceFactory factory = new UPnPDeviceFactory();
        
        public UPnPClient()
        {
        }
        
        public void Discover() {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            string req = "M-SEARCH * HTTP/1.1\r\n" +
                         "HOST: 239.255.255.250:1900\r\n" +
                         "ST:upnp:rootdevice\r\n" +
                         "MAN:\"ssdp:discover\"\r\n" +
                         "MX:3\r\n\r\n";
            byte[] data = Encoding.ASCII.GetBytes(req);
            IPEndPoint ipe = new IPEndPoint(IPAddress.Broadcast, 1900);
            byte[] buffer = new byte[0x1000];

            DateTime start = DateTime.Now;

            do
            {
                s.SendTo(data, ipe);
                s.SendTo(data, ipe);
                s.SendTo(data, ipe);

                int length = 0;
                do
                {
                    length = s.Receive(buffer);
                    string resp = Encoding.ASCII.GetString(buffer, 0, length);
                    bool known = false;
                    
                    UPnPDevice device = factory.GetDevice(resp);
                    
                    foreach(UPnPDevice d in this.devices) {
                        if (d.Uuid == device.Uuid) {
                            known = true;
                        }
                    }
                    if(!known) {
                        devices.Add(device);
                    }
                } while (length > 0);
            } while (start.Subtract(DateTime.Now) < _timeout);
        }
    }
}
