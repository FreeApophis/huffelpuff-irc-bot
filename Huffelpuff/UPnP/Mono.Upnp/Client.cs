//
// Client.cs
//
// Author:
//   Scott Peterson <lunchtimemama@gmail.com>
//
// Copyright (C) 2008 S&S Black Ltd.
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
using System.Collections.Generic;

using Mono.Ssdp;
using Mono.Upnp.Internal;
using Mono.Upnp.Xml;

namespace Mono.Upnp
{
    public class Client : IDisposable
    {
        static WeakReference static_deserializer = new WeakReference (null);
        
        readonly Dictionary<DeviceAnnouncement, DeviceAnnouncement> devices =
            new Dictionary<DeviceAnnouncement, DeviceAnnouncement> ();
        readonly Dictionary<ServiceAnnouncement, ServiceAnnouncement> services =
            new Dictionary<ServiceAnnouncement, ServiceAnnouncement> ();
        readonly Dictionary<string, Root> descriptions =
            new Dictionary<string, Root> ();

        readonly Mono.Ssdp.Client client = new Mono.Ssdp.Client ();
        DeserializerFactory deserializer_facotry;

        public Client ()
            : this (null)
        {
        }
        
        public Client (DeserializerFactory deserializerFactory)
        {
            DeserializerFactory = deserializerFactory ?? new DeserializerFactory ();
            client.ServiceAdded += ClientServiceAdded;
            client.ServiceRemoved += ClientServiceRemoved;
        }
        
        public DeserializerFactory DeserializerFactory {
            get { return deserializer_facotry; }
            set {
                if (value == null) throw new ArgumentNullException ("value");
                deserializer_facotry = value;
            }
        }
        
        public event EventHandler<DeviceEventArgs> DeviceAdded;
        public event EventHandler<DeviceEventArgs> DeviceRemoved;
        public event EventHandler<ServiceEventArgs> ServiceAdded;
        public event EventHandler<ServiceEventArgs> ServiceRemoved;

        public void BrowseAll ()
        {
            client.BrowseAll ();
        }

        public void Browse (TypeInfo type)
        {
            client.Browse (type.ToString ());
        }
        
        XmlDeserializer Deserializer {
            get {
                if (!static_deserializer.IsAlive) {
                    static_deserializer.Target = new XmlDeserializer ();
                }
                return (XmlDeserializer)static_deserializer.Target;
            }
        }

        void ClientServiceAdded (object sender, Mono.Ssdp.ServiceArgs args)
        {
            ClientServiceEvent (args,
                (device) => {
                    if (!devices.ContainsKey (device)) {
                        OnDeviceAdded (new DeviceEventArgs (device, UpnpOperation.Added));
                        devices.Add (device, device);
                    }
                },
                (service) => {
                    if (!services.ContainsKey (service)) {
                        OnServiceAdded (new ServiceEventArgs (service, UpnpOperation.Added));
                        services.Add (service, service);
                    }
                }
            );
        }

        void ClientServiceRemoved (object sender, Mono.Ssdp.ServiceArgs args)
        {
            ClientServiceEvent (args,
                (device) => {
                    if (devices.ContainsKey (device)) {
                        device.Dispose ();
                        OnDeviceRemoved (new DeviceEventArgs (device, UpnpOperation.Removed));
                        devices.Remove (device);
                    }
                },
                (service) => {
                    if (services.ContainsKey (service)) {
                        service.Dispose ();
                        OnServiceRemoved (new ServiceEventArgs (service, UpnpOperation.Removed));
                        services.Remove (service);
                    }
                }
            );
        }

        void ClientServiceEvent (Mono.Ssdp.ServiceArgs args,
                                 Action<DeviceAnnouncement> deviceHandler,
                                 Action<ServiceAnnouncement> serviceHandler)
        {
            if (!args.Usn.StartsWith ("uuid:")) {
                return;
            }

            var colon = args.Usn.IndexOf (':', 5);
            var usn = colon == -1 ? args.Usn : args.Usn.Substring (0, colon);

            if (args.Usn.Contains (":device:")) {
                var type = new DeviceType (args.Service.ServiceType);
                var device = new DeviceAnnouncement (this, type, usn, args.Service.Locations);
                deviceHandler (device);
            } else if (args.Usn.Contains (":service:")) {
                var type = new ServiceType (args.Service.ServiceType);
                var service = new ServiceAnnouncement (this, type, usn, args.Service.Locations);
                serviceHandler (service);
            }
        }
        
        protected virtual void OnDeviceAdded (DeviceEventArgs e)
        {
            OnEvent (DeviceAdded, e);
        }

        protected virtual void OnServiceAdded (ServiceEventArgs e)
        {
            OnEvent (ServiceAdded, e);
        }

        protected virtual void OnDeviceRemoved (DeviceEventArgs e)
        {
            OnEvent (DeviceRemoved, e);
        }

        protected virtual void OnServiceRemoved (ServiceEventArgs e)
        {
            OnEvent (ServiceRemoved, e);
        }
        
        void OnEvent<T> (EventHandler<T> handler, T e) where T : EventArgs
        {
            if (handler != null) {
                handler (this, e);
            }
        }

        internal Service GetService (ServiceAnnouncement announcement)
        {
            foreach (var uri in announcement.Locations) {
                if (descriptions.ContainsKey (uri)) {
                    var description = GetService (announcement, descriptions[uri].RootDevice);
                    if (description != null && !description.IsDisposed) {
                        return description;
                    }
                }
                try {
                    var root = DeserializerFactory.CreateDeserializer (Deserializer).DeserializeRoot (new Uri (uri));
                    if (root == null) {
                        continue;
                    }
                    descriptions[uri] = root;
                    return GetService (announcement, root.RootDevice);
                } catch (Exception e) {
                    Log.Exception (string.Format ("There was a problem fetching the description at {0}.", uri), e);
                }
            }
            return null;
        }

        internal Device GetDevice (DeviceAnnouncement announcement)
        {
            foreach (var uri in announcement.Locations) {
                if (descriptions.ContainsKey (uri)) {
                    var description = GetDevice (announcement, descriptions[uri].RootDevice);
                    if (description != null && !description.IsDisposed) {
                        return description;
                    }
                }
                try {
                    var root = DeserializerFactory.CreateDeserializer (Deserializer).DeserializeRoot (new Uri (uri));
                    if (root == null) {
                        continue;
                    }
                    descriptions[uri] = root;
                    return GetDevice (announcement, root.RootDevice);
                } catch (Exception e) {
                    Log.Exception (string.Format ("There was a problem fetching the description at {0}.", uri), e);
                }
            }
            return null;
        }

        Service GetService (ServiceAnnouncement announcement, Device device)
        {
            foreach (var description in device.Services) {
                if (device.Udn == announcement.DeviceUdn && announcement.Type == description.Type) {
                    return description;
                }
            }
            foreach (var childDevice in device.Devices) {
                var description = GetService (announcement, childDevice);
                if (description != null) {
                    return description;
                }
            }
            return null;
        }

        Device GetDevice (DeviceAnnouncement announcement, Device device)
        {
            if (device.Type == announcement.Type && device.Udn == announcement.Udn) {
                return device;
            }
            foreach (var childDevice in device.Devices) {
                var description = GetDevice (announcement, childDevice);
                if (description != null) {
                    return description;
                }
            }
            return null;
        }
        
        public void Dispose ()
        {
            // TODO proper dispose pattern
            client.Dispose ();
        }
    }
}
