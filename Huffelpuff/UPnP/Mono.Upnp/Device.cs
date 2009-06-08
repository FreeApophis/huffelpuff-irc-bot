//
// Device.cs
//
// Author:
//   Scott Peterson <lunchtimemama@gmail.com>
//
// Copyright (C) 2009 S&S Black Ltd.
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
using System.Collections.ObjectModel;

using Mono.Upnp.Internal;
using Mono.Upnp.Xml;

namespace Mono.Upnp
{
    [XmlType ("device", Protocol.DeviceSchema)]
    public class Device : Description, IXmlDeserializable
    {
        IList<Device> devices;
        IList<Service> services;
        IList<Icon> icons;
        
        public Device (DeviceSettings settings)
            : this (settings, null)
        {
        }
        
        protected internal Device (DeviceSettings settings, IEnumerable<Device> devices)
            : this (devices, GetServices (settings), GetIcons (settings))
        {
            Type = settings.Type;
            Udn = settings.Udn;
            FriendlyName = settings.FriendlyName;
            Manufacturer = settings.Manufacturer;
            ManufacturerUrl = settings.ManufacturerUrl;
            ModelDescription = settings.ModelDescription;
            ModelName = settings.ModelName;
            ModelNumber = settings.ModelNumber;
            ModelUrl = settings.ModelUrl;
            SerialNumber = settings.SerialNumber;
            Upc = settings.Upc;
        }
        
        protected internal Device (IEnumerable<Device> devices, IEnumerable<Service> services, IEnumerable<Icon> icons)
        {
            this.devices = Helper.MakeReadOnlyCopy (devices);
            this.services = Helper.MakeReadOnlyCopy (services);
            this.icons = Helper.MakeReadOnlyCopy (icons);
        }
        
        protected internal Device (Deserializer deserializer)
            : base (deserializer)
        {
            devices = new List<Device> ();
            services = new List<Service> ();
            icons = new List<Icon> ();
        }
        
        static IEnumerable<Service> GetServices (DeviceSettings settings)
        {
            if (settings == null) throw new ArgumentNullException ("settings");
            
            return settings.Services;
        }
        
        static IEnumerable<Icon> GetIcons (DeviceSettings settings)
        {
            return settings.Icons;
        }
        
        [XmlArray ("iconList", Protocol.DeviceSchema)]
        protected virtual ICollection<Icon> IconList {
            get { return icons; }
        }
        
        public IEnumerable<Icon> Icons {
            get { return icons; }
        }
        
        [XmlArray ("serviceList", Protocol.DeviceSchema)]
        protected virtual ICollection<Service> ServiceList {
            get { return services; }
        }
        
        public IEnumerable<Service> Services {
            get { return services; }
        }
        
        [XmlArray ("deviceList", Protocol.DeviceSchema, OmitIfEmpty = true)]
        protected virtual ICollection<Device> DeviceList {
            get { return devices; }
        }
        
        public IEnumerable<Device> Devices {
            get { return devices; }
        }
        
        [XmlElement ("deviceType", Protocol.DeviceSchema)]
        public virtual DeviceType Type { get; protected set; }
        
        [XmlElement ("friendlyName", Protocol.DeviceSchema)]
        public virtual string FriendlyName { get; protected set; }
        
        [XmlElement ("manufacturer", Protocol.DeviceSchema)]
        public virtual string Manufacturer { get; protected set; }
        
        [XmlElement ("manufacturerURL", Protocol.DeviceSchema, OmitIfNull = true)]
        public virtual Uri ManufacturerUrl { get; protected set; }
        
        [XmlElement ("modelDescription", Protocol.DeviceSchema, OmitIfNull = true)]
        public virtual string ModelDescription { get; protected set; }
        
        [XmlElement ("modelName", Protocol.DeviceSchema)]
        public virtual string ModelName { get; protected set; }
        
