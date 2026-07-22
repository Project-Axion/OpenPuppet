using Newtonsoft.Json;
using OpenPuppet.SDK.Events;
using OpenPuppet.SDK.Plugin;
using OpenPuppet.SDK.Plugins;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static OpenPuppet.SDK.Logger;

namespace OpenPuppet.SDK
{
    public interface IPlugin : IDisposable
    {
        /*
            Plugin loading:
            - Attempt to load plugin list
                If this doesn't exist, scan the plugin
                directory, and add all of the plugins
                within that directory to the list,
                and enable all of them
            - Load the plugin list into RegisteredPlugins
            - For each plugin registered:
                - Load the plugin assembly
                - Call the OnInitialized method
                - Check for updates (if possible)
                - Mark as loaded

            Plugin unloading:
            - Call OnShutdown
            - Call Dispose
            - Unload and clean up
            - Cycle through GC cleaning
                (Warn if reference leaks occur)
            - Mark as EITHER:
                - Ready     Meaning, ready to be loaded
                - Unloaded  Meaning unloaded, but
                            has reference leaks, so should
                            not be loaded again
        */

        public static Dictionary<string, RegisteredPlugin> RegisteredPlugins { get; } = [];

        static readonly object _pluginLock = new();
        static int MaxUnloadGCAttempts = 10;
        public static string? InstallPath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }
        static string PluginPath
        {
            get
            {
                return Path.Combine(InstallPath!, "Plugins");
            }
        }
        static string PluginListPath
        {
            get
            {
                return Path.Combine(PluginPath!, "plugins.json");
            }
        }

        public const string PluginID = "";
        public PluginLogger Logger { get; }

        public static void RegisterPlugin(PluginMetadata metadata, string path, IPlugin? plugin = null, bool enabled = false)
        {
            if (RegisteredPlugins.ContainsKey(metadata.ID))
                throw new ArgumentException($"Plugin with the ID of \"{metadata.ID}\" has already been registered");

            RegisteredPlugins.Add(metadata.ID, new(metadata, path, enabled));
        }

        public static void EnablePlugin(string id)
        {
            if (RegisteredPlugins.TryGetValue(id, out var plugin))
            {
                plugin.Enabled = true;
                SavePluginList();

                LoadPlugin(id, plugin.Path);
            }
            else throw new ArgumentException($"Plugin with the ID of \"{id}\" has not been registered");
        }

        public static void DisablePlugin(string id)
        {
            if (RegisteredPlugins.TryGetValue(id, out var plugin))
            {
                plugin.Enabled = false;
                SavePluginList();
                UnloadPlugin(id);
            }
            else throw new ArgumentException($"Plugin with the ID of \"{id}\" has not been registered");
        }

        public static void SetPluginEnabled(string ID, bool enabled)
        {
            if (enabled) EnablePlugin(ID);
            else DisablePlugin(ID);
        }

        [Obsolete("Please use UninstallPlugin instead", true)]
        public static void RemovePlugin(string registry)
        {
            UninstallPlugin(registry);
        }

        public static void LoadPlugin(string id, string path)
        {
            string anycpu = Path.Combine(path, "anycpu.dll"),
                    specific = Path.Combine(path, $"{RuntimeInformation.ProcessArchitecture}.dll");

            if (File.Exists(specific))
                LoadAssembly(specific, id);
            else if (File.Exists(anycpu))
                LoadAssembly(anycpu, id);
            else
            {
                // TODO: Find if there's a more appropriate exception for this
                throw new DllNotFoundException($"Plugin with the ID \"{id}\" does not contain " +
                    $"a DLL for architecture \"{RuntimeInformation.ProcessArchitecture}\"");
            }
        }

