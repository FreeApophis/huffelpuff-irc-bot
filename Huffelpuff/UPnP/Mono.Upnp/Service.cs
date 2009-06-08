//
// Service.cs
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

using Mono.Upnp.Control;
using Mono.Upnp.Internal;
using Mono.Upnp.Xml;

namespace Mono.Upnp
{
    public class Service<T> : Service
    {
        protected Service (T service)
            : this (null, null, service)
        {
        }
        
        public Service (ServiceType type, string id, T service)
            : base (type, id, ServiceControllerBuilder.Build<T> (service))
        {
        }
    }
    
    [XmlType ("service", Protocol.DeviceSchema)]
    public class Service : Description
    {
        ServiceController controller;

        protected internal Service (Deserializer deserializer)
            : base (deserializer)
        {
        }
        
        protected Service (ServiceController controller)
            : this (null, null, controller)
        {
        }
        
        public Service (ServiceType type, string id, ServiceController controller)
        {
            if (controller == null) throw new ArgumentNullException ("controller");
            
            this.controller = controller;
            Type = type;
            Id = id;
        }
        
        [XmlElement ("serviceType", Protocol.DeviceSchema)]
        public virtual ServiceType Type { get; protected set; }
        
        [XmlElement ("serviceId", Protocol.DeviceSchema)]
        public virtual string Id { get; protected set; }
        
        [XmlElement ("SCPDURL", Protocol.DeviceSchema)]
        protected virtual string ScpdUrlFragment {
            get { return CollapseUrl (ScpdUrl); }
            set { ScpdUrl = ExpandUrl (value); }
        }
        
        public virtual Uri ScpdUrl { get; protected set; }
        
        [XmlElement ("controlURL", Protocol.DeviceSchema)]
        protected virtual string ControlUrlFragment {
            get { return CollapseUrl (ControlUrl); }
            set { ControlUrl = ExpandUrl (value); }
        }   
        
        public virtual Uri ControlUrl { get; protected set; }
        
        [XmlElement ("eventSubURL", Protocol.DeviceSchema)]
        protected virtual string EventUrlFragment {
            get { return CollapseUrl (EventUrl); }
            set { EventUrl = ExpandUrl (value); }
        }
        
        public virtual Uri EventUrl { get; protected set; }
        
        protected internal virtual void Initialize (XmlSerializer serializer, Root root, Uri serviceUrl)
        {
            Initialize (root);
            if (serviceUrl == null) throw new ArgumentNullException ("serviceUrl");
            if (controller == null) throw new InvalidOperationException ("The service was created for deserialization and cannot be initialized.");
            
            ScpdUrl = new Uri (serviceUrl, "scpd/");
            ControlUrl = new Uri (serviceUrl, "control/");
            EventUrl = new Uri (serviceUrl, "event/");
            controller.Initialize (serializer, this);
        }
        
        protected internal virtual void Start ()
        {
            if (controller == null) throw new InvalidOperationException ("The service was created for deserialization and cannot be started.");
            
            controller.Start ();
        }
        
        protected internal virtual void Stop ()
        {
            if (controller == null) throw new InvalidOperationException ("The service was created for deserialization and cannot be stopped.");
            
            controller.Stop ();
        }

        public ServiceController GetController ()
        {
            if (controller == null) {
                if (IsDisposed) {
                    throw new ObjectDisposedException (ToString (), "The service has gone off the network.");
                }
                controller = Deserializer.GetServiceController (this);
            }
            
            return controller;
        }
        
        [XmlTypeDeserializer]
        protected virtual ServiceType DeserializeServiceType (XmlDeserializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            return new ServiceType (context.Reader.ReadElementContentAsString ());
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

        public override string ToString ()
        {
            return string.Format ("ServiceDescription {{ {0}, {1} }}", Id, Type);
        }
    }
}
