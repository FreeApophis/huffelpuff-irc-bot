// 
// ServiceControllerBuilder.cs
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
using System.Reflection;

using Mono.Upnp.Control;

namespace Mono.Upnp.Internal
{
    static class ServiceControllerBuilder
    {
        class Eventer : StateVariableEventer
        {
            public static readonly MethodInfo handler = typeof (Eventer).GetMethod ("Handler");
            
            public void Handler<T> (object sender, StateVariableChangedArgs<T> args)
            {
                OnStateVariableUpdated (args.NewValue.ToString ());
            }
        }
        
        class ArgumentInfo
        {
            public Argument Argument;
            public ParameterInfo ParameterInfo;
        }
        
        class StateVariableInfo
        {
            public StateVariable StateVariable;
            public Type Type;
        }
        
        public static ServiceController Build<T> (T service)
        {
            var state_variables = new Dictionary<string, StateVariableInfo> ();
            return new ServiceController (BuildActions (service, state_variables), BuildStateVariables (service, state_variables));
        }
        
        static IEnumerable<ServiceAction> BuildActions<T> (T service, Dictionary<string, StateVariableInfo> stateVariables)
        {
            foreach (var method in typeof (T).GetMethods (BindingFlags.Public | BindingFlags.Instance)) {
                var action = BuildAction (method, service, stateVariables);
                if (action != null) {
                    yield return action;
                }
            }
        }
        
        static ServiceAction BuildAction (MethodInfo method, object service, Dictionary<string, StateVariableInfo> stateVariables)
        {
            var attributes = method.GetCustomAttributes (typeof (UpnpActionAttribute), false);
            if (attributes.Length != 0) {
                var attribute = (UpnpActionAttribute)attributes[0];
                var name = string.IsNullOrEmpty (attribute.Name) ? method.Name : attribute.Name;
                var parameters = method.GetParameters ();
                var arguments = new ArgumentInfo[parameters.Length];
                for (var i = 0; i < parameters.Length; i++) {
                    arguments[i] = BuildArgumentInfo (parameters[i], name, stateVariables);
                }
                var return_argument = BuildArgumentInfo (method.ReturnParameter, name, stateVariables);
                return new ServiceAction (name, Combine (arguments, return_argument), args => {
                    var argument_array = new object[arguments.Length];
                    for (var i = 0; i < arguments.Length; i++) {
                        if (arguments[i].Argument.Direction == ArgumentDirection.Out) {
                            continue;
                        }
                        string value;
                        if (args.TryGetValue (arguments[i].Argument.Name, out value)) {
                            argument_array[i] = Convert.ChangeType (value, arguments[i].ParameterInfo.ParameterType);
                        } else {
                            // TODO throw
                        }
                    }
                    var result = method.Invoke (service, argument_array);
                    var out_arguments = new Dictionary<string, string> ();
                    for (var i = 0; i < arguments.Length; i++) {
                        if (arguments[i].Argument.Direction == ArgumentDirection.In) {
                            continue;
                        }
                        out_arguments.Add (arguments[i].Argument.Name, argument_array[i].ToString ());
                    }
                    if (return_argument != null) {
                        out_arguments.Add (return_argument.Argument.Name, result.ToString ());
                    }
                    return out_arguments;
                });
            } else {
                return null;
            }
        }
        
        static ArgumentInfo BuildArgumentInfo (ParameterInfo parameterInfo, string actionName, Dictionary<string, StateVariableInfo> stateVariables)
        {
            if (parameterInfo.ParameterType == typeof (void)) {
                return null;
            }
            var attributes = parameterInfo.GetCustomAttributes (typeof (UpnpArgumentAttribute), false);
            var attribute = attributes.Length != 0 ? (UpnpArgumentAttribute)attributes[0] : null;
            var name = attribute != null && !string.IsNullOrEmpty (attribute.Name) ? attribute.Name : (string.IsNullOrEmpty (parameterInfo.Name) ? "result" : parameterInfo.Name);
            var related_state_variable = BuildRelatedStateVariable (parameterInfo, actionName, name, stateVariables);
            var direction = parameterInfo.IsRetval || parameterInfo.ParameterType.IsByRef ? ArgumentDirection.Out : ArgumentDirection.In;
            return new ArgumentInfo {
                ParameterInfo = parameterInfo,
                Argument = new Argument (name, related_state_variable.StateVariable.Name, direction, parameterInfo.IsRetval)
            };
        }
        
