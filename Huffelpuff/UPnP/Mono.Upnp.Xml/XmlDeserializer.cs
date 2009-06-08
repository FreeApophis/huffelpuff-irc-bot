// 
// XmlDeserializer.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Mono.Upnp.Xml
{
    public delegate T Deserializer<T> (XmlDeserializationContext context);
    
    public sealed class XmlDeserializer
    {
        delegate void Action ();
        delegate object Deserializer (XmlDeserializationContext context);
        delegate object ItemDeserializer (object obj, XmlDeserializationContext context);
        delegate void ObjectDeserializer (object obj, XmlDeserializationContext context);
        
        class Deserializers
        {
            public Deserializer Deserializer;
            public ObjectDeserializer AutoDeserializer;
            public ObjectDeserializer AttributeDeserializer;
            public ObjectDeserializer AttributeAutoDeserializer;
            public ObjectDeserializer ElementDeserializer;
            public ObjectDeserializer ElementAutoDeserializer;
            public Dictionary<Type, MethodInfo> TypeDeserializers;
        }
        
        readonly Dictionary<Type, Deserializers> deserializers = new Dictionary<Type, Deserializers> ();
        
        public T Deserialize<T> (XmlReader reader)
        {
            return Deserialize<T> (reader, null);
        }
        
        public T Deserialize<T> (XmlReader reader, Deserializer<T> typeDeserializer)
        {
            if (reader == null) throw new ArgumentNullException ("reader");
            
            return DeserializeCore<T> (reader, typeDeserializer);
        }
        
        T DeserializeCore<T> (XmlReader reader, Deserializer<T> typeDeserializer)
        {
            var context = new XmlDeserializationContext (this, reader);
            if (typeDeserializer != null) {
                return typeDeserializer (context);
            } else {
                var deserializer = GetDeserializer (typeof (T));
                return (T) deserializer (context);
            }
        }
        
        internal void AutoDeserialize<T> (T obj, XmlDeserializationContext context)
        {
            var deserializer = GetAutoDeserializer (typeof (T));
            deserializer (obj, context);
        }
        
        internal void AutoDeserializeAttribute<T> (T obj, XmlDeserializationContext context)
        {
            var deserializer = GetAttributeAutoDeserializer (typeof (T));
            deserializer (obj, context);
        }
        
        internal void AutoDeserializeElement<T> (T obj, XmlDeserializationContext context)
        {
            var deserializer = GetElementAutoDeserializer (typeof (T));
            deserializer (obj, context);
        }
        
        Deserializers GetDeserializers (Type type)
        {
            if (!deserializers.ContainsKey (type)) {
                deserializers[type] = new Deserializers ();
            }
            return deserializers[type];
        }
        
        Deserializer GetDeserializer (Type type)
        {
            var deserializers = GetDeserializers (type);
            if (deserializers.Deserializer == null) {
                Deserializer deserializer = null;
                deserializers.Deserializer = context => deserializer (context);
                deserializer = CreateDeserializer (type, deserializers);
                deserializers.Deserializer = deserializer;
            }
            return deserializers.Deserializer;
        }
        
        Deserializer CreateDeserializer (Type type, Deserializers deserializers)
        {
            if (type == typeof (string)) {
                return context => context.Reader.ReadElementContentAsString ();
            } else if (type == typeof (int)) {
                return context => context.Reader.ReadElementContentAsInt ();
            } else if (type == typeof (double)) {
                return context => context.Reader.ReadElementContentAsDouble ();
            } else if (type == typeof (bool)) {
                return context => context.Reader.ReadElementContentAsBoolean ();
            } else if (type == typeof (long)) {
                return context => context.Reader.ReadElementContentAsLong ();
            } else if (type == typeof (float)) {
                return context => context.Reader.ReadElementContentAsFloat ();
            } else if (type == typeof (decimal)) {
                return context => context.Reader.ReadElementContentAsDecimal ();
            } else if (type == typeof (DateTime)) {
                return context => context.Reader.ReadElementContentAsDateTime ();
            } else if (type == typeof (Uri)) {
                return context => new Uri (context.Reader.ReadElementContentAsString ());
            } else if (type.IsEnum) {
                var map = GetEnumMap (type);
                return context => map[context.Reader.ReadElementContentAsString ()];
            } else {
                // TODO check for default ctor
                if (typeof (IXmlDeserializable).IsAssignableFrom (type)) {
                    return context => {
                        var obj = Activator.CreateInstance (type, true);
                        ((IXmlDeserializable)obj).Deserialize (context);
                        return obj;
                    };
                } else {
                    var deserializer = GetAutoDeserializer (type, deserializers);
                    return context => {
                        var obj = Activator.CreateInstance (type, true);
                        deserializer (obj, context);
                        return obj;
                    };
                }
            }
        }
        
        // TODO we could use a trie for this and save some memory
        static Dictionary<string, object> GetEnumMap (Type type)
        {
            var fields = type.GetFields (BindingFlags.Public | BindingFlags.Static);
            var dictionary = new Dictionary<string, object> (fields.Length);
            foreach (var field in fields) {
                var enum_attribute = field.GetCustomAttributes (typeof (XmlEnumAttribute), false);
                string name;
                if (enum_attribute.Length != 0) {
                    name = ((XmlEnumAttribute)enum_attribute[0]).Value;
                } else {
                    name = field.Name;
                }
                dictionary.Add (name, field.GetValue (null));
            }
            return dictionary;
        }
        
        ObjectDeserializer GetAutoDeserializer (Type type)
        {
            return GetAutoDeserializer (type, GetDeserializers (type));
        }
        
        ObjectDeserializer GetAutoDeserializer (Type type, Deserializers deserializers)
        {
            if (deserializers.AutoDeserializer == null) {
                deserializers.AutoDeserializer = CreateAutoDeserializer (type, deserializers);
            }
            return deserializers.AutoDeserializer;
        }
        
        ObjectDeserializer CreateAutoDeserializer (Type type, Deserializers deserializers)
        {
            var attribute_deserializer = GetAttributeDeserializer (type, deserializers);
            var element_deserializer = GetElementDeserializer (type, deserializers);
            return (obj, context) => {
                try {
                    var depth = context.Reader.Depth;
                    while (context.Reader.MoveToNextAttribute ()) {
                        try {
                            attribute_deserializer (obj, context);
                        } catch (Exception exception) {
                        }
                    }
                    while (context.Reader.Read () && context.Reader.NodeType == XmlNodeType.Element && context.Reader.Depth > depth) {
                        var element_reader = context.Reader.ReadSubtree ();
                        element_reader.Read ();
                        try {
                            element_deserializer (obj, new XmlDeserializationContext (this, element_reader));
                        } catch (Exception exception) {
                        } finally {
                            element_reader.Close ();
                        }
                    }
                } catch {
                }
            };
        }
        
        Dictionary<Type, MethodInfo> GetTypeDeserializers (Type type)
        {
            Dictionary<Type, MethodInfo> type_deserializers = null;
            type_deserializers = ProcessTypeDeserializers (type, BindingFlags.Instance | BindingFlags.Public, type_deserializers);
            type_deserializers = ProcessTypeDeserializers (type, BindingFlags.Instance | BindingFlags.NonPublic, type_deserializers);
            return type_deserializers;
        }
        
        Dictionary<Type, MethodInfo> ProcessTypeDeserializers (Type type, BindingFlags flags, Dictionary<Type, MethodInfo> typeDeserializers)
        {
            foreach (var method in type.GetMethods (flags)) {
                var attributes =  method.GetCustomAttributes (typeof (XmlTypeDeserializerAttribute), false);
                if (attributes.Length != 0) {
                    if (method.ReturnType == typeof (void)) {
                        // TODO throw
                        continue;
                    }
                    var parameters = method.GetParameters ();
                    if (parameters.Length != 1 || parameters[0].ParameterType != typeof (XmlDeserializationContext)) {
                        // TODO throw
                        continue;
                    }
                    if (typeDeserializers == null) {
                        typeDeserializers = new Dictionary<Type, MethodInfo> ();
                    } else if (typeDeserializers.ContainsKey (method.ReturnType)) {
                        // TODO throw
                    }
                    typeDeserializers[method.ReturnType] = method;
                }
            }
            return typeDeserializers;
        }
        
        ObjectDeserializer GetAttributeDeserializer (Type type, Deserializers deserializers)
        {
            if (deserializers.AttributeDeserializer == null) {
                if (typeof (IXmlDeserializable).IsAssignableFrom (type)) {
                    deserializers.AttributeDeserializer = (obj, context) => ((IXmlDeserializable)obj).DeserializeAttribute (context);
                } else {
                    deserializers.AttributeDeserializer = GetAttributeAutoDeserializer (type, deserializers);
                }
            }
            return deserializers.AttributeDeserializer;
        }
        
        ObjectDeserializer GetAttributeAutoDeserializer (Type type)
        {
            return GetAttributeAutoDeserializer (type, GetDeserializers (type));
        }
        
        ObjectDeserializer GetAttributeAutoDeserializer (Type type, Deserializers deserializers)
        {
            if (deserializers.AttributeAutoDeserializer == null) {
                deserializers.AttributeAutoDeserializer = CreateAttributeAutoDeserializer (
                    CreateAttributeAutoDeserializers (type, deserializers));
            }
            return deserializers.AttributeAutoDeserializer;
        }
        
        ObjectDeserializer CreateAttributeAutoDeserializer (Dictionary<string, ObjectDeserializer> deserializers)
        {
            if (deserializers.Count == 0) {
                return (obj, context) => {};
            } else {
                return (obj, context) => {
                    var name = CreateName (context.Reader.LocalName, context.Reader.NamespaceURI);
                    if (deserializers.ContainsKey (name)) {
                        deserializers[name] (obj, context);
                    }
                };
            }
        }
        
        ObjectDeserializer CreateElementAutoDeserializer (Dictionary<string, ObjectDeserializer> deserializers)
        {
            if (deserializers.Count == 0) {
                return (obj, context) => {};
            } else {
                return (obj, context) => {
                    var name = CreateName (context.Reader.LocalName, context.Reader.NamespaceURI);
                    if (deserializers.ContainsKey (name)) {
                        deserializers[name] (obj, context);
                    } else if (deserializers.ContainsKey (context.Reader.Name)) {
                        deserializers[context.Reader.Name] (obj, context);
                    } else {
                        // TODO this is a workaround for mono bug 334752 and another problem
                        context.Reader.Skip ();
                    }
                };
            }
        }
        
        Dictionary<string, ObjectDeserializer> CreateAttributeAutoDeserializers (Type type, Deserializers deserializers)
        {
            var object_deserializers = new Dictionary<string, ObjectDeserializer> ();
            ProcessAttributeAutoDeserializers (type, BindingFlags.Instance | BindingFlags.Public, deserializers, object_deserializers);
            ProcessAttributeAutoDeserializers (type, BindingFlags.Instance | BindingFlags.NonPublic, deserializers, object_deserializers);
            return object_deserializers;
        }
        
        void ProcessAttributeAutoDeserializers (Type type, BindingFlags flags, Deserializers deserializers, Dictionary<string, ObjectDeserializer> objectDeserializeres)
        {
            foreach (var property in type.GetProperties (flags)) {
                XmlAttributeAttribute attribute_attribute = null;
                foreach (var custom_attribute in property.GetCustomAttributes (false)) {
                    if (custom_attribute is DoNotDeserializeAttribute) {
                        attribute_attribute = null;
                        break;
                    }
                    var attribute = custom_attribute as XmlAttributeAttribute;
                    if (attribute != null) {
                        attribute_attribute = attribute;
                        continue;
                    }
                }
                if (attribute_attribute != null) {
                    var deserializer =
                        CreateCustomDeserializer (property, deserializers) ??
                        CreateAttributeDeserializer (property, property.PropertyType);
                    AddDeserializer (objectDeserializeres,
                        CreateName (property.Name, attribute_attribute.Name, attribute_attribute.Namespace),
                        deserializer);
                    break;
                }
            }
        }
        
        ObjectDeserializer CreateAttributeDeserializer (PropertyInfo property, Type type)
        {
            if (!property.CanWrite) {
                // TODO throw
            }
            var deserializer = CreateAttributeDeserializer (type);
            return (obj, context) => property.SetValue (obj, deserializer (context), null);
        }
                    
        ObjectDeserializer CreateCustomDeserializer (PropertyInfo property, Deserializers deserializers)
        {
            if (!property.CanWrite) {
                // TODO throw
            }
            if (deserializers.TypeDeserializers != null && deserializers.TypeDeserializers.ContainsKey (property.PropertyType)) {
                var method = deserializers.TypeDeserializers[property.PropertyType];
                return (obj, context) => property.SetValue (obj, method.Invoke (obj, new object [] { context }), null);
            }
            return null;
        }
        
        Deserializer CreateAttributeDeserializer (Type type)
        {
            if (type == typeof (string)) {
                return context => context.Reader.ReadContentAsString ();
            } else if (type == typeof (int)) {
                return context => context.Reader.ReadContentAsInt ();
            } else if (type == typeof (double)) {
                return context => context.Reader.ReadContentAsDouble ();
            } else if (type == typeof (bool)) {
                return context => context.Reader.ReadContentAsBoolean ();
            } else if (type == typeof (long)) {
                return context => context.Reader.ReadContentAsLong ();
            } else if (type == typeof (float)) {
                return context => context.Reader.ReadContentAsFloat ();
            } else if (type == typeof (decimal)) {
                return context => context.Reader.ReadContentAsDecimal ();
            } else if (type == typeof (DateTime)) {
                return context => context.Reader.ReadContentAsDateTime ();
            } else if (type == typeof (Uri)) {
                return context => new Uri (context.Reader.ReadContentAsString ());
            } else if (type.IsEnum ) {
                var map = GetEnumMap (type);
                return context => map[context.Reader.ReadContentAsString ()];
            } else {
                return context => context.Reader.ReadContentAs (type, null);
            }
        }
        
        ObjectDeserializer GetElementDeserializer (Type type, Deserializers deserializers)
        {
            if (deserializers.ElementDeserializer == null) {
                if (typeof (IXmlDeserializable).IsAssignableFrom (type)) {
                    deserializers.ElementDeserializer = (obj, context) => ((IXmlDeserializable)obj).DeserializeElement (context);
                } else {
                    deserializers.ElementDeserializer = GetElementAutoDeserializer (type, deserializers);
                }
            }
            return deserializers.ElementDeserializer;
        }
        
        ObjectDeserializer GetElementAutoDeserializer (Type type)
        {
            return GetElementAutoDeserializer (type, GetDeserializers (type));
        }
        
        ObjectDeserializer GetElementAutoDeserializer (Type type, Deserializers deserializers)
        {
            if (deserializers.ElementAutoDeserializer == null) {
                deserializers.TypeDeserializers = GetTypeDeserializers (type);
                deserializers.ElementAutoDeserializer = CreateElementAutoDeserializer (CreateElementAutoDeserializers (type, deserializers));
            }
            return deserializers.ElementAutoDeserializer;
        }
        
        Dictionary<string, ObjectDeserializer> CreateElementAutoDeserializers (Type type, Deserializers deserializers)
        {
            var object_deserializers = new Dictionary<string, ObjectDeserializer> ();
            ProcessElementAutoDeserializers (type, BindingFlags.Instance | BindingFlags.Public, deserializers, object_deserializers);
            ProcessElementAutoDeserializers (type, BindingFlags.Instance | BindingFlags.NonPublic, deserializers, object_deserializers);
            return object_deserializers;
        }
        
        void ProcessElementAutoDeserializers (Type type, BindingFlags flags, Deserializers deserializers, Dictionary<string, ObjectDeserializer> objectDeserializers)
        {
            foreach (var property in type.GetProperties (flags)) {
                XmlElementAttribute element_attribute = null;
                XmlFlagAttribute flag_attribute = null;
                XmlArrayAttribute array_attribute = null;
                XmlArrayItemAttribute array_item_attribute = null;
                
                foreach (var custom_attribute in property.GetCustomAttributes (false)) {
                    if (custom_attribute is DoNotDeserializeAttribute) {
                        element_attribute = null;
                        flag_attribute = null;
                        array_attribute = null;
                        break;
                    }
                    
                    var element = custom_attribute as XmlElementAttribute;
                    if (element != null) {
                        element_attribute = element;
                        continue;
                    }
                    
                    var flag = custom_attribute as XmlFlagAttribute;
                    if (flag != null) {
                        flag_attribute = flag;
                        continue;
                    }
                    
                    var array = custom_attribute as XmlArrayAttribute;
                    if (array != null) {
                        array_attribute = array;
                        continue;
                    }
                    
                    var array_item = custom_attribute as XmlArrayItemAttribute;
                    if (array_item != null) {
                        array_item_attribute = array_item;
                        continue;
                    }
                }
                
                if (element_attribute != null) {
                    var deserializer =
                        CreateCustomDeserializer (property, deserializers) ??
                        CreateElementDeserializer (property);
                    AddDeserializer (objectDeserializers,
                        CreateName (property.Name, element_attribute.Name, element_attribute.Namespace),
                        deserializer);
                    continue;
                }
                
                if (flag_attribute != null) {
                    AddDeserializer (objectDeserializers,
                        CreateName (property.Name, flag_attribute.Name, flag_attribute.Namespace),
                        CreateFlagDeserializer (property));
                    continue;
                }
                
                if (array_attribute != null) {
                    AddDeserializer (objectDeserializers,
                        CreateName (property.Name, array_attribute.Name, array_attribute.Namespace),
                        CreateElementDeserializer (property, array_item_attribute, deserializers));
                    continue;
                }
            }
        }
        
        ObjectDeserializer CreateElementDeserializer (PropertyInfo property)
        {
            if (!property.CanWrite) {
                // TODO throw
            }
            var next = GetDeserializer (property.PropertyType);
            return (obj, context) => property.SetValue (obj, next (context), null);
        }
        
        ObjectDeserializer CreateFlagDeserializer (PropertyInfo property)
        {
            if (!property.CanWrite) {
                // TODO throw
            }
            return (obj, context) => property.SetValue (obj, true, null);
        }
        
        ItemDeserializer CreateItemDeserializer (Type type, Deserializers deserializers)
        {
            if (deserializers.TypeDeserializers != null && deserializers.TypeDeserializers.ContainsKey (type)) {
                var method = deserializers.TypeDeserializers[type];
                return (obj, context) => method.Invoke (obj, new object [] { context });
            } else {
                var deserializer = GetDeserializer (type);
                return (obj, context) => deserializer (context);
            }
        }
        
        ObjectDeserializer CreateElementDeserializer (PropertyInfo property, XmlArrayItemAttribute arrayItemAttribute, Deserializers deserializers)
        {
            if (!property.CanWrite) {
                // TODO throw
            }
			
			Type icollection;
			if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition () == typeof (ICollection<>)) {
				icollection = property.PropertyType;
			} else {
				icollection = property.PropertyType.GetInterface ("ICollection`1");
			}
            
            if (icollection == null) {
                // TODO throw
            }
            
            var add = icollection.GetMethod ("Add");
            var item_type = icollection.GetGenericArguments ()[0];
            var item_deserializer = CreateItemDeserializer (item_type, deserializers);
            return (obj, context) => {
                var collection = property.GetValue (obj, null);
                var depth = context.Reader.Depth;
                while (context.Reader.Read () && context.Reader.NodeType == XmlNodeType.Element && context.Reader.Depth > depth) {
                    var item_reader = context.Reader.ReadSubtree ();
                    item_reader.Read ();
                    try {
                        add.Invoke (collection, new object [] { item_deserializer (obj, new XmlDeserializationContext (this, item_reader)) }); 
                    } catch {
                    } finally {
                        item_reader.Close ();
                    }
                }
            };
        }
        
        static string CreateName (string name, string @namespace)
        {
            return CreateName (null, name, @namespace);
        }
        
        static string CreateName (string backupName, string name, string @namespace)
        {
            if (string.IsNullOrEmpty (@namespace)) {
                return name ?? backupName;
            } else {
                return string.Format ("{0}/{1}", name ?? backupName, @namespace);
            }
        }
        
        static void AddDeserializer (Dictionary<string, ObjectDeserializer> deserializers, string name, ObjectDeserializer deserializer)
        {
            if (deserializers.ContainsKey (name)) {
                // TODO throw
            }
            deserializers[name] = deserializer;
        }
    }
}
