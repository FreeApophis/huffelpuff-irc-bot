//
// NotifyListener.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Net;
using System.Net.Sockets;

namespace Mono.Ssdp.Internal
{
    internal class NotifyListener : MulticastReader
    {
        private readonly object mutex = new object ();
        private SsdpSocket socket;
        private Client client;
        
        public NotifyListener (Client client)
        {
            this.client = client;
        }
        
        public void Start ()
        {
            lock (mutex) {
                Stop ();
                
                socket = new SsdpSocket ();
                socket.Bind ();
                AsyncReadResult (socket);
            }
        }
        
        public void Stop ()
        {
            lock (mutex) {
                if (socket != null) {
                    socket.Close ();
                    socket = null;
                }
            }
        }
        
        internal override bool OnAsyncResultReceived (AsyncReceiveBuffer result)
        {
            try {
                HttpDatagram dgram = HttpDatagram.Parse (result.Buffer);
                if (dgram == null || dgram.Type != HttpDatagramType.Notify) {
                    return true;
                }
                
                string nts = dgram.Headers.Get ("NTS");
                string usn = dgram.Headers.Get ("USN");
                string nt = dgram.Headers.Get ("NT");
                
                if (String.IsNullOrEmpty (nts) || String.IsNullOrEmpty (usn)) {
                    return true;
                }
                
                if (!client.ServiceTypeRegistered (nt)) {
                    return true;
                }
                
                if (nts == Protocol.SsdpAliveNts) {
                    try {
                        if (!client.ServiceCache.Update (usn, dgram)) {
                            client.ServiceCache.Add (new BrowseService (dgram, true));
                        }
                    } catch (Exception e) {
                        Log.Exception ("Invalid ssdp:alive NOTIFY", e);
                    }
                } else if (nts == Protocol.SsdpByeByeNts) {
                    client.ServiceCache.Remove (usn);
                }
            } catch (Exception e) {
                Log.Exception ("Invalid HTTPMU/NOTIFY datagram", e);
            }
            
            return true;
        }
    }
}
