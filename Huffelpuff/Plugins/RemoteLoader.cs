/*
 *  The Huffelpuff Irc Bot, versatile pluggable bot for IRC chats
 * 
 *  Copyright (c) 2008-2010 Thomas Bruderer <apophis@apophis.ch>
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
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Huffelpuff.Plugins
{
    /// <summary>
    /// The remote loader loads assumblies into a remote <see cref="AppDomain"/>
    /// </summary>
    public class RemoteLoader : MarshalByRefObject
    {
        protected ArrayList TypeList = new ArrayList();
        protected ArrayList assemblyList = new ArrayList();

        /// <summary>
        /// Loads the assembly into the remote domain
        /// </summary>
        /// <param name="fullname">The full filename of the assembly to load</param>
        public void LoadAssembly(string fullname)
        {
            string filename = Path.GetFileNameWithoutExtension(fullname);

            Assembly assembly = Assembly.Load(filename);
            assemblyList.Add(assembly);
            foreach (Type loadedType in assembly.GetTypes())
            {
                TypeList.Add(loadedType);
            }
        }

        /// <summary>
        /// Loads the script into the remote domain
        /// </summary>
        /// <param name="filename">The full filename of the script to load</param>
        /// <param name="references">The dll references to compile with</param>
        /// <returns>A list of compiler errors if any.</returns>
        public IList LoadScript(string filename, IList references)
        {
            var assemblyFactory = new AssemblyFactory();
            try
            {
                Assembly scriptAssembly = assemblyFactory.CreateAssembly(filename, references);
                assemblyList.Add(scriptAssembly);
                foreach (Type loadedType in scriptAssembly.GetTypes())
                {
                    TypeList.Add(loadedType);
                }

                // No errors, return an empty list.
                return new ArrayList();
            }
            catch
            {
                var compilerErrors = new ArrayList();
                foreach (CompilerError error in assemblyFactory.CompilerErrors)
                {
                    compilerErrors.Add(error.ErrorText);
                }
                return compilerErrors;
            }
        }

        /// <summary>
        /// Loads the scripts into the remote domain
        /// </summary>
        /// <param name="filenames">The filenames of the scripts to load</param>
        /// <param name="references">The dll references to compile with</param>
        /// <returns>A list of compiler errors if any</returns>
        public IList LoadScripts(IList filenames, IList references)
        {
            var assemblyFactory = new AssemblyFactory();
            try
            {
                Assembly scriptAssembly = assemblyFactory.CreateAssembly(filenames, references);
                assemblyList.Add(scriptAssembly);
                foreach (Type loadedType in scriptAssembly.GetTypes())
                {
                    TypeList.Add(loadedType);
                }

                // No errors, return an empty list.
                return new ArrayList();
            }
            catch
            {
                var compilerErrors = new ArrayList();
                foreach (CompilerError error in assemblyFactory.CompilerErrors)
                {
                    compilerErrors.Add(error.ErrorText);
                }
                return compilerErrors;
            }
        }

        /// <summary>
        /// The types loaded by the plugin manager
        /// </summary>
        public string[] GetTypes()
        {
            var classList = new ArrayList();
            foreach (Type pluginType in TypeList)
            {
                classList.Add(pluginType.FullName);
            }
            return (string[])classList.ToArray(typeof(string));
        }

        /// <summary>
        /// The assemblies loaded by the plugin manager
        /// </summary>
        public string[] GetAssemblies()
        {
            var assemblyNameList = new ArrayList();
            foreach (Assembly userAssembly in assemblyList)
            {
                assemblyNameList.Add(userAssembly.FullName);
            }
            return (string[])assemblyNameList.ToArray(typeof(string));
        }

        /// <summary>
        /// Retrieves the type objects for all subclasses of the given type within the loaded plugins.
        /// </summary>
        /// <param name="baseClass">The base class</param>
        /// <returns>All subclases</returns>
        public string[] GetSubclasses(string baseClass)
        {
            Type baseClassType = Type.GetType(baseClass) ?? GetTypeByName(baseClass);
            if (baseClassType == null)
            {
                throw new ArgumentException("Cannot find a type of name " + baseClass +
                    " within the plugins or the common library.");
            }
            var subclassList = new ArrayList();
            foreach (var pluginType in TypeList.Cast<Type>().Where(pluginType => pluginType.IsSubclassOf(baseClassType)))
            {
                subclassList.Add(pluginType.FullName);
            }
            return (string[])subclassList.ToArray(typeof(string));
        }

        /// <summary>
        /// Returns a proxy to an instance of the specified plugin type
        /// </summary>
        /// <param name="typeName">The name of the type to create an instance of</param>
        /// <param name="bindingFlags">The binding flags for the constructor</param>
        /// <param name="constructorParams">The parameters to pass to the constructor</param>
        /// <returns>The constructed object</returns>
        public MarshalByRefObject CreateInstance(string typeName, BindingFlags bindingFlags,
            object[] constructorParams)
        {
            Assembly owningAssembly = null;
            foreach (var assembly in assemblyList.Cast<Assembly>().Where(assembly => assembly.GetType(typeName) != null))
            {
                owningAssembly = assembly;
            }
            if (owningAssembly == null)
            {
                throw new InvalidOperationException("Could not find owning assembly for type " + typeName);
            }
            var createdInstance = owningAssembly.CreateInstance(typeName, false, bindingFlags, null,
                constructorParams, null, null) as MarshalByRefObject;
            if (createdInstance == null)
            {
                throw new ArgumentException("typeName must specify a Type that derives from MarshalByRefObject");
            }
            return createdInstance;
        }

        /// <summary>
        /// Determines if this loader manages the specified type
        /// </summary>
        /// <param name="typeName">The type to check if this PluginManager handles</param>
        /// <returns>True if this PluginManager handles the type</returns>
        public bool ManagesType(string typeName)
        {
            return (GetTypeByName(typeName) != null);
        }

        /// <summary>
        /// Returns the Type object by FullName
        /// </summary>
        /// <param name="typeName">The plugin Type to look up</param>
        /// <returns>The Type object for the specified type name; null if not found</returns>
        private Type GetTypeByName(string typeName)
        {
            return TypeList.Cast<Type>().FirstOrDefault(pluginType => pluginType.FullName == typeName);
        }

        /// <summary>
        /// Returns the value of a static property
        /// </summary>
        /// <param name="typeName">The type to retrieve the static property value from</param>
        /// <param name="propertyName">The name of the property to retrieve</param>
        /// <returns>The value of the static property</returns>
        public object GetStaticPropertyValue(string typeName, string propertyName)
        {
            Type type = GetTypeByName(typeName);
            if (type == null)
            {
                throw new ArgumentException("Cannot find a type of name " + typeName +
                    " within the plugins or the common library.");
            }
            return type.GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
        }

        /// <summary>
        /// Returns the result of a static method call
        /// </summary>
        /// <param name="typeName">The type to call the static method on</param>
        /// <param name="methodName">The name of the method</param>
        /// <param name="methodParams">The parameters to pass to the method</param>
        /// <returns>The return value of the method</returns>
        public object CallStaticMethod(string typeName, string methodName, object[] methodParams)
        {
            Type type = GetTypeByName(typeName);
            if (type == null)
            {
                throw new ArgumentException("Cannot find a type of name " + typeName + " within the plugins or the common library.");
            }
            return type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static).Invoke(null, BindingFlags.Public | BindingFlags.Static, null, methodParams, null);
        }
    }
}
