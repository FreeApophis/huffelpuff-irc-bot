using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Huffelpuff.Utils;

namespace Huffelpuff.Plugins
{
    /// <summary>
    /// The PluginManager tracks changes to the plugin directory, handles reloading of the plugins,
    /// and monitoring the plugin directory.
    /// </summary>
    public class PluginManager
    {
        private bool _started;
        private bool _autoReload = true;
        private IList _compilerErrors;
        protected string PluginDirectory;
        protected FileSystemWatcher FileWatcher;
        protected DateTime ChangeTime = new DateTime(0);
        protected Thread PluginReloadThread;
        protected string LockObject = "{PLUGINMANAGERLOCK}";
        protected bool BeginShutdown;
        protected bool Active = true;
        protected AppDomain PluginAppDomain;
        protected AppDomainSetup PluginAppDomainSetup;
        protected RemoteLoader RemoteLoader;
        protected LocalLoader LocalLoader;
        protected IList References = new ArrayList();

        /// <summary>
        /// Constructs a plugin manager
        /// </summary>
        /// <param name="pluginRelativePath">The relative path to the plugins directory</param>
        /// <param name="autoReload">Should auto reload on file changes</param>
        public PluginManager(string pluginRelativePath = "plugins", bool autoReload = true)
        {
            this._autoReload = autoReload;

            PluginDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            PluginDirectory = Path.Combine(PluginDirectory, pluginRelativePath);

            LocalLoader = new LocalLoader(PluginDirectory);

            // Add the most common references since plugin authors can't control which references they
            // use in scripts.  Adding a reference later that already exists does nothing.
            AddReference("Accessibility.dll");
            AddReference("Microsoft.Vsa.dll");
            AddReference("System.Configuration.Install.dll");
            AddReference("System.Data.dll");
            AddReference("System.Design.dll");
            AddReference("System.DirectoryServices.dll");
            AddReference("System.Drawing.Design.dll");
            AddReference("System.Drawing.dll");
            AddReference("System.EnterpriseServices.dll");
            AddReference("System.Management.dll");
            AddReference("System.Runtime.Remoting.dll");
            AddReference("System.Runtime.Serialization.Formatters.Soap.dll");
            AddReference("System.Security.dll");
            AddReference("System.ServiceProcess.dll");
            AddReference("System.Web.dll");
            AddReference("System.Web.RegularExpressions.dll");
            AddReference("System.Web.Services.dll");
            AddReference("System.Windows.Forms.Dll");
            AddReference("System.XML.dll");
            AddReference("SharpIRC.dll");

        }

        /// <summary>
        /// The destructor for the plugin manager
        /// </summary>
        ~PluginManager()
        {
            Stop();
        }

        /// <summary>
        /// Fires when the plugins have been reloaded and references to the old objects need
        /// to be updated.
        /// </summary>
        public event EventHandler PluginsReloaded;

        /// <summary>
        /// Handles changes to the file system in the plugin directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileWatcherChanged(object sender, FileSystemEventArgs e)
        {
            ChangeTime = DateTime.Now + new TimeSpan(0, 0, 10);
        }

        /// <summary>
        /// The main updater thread loop.
        /// </summary>
        protected void ReloadThreadLoop()
        {
            if (!_started)
            {
                throw new InvalidOperationException("PluginManager has not been started.");
            }
            var invalidTime = new DateTime(0);
            while (!BeginShutdown)
            {
                if (ChangeTime != invalidTime && DateTime.Now > ChangeTime)
                {
                    ReloadPlugins();
                }
                Thread.Sleep(5000);
            }
            Active = false;
        }

        /// <summary>
        /// Initializes the plugin manager
        /// </summary>
        public void Start()
        {
            _started = true;
            if (_autoReload)
            {
                var dir = new DirectoryInfo(PluginDirectory);
                if (!dir.Exists) { dir.Create(); }

                FileWatcher = new FileSystemWatcher(PluginDirectory) { EnableRaisingEvents = true };
                FileWatcher.Changed += FileWatcherChanged;
                FileWatcher.Deleted += FileWatcherChanged;
                FileWatcher.Created += FileWatcherChanged;

                PluginReloadThread = new Thread(ReloadThreadLoop);
                PluginReloadThread.Start();
            }
            ReloadPlugins();
        }

