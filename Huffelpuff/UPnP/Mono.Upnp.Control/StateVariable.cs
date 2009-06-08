//
// StateVariable.cs
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

using Mono.Upnp.Internal;
using Mono.Upnp.Xml;

namespace Mono.Upnp.Control
{
    [XmlType ("stateVariable", Protocol.ServiceSchema)]
    public class StateVariable : XmlAutomatable
    {
        readonly ServiceController controller;
        readonly StateVariableEventer eventer;
        IList<string> allowed_values;
        //event EventHandler<StateVariableChangedArgs<string>> changed;
        
        protected StateVariable ()
        {
        }

        protected internal StateVariable (ServiceController service)
        {
            if (service == null) throw new ArgumentNullException ("service");

            this.controller = service;
        }
        
        public StateVariable (string name, string dataType, StateVariableEventer eventer)
            : this (name, dataType, eventer, false)
        {
        }
        
        public StateVariable (string name, string dataType, StateVariableEventer eventer, bool isMulticast)
            : this (name, dataType, null, true, isMulticast)
        {
            if (eventer == null) throw new ArgumentNullException ("eventer");
            
            this.eventer = eventer;
            eventer.StateVariableUpdated += OnStateVariableUpdated;
        }
        
        public StateVariable (string name, string dataType)
            : this (name, dataType, null, false, false)
        {
        }
        
        public StateVariable (string name, string dataType, string defaultValue)
            : this (name, dataType, defaultValue, false, false)
        {
        }
        
        public StateVariable (string name, IEnumerable<string> allowedValues)
            : this (name, allowedValues, null)
        {
        }
        
        public StateVariable (string name, IEnumerable<string> allowedValues, string defaultValue)
            : this (name, "string", defaultValue, false, false)
        {
            allowed_values = Helper.MakeReadOnlyCopy (allowedValues);
        }
        
        public StateVariable (string name, string dataType, AllowedValueRange allowedValueRange)
            : this (name, dataType, allowedValueRange, null)
        {
        }
        
        public StateVariable (string name, string dataType, AllowedValueRange allowedValueRange, string defaultValue)
            : this (name, dataType, defaultValue, false, false)
        {
            AllowedValueRange = allowedValueRange;
        }
        
        StateVariable (string name, string dataType, string defaultValue, bool sendsEvents, bool isMulticast)
        {
            if (name == null) throw new ArgumentNullException ("name");
            if (dataType == null) throw new ArgumentNullException ("dataType");
            
            Name = name;
            DataType = dataType;
            DefaultValue = defaultValue;
            SendsEvents = sendsEvents;
            IsMulticast = isMulticast;
        }
        
        [XmlElement ("name", Protocol.ServiceSchema)]
        public virtual string Name { get; protected set; }

        [XmlElement ("dataType", Protocol.ServiceSchema)]
        public virtual string DataType { get; protected set; }

        [XmlAttribute ("sendEvents", OmitIfNull = true)]
        protected virtual string SendsEventsString {
            get { return SendsEvents ? "yes" : null; }
            set { SendsEvents = value == "yes"; }
        }
        
        public bool SendsEvents { get; protected set; }
        
        public virtual bool IsMulticast { get; protected set; }

        [XmlAttribute ("multicast", OmitIfNull = true)]
        protected virtual string IsMulticastString {
            get { return IsMulticast ? "yes" : null; }
            set { IsMulticast = value == "yes"; }
        }
        
        [XmlElement ("defaultValue", Protocol.ServiceSchema, OmitIfNull = true)]
        public string DefaultValue { get; protected set; }

        [XmlArray ("allowedValueList", Protocol.ServiceSchema, OmitIfNull = true)]
        [XmlArrayItem ("allowedValue", Protocol.ServiceSchema)]
        protected virtual ICollection<string> AllowedValueCollection {
            get { return allowed_values; }
        }
        
        public IEnumerable<string> AllowedValues {
            get { return allowed_values; }
        }

        [XmlElement ("allowedValueRange", Protocol.ServiceSchema, OmitIfNull = true)]
        public virtual AllowedValueRange AllowedValueRange { get; protected set; }
        
        protected virtual void OnStateVariableUpdated (object sender, StateVariableChangedArgs<string> args)
        {
        }
        
        protected override void DeserializeAttribute (XmlDeserializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            context.AutoDeserializeAttribute (this);
        }
        
        protected override void DeserializeElement (XmlDeserializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            if (context.Reader.LocalName == "allowedValueList" && context.Reader.NamespaceURI == Protocol.ServiceSchema) {
                allowed_values = new List<string> ();
            }
            context.AutoDeserializeElement (this);
        }

        protected override void SerializeSelfAndMembers (XmlSerializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            context.AutoSerializeObjectAndMembers (this);
        }

        protected override void SerializeMembersOnly (Mono.Upnp.Xml.XmlSerializationContext context)
        {
            if (context == null) throw new ArgumentNullException ("context");
            
            context.AutoSerializeMembersOnly (this);
        }
    }
}
