/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 04.06.2009 17:20
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
using System.Text.RegularExpressions;
using System.Xml;

namespace Huffelpuff.UPnP
{
    /// <summary>
    /// Description of Class1.
    /// </summary>
    public class UPnPDeviceFactory
    {
        public UPnPDeviceFactory() {
            
        }
        
        
        private Regex locationMatch = new Regex("(?<=location:).*(?=(\n|\r\n))", RegexOptions.IgnoreCase);
        
        public UPnPDevice GetDevice(string response) {
            
            try {
                Uri location = new Uri(locationMatch.Match(response).Value);

                XmlDocument desc = new XmlDocument();
                desc.Load(WebRequest.Create(location).GetResponse().GetResponseStream());
                
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(desc.NameTable);
                nsMgr.AddNamespace("ns", "urn:schemas-upnp-org:device-1-0");
                
                XmlNode deviceType = desc.SelectSingleNode("/ns:root/ns:device/ns:deviceType", nsMgr);
                
                //Console.WriteLine(deviceType.InnerText);
                
                if(deviceType.InnerText.Contains("InternetGatewayDevice")) {
                    return new UPnPGateway(response, desc);
                } else if(deviceType.InnerText.Contains("MediaServer")) {
                    return new UPnPMediaServer(response, desc);
                } else {
                    return new UPnPUknownDevice(response, desc);
                }
                
                
                
                
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