        [XmlElement ("modelNumber", Protocol.DeviceSchema, OmitIfNull = true)]
        public virtual string ModelNumber { get; protected set; }
        
        [XmlElement ("modelURL", Protocol.DeviceSchema, OmitIfNull = true)]
        public virtual Uri ModelUrl { get; protected set; }
        
        [XmlElement ("serialNumber", Protocol.DeviceSchema, OmitIfNull = true)]
        public virtual string SerialNumber { get; protected set; }
        
        [XmlElement ("UDN", Protocol.DeviceSchema)]
        public virtual string Udn { get; protected set; }
        
        [XmlElement ("UPC", Protocol.DeviceSchema, OmitIfNull = true)]
        public virtual string Upc { get; protected set; }

        protected internal virtual void Initialize (XmlSerializer serializer, Root root, Uri deviceUrl)
        {
            if (deviceUrl == null) throw new ArgumentNullException ("deviceUrl");
            if (Deserializer != null) throw new InvalidOperationException ("The device was constructed for deserialization and cannot be initalized. Use one of the other constructors.");
            
            for (var i = 0; i < devices.Count; i++) {
                devices[i].Initialize (serializer, root, new Uri (deviceUrl, string.Format ("device/{0}/", i)));
            }
            
            for (var i = 0; i < services.Count; i++) {
                services[i].Initialize (serializer, root, new Uri (deviceUrl, string.Format ("service/{0}/", i)));
            }
            
            for (var i = 0; i < icons.Count; i++) {
                icons[i].Initialize (root, new Uri (deviceUrl, string.Format ("icon/{0}/", i)));
            }
        }

        protected internal virtual void Start ()
        {
            foreach (var device in devices) {
                device.Start ();
            }
            
            foreach (var service in services) {
                service.Start ();
            }
            
            foreach (var icon in icons) {
                icon.Start ();
            }
        }

        protected internal virtual void Stop ()
        {
            foreach (var device in devices) {
                device.Stop ();
            }
            
            foreach (var service in services) {
                service.Stop ();
            }
            
            foreach (var icon in icons) {
                icon.Stop ();
            }
        }
        
        [XmlTypeDeserializer]
        protected virtual DeviceType DeserializeDeviceType (XmlDeserializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            return new DeviceType (context.Reader.ReadElementContentAsString ());
        }
        
        [XmlTypeDeserializer]
        protected virtual Device DeserializeDevice (XmlDeserializationContext context)
        {
            return Deserializer != null ? Deserializer.DeserializeDevice (context) : null;
        }
        
        [XmlTypeDeserializer]
        protected virtual Service DeserializeService (XmlDeserializationContext context)
        {
            return Deserializer != null ? Deserializer.DeserializeService (context) : null;
        }
        
        [XmlTypeDeserializer]
        protected virtual Icon DeserializeIcon (XmlDeserializationContext context)
        {
            return Deserializer != null ? Deserializer.DeserializeIcon (context) : null;
        }
        
        void IXmlDeserializable.Deserialize (XmlDeserializationContext context)
        {
            Deserialize (context);
            devices = new ReadOnlyCollection<Device> (devices);
            services = new ReadOnlyCollection<Service> (services);
            icons = new ReadOnlyCollection<Icon> (icons);
        }
        
        void IXmlDeserializable.DeserializeAttribute (XmlDeserializationContext context)
        {
            DeserializeAttribute (context);
        }
        
        void IXmlDeserializable.DeserializeElement (XmlDeserializationContext context)
        {
            DeserializeElement (context);
        }

        protected override void DeserializeElement (XmlDeserializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            context.AutoDeserializeElement (this);
        }
        
        protected override void SerializeSelfAndMembers (XmlSerializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            context.AutoSerializeObjectAndMembers (this);
        }
        
        protected override void SerializeMembersOnly (XmlSerializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            context.AutoSerializeMembersOnly (this);
        }
    }
}
