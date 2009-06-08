// 
// Root.cs
//  
// Author:
//       Scott Peterson <lunchtimemama@gmail.com>
// 
// Copyright (c) 2009 Scott Peterson
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

using Mono.Upnp.Control;
using Mono.Upnp.Internal;
using Mono.Upnp.Xml;

namespace Mono.Upnp
{
    [XmlType ("root", Protocol.DeviceSchema)]
    public class Root : Description
    {
        protected internal Root (Deserializer deserializer, Uri url)
            : base (deserializer)
        {
            if (url == null) throw new ArgumentNullException ("url");
            
            UrlBase = url;
        }
        
        public Root (DeviceSettings rootDeviceSettings)
            : this (rootDeviceSettings, null)
        {
        }
        
        public Root (DeviceSettings rootDeviceSettings, IEnumerable<Device> embeddedDevices)
            : this (CreateRootDevice (rootDeviceSettings, embeddedDevices))
        {
        }
        
        protected Root (Device rootDevice)
        {
            RootDevice = rootDevice;
            SpecVersion = new SpecVersion (1, 1);
        }
        
        static Device CreateRootDevice (DeviceSettings rootDeviceSettings, IEnumerable<Device> embeddedDevices)
        {
            if (rootDeviceSettings == null) throw new ArgumentNullException ("rootDeviceSettings");
            
            return new Device (rootDeviceSettings, embeddedDevices);
        }
        
        [XmlAttribute ("configId")]
        public virtual string ConfigurationId { get; protected set; }
        
        [XmlElement ("specVersion", Protocol.DeviceSchema)]
        public virtual SpecVersion SpecVersion { get; protected set; }
        
        [DoNotSerialize]
        [XmlElement ("URLBase", Protocol.DeviceSchema)]
        public virtual Uri UrlBase { get; protected set; }
        
        [XmlElement ("device", Protocol.DeviceSchema)]
        public virtual Device RootDevice { get; protected set; }
        
        protected internal virtual void Initialize (XmlSerializer serializer, Uri url)
        {
            // TODO better error message
            if (url == null) throw new ArgumentNullException ("url");
            if (RootDevice == null) throw new InvalidOperationException ("The RootDevice is null.");
            
            UrlBase = url;
            // TODO clean this up, localize it
            RootDevice.Initialize (serializer, this, url);
        }
        
        protected internal virtual void Start ()
        {
            RootDevice.Start ();
        }
        
        protected internal virtual void Stop ()
        {
            RootDevice.Stop ();
        }
        
        [XmlTypeDeserializer]
        protected virtual Device DeserializeDevice (XmlDeserializationContext context)
        {
            return Deserializer != null ? Deserializer.DeserializeDevice (context) : null;
        }
        
        protected override void DeserializeAttribute (XmlDeserializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            context.AutoDeserializeAttribute (this);
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
