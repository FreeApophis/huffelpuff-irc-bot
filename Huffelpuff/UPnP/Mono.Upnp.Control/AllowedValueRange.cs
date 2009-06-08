//
// AllowedValueRange.cs
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

using Mono.Upnp.Internal;
using Mono.Upnp.Xml;

namespace Mono.Upnp.Control
{
    [XmlType ("allowedValueRange", Protocol.ServiceSchema)]
    public sealed class AllowedValueRange
    {
        AllowedValueRange ()
        {
        }
        
        public AllowedValueRange (string minimum, string maximum)
            : this (minimum, maximum, null)
        {
        }
        
        public AllowedValueRange (string minimum, string maximum, string step)
        {
            if (minimum == null) throw new ArgumentNullException ("minimum");
            if (maximum == null) throw new ArgumentNullException ("maximum");
            
            Minimum = minimum;
            Maximum = maximum;
            Step = step;
        }
        
        [XmlElement ("minimum", Protocol.ServiceSchema)]
        public string Minimum { get; private set; }
        
        [XmlElement ("maximum", Protocol.ServiceSchema)]
        public string Maximum { get; private set; }
        
        [XmlElement ("step", Protocol.ServiceSchema, OmitIfNull = true)]
        public string Step { get; private set; }
        
        internal IComparable Min { get; set; }
        
        internal IComparable Max { get; set; }
    }
}