        /// <summary>
        /// Reloads all plugins in the plugins directory
        /// </summary>
        public void ReloadPlugins()
        {
            if (!_started)
            {
                throw new InvalidOperationException("PluginManager has not been started.");
            }
            lock (LockObject)
            {
                LocalLoader.Unload();
                LocalLoader = new LocalLoader(PluginDirectory);
                try
                {
                    LoadUserAssemblies();
                }
                catch (Exception exception)
                {
                    Log.Instance.Log(exception);
                }

                ChangeTime = new DateTime(0);
                PluginsReloaded?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Loads all user created plugin assemblies
        /// </summary>
        protected void LoadUserAssemblies()
        {
            if (!_started)
            {
                throw new InvalidOperationException("PluginManager has not been started.");
            }
            _compilerErrors = new ArrayList();
            var directory = new DirectoryInfo(PluginDirectory);
            if (PluginSources == PluginSourceEnum.DynamicAssemblies ||
                PluginSources == PluginSourceEnum.Both)
            {
                foreach (var file in directory.GetFiles("*.dll"))
                {
                    try
                    {
                        LocalLoader.LoadAssembly(file.FullName);
                    }
                    catch (PolicyException e)
                    {
                        throw new PolicyException(String.Format("Cannot load {0} - code requires privilege to execute", file.Name), e);
                    }
                }
            }
            if (PluginSources != PluginSourceEnum.DynamicCompilation && PluginSources != PluginSourceEnum.Both) return;

            // Load all C# scripts
            {
                var scriptList = new ArrayList();
                foreach (FileInfo file in directory.GetFiles("*.cs"))
                {
                    scriptList.Add(file.FullName);
                }
                LoadScriptBatch((string[])scriptList.ToArray(typeof(string)));
            }

            // Load all VB.net scripts
            {
                var scriptList = new ArrayList();
                foreach (var file in directory.GetFiles("*.vb"))
                {
                    scriptList.Add(file.FullName);
                }
                LoadScriptBatch((string[])scriptList.ToArray(typeof(string)));
            }

            // Load all JScript scripts
            {
                var scriptList = new ArrayList();
                foreach (var file in directory.GetFiles("*.js"))
                {
                    scriptList.Add(file.FullName);
                }
                LoadScriptBatch((string[])scriptList.ToArray(typeof(string)));
            }
        }

        /// <summary>
        /// Batch loads a set of scripts of the same language
        /// </summary>
        /// <param name="fileNames">The list of script fileNames to load</param>
        private void LoadScriptBatch(string[] fileNames)
        {
            if (fileNames.Length <= 0) return;
            var errors = LocalLoader.LoadScripts(fileNames, References);
            if (errors.Count <= 0) return;

            // If there are compiler errors record them and the file they occurred in
            foreach (string error in errors)
            {
                _compilerErrors.Add(error);
            }
            if (!IgnoreErrors)
            {
                var aggregateErrorText = new StringBuilder();
                foreach (string error in errors)
                {
                    aggregateErrorText.Append(error + "\r\n");
                }
                throw new InvalidOperationException("\r\nCompiler error(s) have occurred:\r\n\r\n " + aggregateErrorText + "\r\n");
            }
        }

        /// <summary>
        /// Adds a reference to the plugin manager to be used when compiling scripts
        /// </summary>
        /// <param name="referenceToDll">The reference to the dll to add</param>
        public void AddReference(string referenceToDll)
        {
            if (!References.Contains(referenceToDll))
            {
                References.Add(referenceToDll);
            }
        }

        /// <summary>
        /// Shuts down the plugin manager
        /// </summary>
        public void Stop()
        {
            try
            {
                _started = false;
                LocalLoader.Unload();
                BeginShutdown = true;
                while (Active)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception exception)
            {
                // We don't want to get any exceptions thrown if unloading fails for some reason.
                Log.Instance.Log(exception);
            }
        }

        /// <summary>
        /// Should auto reload on file changes
        /// </summary>
        public bool AutoReload
        {
            get => _autoReload;
            set
            {
                if (_autoReload == value) return;

                _autoReload = value;
                if (!_autoReload)
                {
                    FileWatcher.EnableRaisingEvents = false;
                    Stop();
                    PluginReloadThread = null;
                    FileWatcher = null;
                }
                else
                {
                    FileWatcher = new FileSystemWatcher(PluginDirectory) { EnableRaisingEvents = true };
                    FileWatcher.Changed += FileWatcherChanged;
                    FileWatcher.Deleted += FileWatcherChanged;
                    FileWatcher.Created += FileWatcherChanged;

                    PluginReloadThread = new Thread(ReloadThreadLoop);
                    PluginReloadThread.Start();
                }
            }
        }

        /// <summary>
        /// Determines whether an exception will be thrown if a compiler error occurs in a script file
        /// </summary>
        public bool IgnoreErrors { get; set; } = true;

        /// <summary>
        /// The type of plugin sources that will be managed by the plugin manager
        /// </summary>
        public PluginSourceEnum PluginSources { get; set; } = PluginSourceEnum.Both;

        /// <summary>
        /// The list of all compiler errors for all scripts.
        /// Null if no compilation has ever occurred, empty list if compilation succeeded
        /// </summary>
        public IList CompilerErrors
        {
            get
            {
                if (!_started)
                {
                    throw new InvalidOperationException("PluginManager has not been started.");
                }
                return _compilerErrors;
            }
        }

        /// <summary>
        /// The list of loaded plugin assemblies
        /// </summary>
        public string[] Assemblies
        {
            get
            {
                if (!_started)
                {
                    throw new InvalidOperationException("PluginManager has not been started.");
                }
                return LocalLoader.Assemblies;
            }
        }

        /// <summary>
        /// The list of loaded plugin types
        /// </summary>
        public string[] Types
        {
            get
            {
                if (!_started)
                {
                    throw new InvalidOperationException("PluginManager has not been started.");
                }
                return LocalLoader.Types;
            }
        }

        /// <summary>
        /// Retrieves the type objects for all subclasses of the given type within the loaded plugins.
        /// </summary>
        /// <param name="baseClass">The base class</param>
        /// <returns>All sub classes</returns>
        public string[] GetSubclasses(string baseClass)
        {
            if (!_started)
            {
                throw new InvalidOperationException("PluginManager has not been started.");
            }
            return LocalLoader.GetSubclasses(baseClass);
        }

        /// <summary>
        /// Determines if this loader manages the specified type
        /// </summary>
        /// <param name="typeName">The type to check if this PluginManager handles</param>
        /// <returns>True if this PluginManager handles the type</returns>
        public bool ManagesType(string typeName)
        {
            if (!_started)
            {
                throw new InvalidOperationException("PluginManager has not been started.");
            }
            return LocalLoader.ManagesType(typeName);
        }

        /// <summary>
        /// Returns the value of a static property
        /// </summary>
        /// <param name="typeName">The type to retrieve the static property value from</param>
        /// <param name="propertyName">The name of the property to retrieve</param>
        /// <returns>The value of the static property</returns>
        public object GetStaticPropertyValue(string typeName, string propertyName)
        {
            if (!_started)
            {
                throw new InvalidOperationException("PluginManager has not been started.");
            }
            return LocalLoader.GetStaticPropertyValue(typeName, propertyName);
        }

        /// <summary>
        /// Returns the result of a static method call
        /// </summary>
        /// <param name="typeName">The type to call the static method on</param>
        /// <param name="methodName"></param>
        /// <param name="methodParams">The parameters to pass to the method</param>
        /// <returns>The return value of the method</returns>
        public object CallStaticMethod(string typeName, string methodName, object[] methodParams)
        {
            if (!_started)
            {
                throw new InvalidOperationException("PluginManager has not been started.");
            }
            return LocalLoader.CallStaticMethod(typeName, methodName, methodParams);
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
            if (!_started)
            {
                throw new InvalidOperationException("PluginManager has not been started.");
            }
            return LocalLoader.CreateInstance(typeName, bindingFlags, constructorParams);
        }
    }
}
