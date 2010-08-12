using Huffelpuff.Utils;
using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace Huffelpuff.Plugins
{
    /// <summary>
    /// Local loader class
    /// </summary>
    public class LocalLoader : MarshalByRefObject
    {
        AppDomain appDomain;
        readonly RemoteLoader remoteLoader;

        /// <summary>
        /// Creates the local loader class
        /// </summary>
        /// <param name="pluginDirectory">The plugin directory</param>
        public LocalLoader(string pluginDirectory)
        {
            var setup = new AppDomainSetup
            {
                ApplicationName = "Plugins",
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                PrivateBinPath = "plugins",
                CachePath = Path.Combine(pluginDirectory, "cache" + Path.DirectorySeparatorChar),
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = pluginDirectory
            };

            appDomain = AppDomain.CreateDomain("Plugins", null, setup);

            // Used for a Cross AppDomain Singleton
            appDomain.SetData("PersistentMemoryInstance", PersistentMemory.Instance);
            appDomain.AssemblyResolve += new ResolveEventHandler(AppDomainAssemblyResolve);

            appDomain.UnhandledException += AppDomainUnhandledException;
            appDomain.InitializeLifetimeService();

            remoteLoader = (RemoteLoader)appDomain.CreateInstanceAndUnwrap("Huffelpuff", "Huffelpuff.Plugins.RemoteLoader");
        }

        static Assembly AppDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return null;
        }

        static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs ex)
        {
            var e = (Exception)ex.ExceptionObject;
            Log.Instance.Log(e.Message, Level.Error, ConsoleColor.Red);
            Log.Instance.Log(e.Source, Level.Error, ConsoleColor.Red);
            Log.Instance.Log(e.StackTrace, Level.Error, ConsoleColor.Red);
        }

        /// <summary>
        /// Loads the specified assembly
        /// </summary>
        /// <param name="filename">The filename of the assembly to load</param>
        public void LoadAssembly(string filename)
        {
            remoteLoader.LoadAssembly(filename);
        }

        /// <summary>
        /// Loads the specified script
        /// </summary>
        /// <param name="filename">The filename of the script to load</param>
        /// <returns>A list of compiler errors if any</returns>
        public IList LoadScript(string filename)
        {
            return LoadScript(filename, new ArrayList());
        }

        /// <summary>
        /// Loads the specified script
        /// </summary>
        /// <param name="filename">The filename of the script to load</param>
        /// <param name="references">The dll references to compile with</param>
        /// <returns>A list of compiler errors if any</returns>
        public IList LoadScript(string filename, IList references)
        {
            return remoteLoader.LoadScript(filename, references);
        }

        /// <summary>
        /// Loads the specified scripts
        /// </summary>
        /// <param name="filenames">The filenames of the scripts to load</param>
        /// <returns>A list of compiler errors if any</returns>
        public IList LoadScripts(IList filenames)
        {
            return LoadScripts(filenames, new ArrayList());
        }

        /// <summary>
        /// Loads the specified scripts
        /// </summary>
        /// <param name="filenames">The filenames of the scripts to load</param>
        /// <param name="references">The dll references to compile with</param>
        /// <returns>A list of compiler errors if any</returns>
        public IList LoadScripts(IList filenames, IList references)
        {
            return remoteLoader.LoadScripts(filenames, references);
        }

        /// <summary>
        /// Unloads the plugins
        /// </summary>
        public void Unload()
        {
            AppDomain.Unload(appDomain);
            appDomain = null;
        }

        /// <summary>
        /// The list of loaded plugin assemblies
        /// </summary>
        public string[] Assemblies
        {
            get
            {
                return remoteLoader.GetAssemblies();
            }
        }

        /// <summary>
        /// The list of loaded plugin types
        /// </summary>
        public string[] Types
        {
            get
            {
                return remoteLoader.GetTypes();
            }
        }

        /// <summary>
        /// Retrieves the type objects for all subclasses of the given type within the loaded plugins.
        /// </summary>
        /// <param name="baseClass">The base class</param>
        /// <returns>All subclases</returns>
        public string[] GetSubclasses(string baseClass)
        {
            return remoteLoader.GetSubclasses(baseClass);
        }

        /// <summary>
        /// Determines if this loader manages the specified type
        /// </summary>
        /// <param name="typeName">The type to check if this PluginManager handles</param>
        /// <returns>True if this PluginManager handles the type</returns>
        public bool ManagesType(string typeName)
        {
            return remoteLoader.ManagesType(typeName);
        }

        /// <summary>
        /// Returns the value of a static property
        /// </summary>
        /// <param name="typeName">The type to retrieve the static property value from</param>
        /// <param name="propertyName">The name of the property to retrieve</param>
        /// <returns>The value of the static property</returns>
        public object GetStaticPropertyValue(string typeName, string propertyName)
        {
            return remoteLoader.GetStaticPropertyValue(typeName, propertyName);
        }

        /// <summary>
        /// Returns the result of a static method call
        /// </summary>
        /// <param name="typeName">The type to call the static method on</param>
        /// <param name="methodName">The name of the method to call</param>
        /// <param name="methodParams">The parameters to pass to the method</param>
        /// <returns>The return value of the method</returns>
        public object CallStaticMethod(string typeName, string methodName, object[] methodParams)
        {
            return remoteLoader.CallStaticMethod(typeName, methodName, methodParams);
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
            return remoteLoader.CreateInstance(typeName, bindingFlags, constructorParams);
        }
    }
}