        public static void UnloadPlugin(string id, bool soft = false)
        {
            lock(_pluginLock)
            {
                if(!RegisteredPlugins.TryGetValue(id, out var plugin))
                    throw new ArgumentException($"Plugin with the ID of \"{id}\" has not been registered");

                if(plugin.State == PluginState.Loaded &&
                    plugin.Plugin != null &&
                    plugin.LoadContext != null &&
                    plugin.WeakReference != null)
                {
                    var weakRef = plugin.WeakReference;
                    var loadContext = plugin.LoadContext;

                    try
                    {
                        plugin.Plugin.OnShutdown();
                        plugin.Plugin.Dispose();
                    }
                    catch (Exception ex)
                    {
                        SDK.logger.WriteLine(
                            ILogger.Level.Error,
                            $"Plugin \"{id}\" threw an exception during OnShutdown: {ex}"
                        );
                    }
                    finally
                    {
                        plugin.Plugin = null;
                    }

                    try
                    {
                        loadContext.Unload();
                    }
                    finally
                    {
                        plugin.LoadContext = null;
                        loadContext = null;
                    }

                    for (int i = 0; weakRef.IsAlive && i < MaxUnloadGCAttempts; i++)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }

                    bool leaked = weakRef.IsAlive;
                    weakRef = null;

                    if (leaked)
                    {
                        SDK.logger.WriteLine(
                            ILogger.Level.Warn,
                            $"Could not unload Plugin \"{id}\", possible reference leak"
                        );

                        if (soft)
                            IEvent<bool>.Invoke("openpuppet.restart", null, false);
                        else
                            IEvent<EventArgs>.Invoke("openpuppet.unstable", null, EventArgs.Empty);
                    }

                    plugin.State = leaked ? PluginState.Unloaded : PluginState.Ready;
                } else
                {
                    SDK.logger.WriteLine(
                        ILogger.Level.Log,
                        $"Plugin with the ID of \"{id}\" cannot be unloaded, as it is not in an unloadable state"
                    );
                }
            }
        }

        static void LoadAssembly(string path, string id)
        {
            if (!RegisteredPlugins.TryGetValue(id, out var plugin))
                throw new ArgumentException($"Plugin with the ID of \"{id}\" hsa not been registered");

            plugin.LoadContext = new PluginLoadContext(path);
            plugin.WeakReference = new(plugin.LoadContext);

            try
            {
                Assembly asm = plugin.LoadContext.LoadFromAssemblyPath(path);
                var t = asm.DefinedTypes.Single(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && t.IsAnsiClass);
                plugin.Plugin = (IPlugin)Activator.CreateInstance(t.AsType())!;
                plugin.State = PluginState.Loaded;
                plugin.Plugin.OnInitialized();
                /*asm.DefinedTypes.Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass).ToList().ForEach(t =>
                {
                    var p = (IPlugin)Activator.CreateInstance(t.AsType())!;
                    //PluginManager.LoadedPlugins.Add(plugin, path);
                    plugin.Plugin = p;
                    plugin.State = PluginState.Loaded;

                    plugin.OnInitialized();
                });*/
            } catch
            {
                SDK.logger.WriteLine(
                    ILogger.Level.Warn,
                    $"Failed to load plugin with ID \"{id}\""
                );
                UnloadPlugin(id);
            }
        }

        public static void LoadPluginDirectory(string path, bool enabled = true)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException($"\"{path}\" does not exist");

            string meta = Path.Combine(path, "meta.inf");
            if (!File.Exists(meta))
                throw new ArgumentException($"No metadata present in \"{path}\" (\"{meta}\" does not exist)");

            PluginMetadata? metadata = JsonConvert.DeserializeObject<PluginMetadata>(File.ReadAllText(meta))
                ?? throw new ArgumentException($"Metadata in \"{meta}\" is invalid");
            RegisterPlugin(metadata, path, enabled: enabled);
            // TODO: Add further check to see if the plugin is invalid or not
            if (enabled) EnablePlugin(metadata.ID);
        }

        public static void InstallPlugin(LocalInstallSource source)
        {
            // TODO
        }

        public static void InstallPlugin(InternetInstallSource source)
        {
            // TODO
        }

        public static void UninstallPlugin(string id)
        {
            if(RegisteredPlugins.TryGetValue(id, out var plugin))
            {
                switch(plugin.State)
                {
                    case PluginState.Unknown:
                    case PluginState.Invalid:
                    case PluginState.Unloaded:
                        // TODO: Find appropriate exception
                        throw new Exception($"Plugin with the ID \"{id}\" is not ready to be uninstalled");
                    case PluginState.Loaded:
                        UnloadPlugin(id);
                        UninstallPlugin(id);
                        return;
                }

                try
                {
                    Directory.Delete(id);
                } catch(Exception ex)
                {
                    SDK.logger.WriteLine(
                        ILogger.Level.Warn,
                        $"Could not fully uninstall plugin with the ID \"{id}\": {ex.Message}"
                    );
                } finally
                {
                    RegisteredPlugins.Remove(id);
                    SavePluginList();
                }
            }
            else throw new ArgumentException($"Plugin with the ID of \"{id}\" has not been registered");
        }

        static string SafePluginName(string name)
        {
            name = name.ToLower().Replace(' ', '-');
            name = Regex.Replace(name, @"[^a-zA-Z0-9.]", "");

            return name;
        }

        static string GetPluginPath(string name)
        {
            if (PluginPath == null) return null!;
            return Path.Combine(PluginPath, SafePluginName(name));
        }

        static string GetPluginPath(PluginMetadata metadata)
        {
            return GetPluginPath(metadata.Name);
        }

        public static void LoadPluginList()
        {
            static void GenerateDefault()
            {
                Dictionary<string, PluginItem> plugins = new();

                foreach (var item in Directory.GetDirectories(PluginPath))
                {
                    string meta = Path.Combine(item, "meta.inf");
                    if (!File.Exists(meta))
                        throw new ArgumentException($"No metadata present in \"{item}\" (\"{meta}\" does not exist)");

                    PluginMetadata? metadata = JsonConvert.DeserializeObject<PluginMetadata>(File.ReadAllText(meta))
                        ?? throw new ArgumentException($"Metadata in \"{meta}\" is invalid");
                    
                    plugins.Add(
                        metadata.ID,
                        new PluginItem
                        {
                            Path = item,
                            Enabled = true
                        }
                    );
                }

                SavePluginList(plugins);
                foreach (var plugin in plugins)
                    LoadPluginDirectory(plugin.Value.Path, plugin.Value.Enabled);
            }

            if (File.Exists(PluginListPath))
            {
                try
                {
                    var plugins = JsonConvert.DeserializeObject<Dictionary<string, PluginItem>>(File.ReadAllText(PluginListPath));
                    if (plugins == null)
                    {
                        SDK.logger.WriteLine(
                            ILogger.Level.Warn,
                            "Plugins list is null, rewriting with default values. " +
                            "The user will need to reinstall plugins."
                        );

                        GenerateDefault();
                    }
                    else
                    {
                        SDK.logger.WriteLine(
                            ILogger.Level.OK,
                            $"Found {plugins.Count} plugins"
                        );

                        foreach (var plugin in plugins)
                            LoadPluginDirectory(plugin.Value.Path, plugin.Value.Enabled);
                    }
                } catch (Exception ex)
                {
                    SDK.logger.WriteLine(
                        ILogger.Level.Error,
                        $"Failed to load plugins list: " + ex.Message
                    );
                    SDK.logger.WriteLine(
                        ILogger.Level.Warn,
                        "Rewriting plugin list with default values. " +
                        "The user will need to reinstall plugins."
                    );

                    GenerateDefault();
                }
            }
            else GenerateDefault();
        }

        public static Dictionary<string, PluginItem> GetPluginList()
        {
            Dictionary<string, PluginItem> plugins = [];
            foreach (var plugin in RegisteredPlugins)
            {
                plugins.Add(plugin.Key, new PluginItem
                {
                    Path = plugin.Value.Path,
                    Enabled = plugin.Value.Enabled,
                });
            }
            return plugins;
        }

        public static void SavePluginList(Dictionary<string, PluginItem>? list = null)
        {
            File.WriteAllText(
                PluginListPath,
                JsonConvert.SerializeObject(list ?? GetPluginList(), Formatting.Indented)
            );
        }

        /*
            Plugin method definitions
        */

        void IDisposable.Dispose() { }

        /// <summary>
        /// OnInitialized is called when the plugin is loaded.
        /// This should be used to prepare the plugin, and
        /// doing things such as:<br />
        /// - Subscribing to events<br />
        /// - Registering windows<br />
        /// - Registering dialogs<br />
        /// - Registering context menu items
        /// </summary>
        abstract void OnInitialized();

        /// <summary>
        /// OnShutdown is called when the plugin is unloaded.
        /// This should be used to save any data, and
        /// doing things such as:<br />
        /// - Unsubscribing from events<br />
        /// - Deregistering windows<br />
        /// - Deregistering dialogs<br />
        /// - Deregistering context menu items
        /// </summary>
        abstract void OnShutdown();
    }

    public enum PluginState
    {
        // Needs to be checked
        Unknown,
        // Ready to be loaded
        Ready,
        // Loaded, and in use
        Loaded,
        // Unloaded, but has reference leaks
        Unloaded,
        // Cannot be loaded, no DLL
        Invalid
    }

    public class RegisteredPlugin
    {
        /// <summary>
        /// Plugin metadata, loaded from meta.inf
        /// </summary>
        public PluginMetadata Metadata { get; set; }

        /// <summary>
        /// Plugin path, the root directory of the plugin.
        /// This should have the following files:<br />
        /// - meta.inf<br />
        /// - anycpu.dll/pluginid.dll
        /// </summary>
        public string Path { get; set; }

        public PluginLoadContext? LoadContext { get; set; }
        public WeakReference? WeakReference { get; set; }
        public IPlugin? Plugin { get; set; }

        public PluginState State { get; set; }
        public bool Enabled { get; set; }

        public RegisteredPlugin(
            PluginMetadata metadata,
            string path,
            bool enabled,

            PluginLoadContext? loadContext = null,
            WeakReference? weakReference = null,
            IPlugin? plugin = null
        )
        {
            Metadata = metadata;
            Path = path;
            State = PluginState.Unknown;

            LoadContext = loadContext;
            WeakReference = weakReference;
            Plugin = plugin;
        }
    }

    namespace Plugin
    {
        public class PluginItem
        {
            public string Path { get; set; }
            public bool Enabled { get; set; }
        }

        public class LocalInstallSource
        {
            public string Path { get; set; }
            public bool IsArchive { get; set; }

            public LocalInstallSource(string path, bool isArchive = false)
            {
                Path = path;
                IsArchive = isArchive;
            }

            public LocalInstallSource(InternetInstallSource IIS)
            {
                Path = IIS.Path;
                IsArchive = true;
            }
        }

        public class InternetInstallSource
        {
            public string URL { get; set; }
            public string Path { get; set; }

            public InternetInstallSource(string url, string path)
            {
                URL = url;
                Path = path;
            }
        }
    }
}