        static StateVariableInfo BuildRelatedStateVariable (ParameterInfo parameterInfo, string actionName, string argumentName, Dictionary<string, StateVariableInfo> stateVariables)
        {
            var attributes = parameterInfo.GetCustomAttributes (typeof (UpnpRelatedStateVariableAttribute), false);
            var attribute = attributes.Length != 0 ? (UpnpRelatedStateVariableAttribute)attributes[0] : null;
            var name = attribute != null && !string.IsNullOrEmpty (attribute.Name) ? attribute.Name : CreateRelatedStateVariableName (argumentName);
            var data_type = attribute != null && !string.IsNullOrEmpty (attribute.DataType) ? attribute.DataType : GetDataType (parameterInfo.ParameterType);
            var default_value = attribute != null && !string.IsNullOrEmpty (attribute.DefaultValue) ? attribute.DefaultValue : null;
            var allowed_values = parameterInfo.ParameterType.IsEnum ? BuildAllowedValues (parameterInfo.ParameterType) : null;
            var allowed_value_range = attribute != null && !string.IsNullOrEmpty (attribute.MinimumValue) ? new AllowedValueRange (attribute.MinimumValue, attribute.MaximumValue, attribute.StepValue) : null;
                
            StateVariableInfo state_variable_info;
            if (stateVariables.TryGetValue (name, out state_variable_info)) {
                var state_variable = state_variable_info.StateVariable;
                if (state_variable.DataType != data_type ||
                    state_variable.DefaultValue != default_value ||
                    state_variable.AllowedValueRange != allowed_value_range ||
                    ((state_variable.AllowedValues != null || allowed_values != null) &&
                    state_variable_info.Type != parameterInfo.ParameterType)) {
                    if (attribute == null || string.IsNullOrEmpty (attribute.Name)) {
                        name = CreateRelatedStateVariableName (actionName, parameterInfo.Name);
                    }
                } else {
                    return state_variable_info;
                }
            }
            
            if (allowed_values != null) {
                state_variable_info = new StateVariableInfo {
                    StateVariable = new StateVariable (name, allowed_values, default_value),
                    Type = parameterInfo.ParameterType
                };
            } else if (allowed_value_range != null) {
                state_variable_info = new StateVariableInfo {
                    StateVariable = new StateVariable (name, data_type, allowed_value_range, default_value)
                };
            } else {
                state_variable_info = new StateVariableInfo {
                    StateVariable = new StateVariable (name, data_type, default_value)
                };
            }
            stateVariables[name] = state_variable_info;
            return state_variable_info;
        }
        
        static string CreateRelatedStateVariableName (string name)
        {
            return string.Format ("A_ARG_{0}", name);
        }
        
        static string CreateRelatedStateVariableName (string actionName, string argumentName)
        {
            return string.Format ("A_ARG_{0}_{1}", actionName, argumentName);
        }
            
        static string GetDataType (Type type)
        {
            if (type.IsByRef) {
                type = type.GetElementType ();
            }
            if (type.IsEnum || type == typeof (string)) return "string";
            if (type == typeof (int)) return "i4";
            if (type == typeof (byte)) return "ui1";
            if (type == typeof (ushort)) return "ui2";
            if (type == typeof (uint)) return "ui4";
            if (type == typeof (sbyte)) return "i1";
            if (type == typeof (short)) return "i2";
            if (type == typeof (long)) return "int"; // TODO Is this right? The UPnP docs are vague
            if (type == typeof (float)) return "r4";
            if (type == typeof (double)) return "r8";
            if (type == typeof (char)) return "char";
            if (type == typeof (DateTime)) return "date"; // TODO what about "time"?
            if (type == typeof (bool)) return "boolean";
            if (type == typeof (byte[])) return "bin";
            if (type == typeof (Uri)) return "uri";
            throw new Exception (); // TODO proper exception
        }
        
        static IEnumerable<string> BuildAllowedValues (Type type)
        {
            foreach (var field in type.GetFields (BindingFlags.Public | BindingFlags.Static)) {
                var attributes = field.GetCustomAttributes (typeof (UpnpEnumAttribute), false);
                var attribute = attributes.Length != 0 ? (UpnpEnumAttribute)attributes[0] : null;
                if (attribute != null && !string.IsNullOrEmpty (attribute.Name)) {
                    yield return attribute.Name;
                } else {
                    yield return field.Name;
                }
            }
        }
        
        static IEnumerable<StateVariable> BuildStateVariables<T> (T service, Dictionary<string, StateVariableInfo> stateVariables)
        {
            foreach (var state_variable_info in stateVariables.Values) {
                yield return state_variable_info.StateVariable;
            }
            foreach (var event_info in typeof (T).GetEvents (BindingFlags.Public | BindingFlags.Instance)) {
                var state_variable = BuildStateVariable (event_info, service);
                if (state_variable != null) {
                    yield return state_variable;
                }
            }
        }
        
        static StateVariable BuildStateVariable (EventInfo eventInfo, object service)
        {
            var attributes = eventInfo.GetCustomAttributes (typeof (UpnpStateVariableAttribute), false);
            if (attributes.Length != 0) {
                var type = eventInfo.EventHandlerType;
                if (!type.IsGenericType || type.GetGenericTypeDefinition () != typeof (EventHandler<>)) {
                    // TODO throw
                    return null;
                }
                type = type.GetGenericArguments ()[0];
                if (!type.IsGenericType || type.GetGenericTypeDefinition () != typeof (StateVariableChangedArgs<>)) {
                    // TODO throw
                    return null;
                }
                type = type.GetGenericArguments ()[0];
                var eventer = new Eventer ();
                var method = Eventer.handler.MakeGenericMethod (new Type[] { type });
                var handler = Delegate.CreateDelegate (eventInfo.EventHandlerType, eventer, method);
                eventInfo.AddEventHandler (service, handler);
                var attribute = (UpnpStateVariableAttribute)attributes[0];
                var name = string.IsNullOrEmpty (attribute.Name) ? eventInfo.Name : attribute.Name;
                var data_type = string.IsNullOrEmpty (attribute.DataType) ? GetDataType (type) : attribute.DataType;
                return new StateVariable (name, data_type, eventer, attribute.IsMulticast);
            } else {
                return null;
            }
        }
        
        static IEnumerable<Argument> Combine (IEnumerable<ArgumentInfo> arguments, ArgumentInfo return_argument)
        {
            foreach (var argument in arguments) {
                yield return argument.Argument;
            }
            if (return_argument != null) {
                yield return return_argument.Argument;
            }
        }
    }
}
