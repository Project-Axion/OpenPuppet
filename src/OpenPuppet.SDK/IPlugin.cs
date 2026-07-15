using ImGuiNET;
using Newtonsoft.Json;
using OpenPuppet.Plugins;
using OpenPuppet.SDK.Events;
using OpenPuppet.SDK.Plugins;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OpenPuppet
{
    /// <summary>
    /// Plugin interface
    /// </summary>
    public interface IPlugin : IDisposable
    {
        public static Dictionary<string, RegisteredPlugin> RegisteredPlugins { get; } = new();

        static readonly object _pluginLock = new();
        const int MaxUnloadGcAttempts = 10;

        public static void RegisterPlugin(PluginMetadata metadata, string path, IPlugin? plugin)
        {
            if (RegisteredPlugins.ContainsKey(metadata.ID))
                throw new ArgumentException($"Plugin with the ID of \"{metadata.ID}\" is already registered");
            RegisteredPlugins[metadata.ID] = new(metadata, path, null, null, plugin, false);
        }
        public static void EnablePlugin(string registry)
        {
            if (RegisteredPlugins.ContainsKey(registry))
            {
                if (RegisteredPlugins[registry].Assembly != null) return;

                string item = RegisteredPlugins[registry].Path;

                var anycpu = Path.Combine(item, $"anycpu.dll");
                var specific = Path.Combine(item, $"{RuntimeInformation.ProcessArchitecture}.dll");

                if (File.Exists(specific)) LoadAssembly(specific, registry);
                else if (File.Exists(anycpu)) LoadAssembly(anycpu, registry);
                else Console.WriteLine($"Warning: could not load {Path.GetFileName(item)}: " +
                    $"no dll found for architecture '{RuntimeInformation.ProcessArchitecture}'");
            }
            else throw new ArgumentException($"Plugin with the ID of\"{registry}\" has not been registered");
        }
        public static void DisablePlugin(string registry)
        {
            if (RegisteredPlugins.ContainsKey(registry))
            {
                if (RegisteredPlugins[registry].Assembly == null) return;

                //RegisteredPlugins[registry].Assembly!.OnShutdown();
                RegisteredPlugins[registry].Enabled = false; // Temporary, move to unloading method
                UnloadPlugin(registry);
            }
            else throw new ArgumentException($"Plugin with the ID of \"{registry}\" has not been registered");
        }
        public static void RemovePlugin(string registry)
        {
            if (RegisteredPlugins.ContainsKey(registry))
            {
                SDK.SDK.logger.WriteLine(
                    SDK.Logger.ILogger.Level.Warn,
                    "Removing plugins has not yet been fully implemented"
                );
                // Uninstall
                RegisteredPlugins.Remove(registry);
            }
            else throw new ArgumentException($"Plugin with the ID of \"{registry}\" has not been registered");
        }

        public static bool UnloadPlugin(string registry, bool soft = false)
        {
            lock (_pluginLock)
            {
                if (!RegisteredPlugins.TryGetValue(registry, out var plugin))
                    throw new ArgumentException($"Plugin with the ID of \"{registry}\" has not been registered");

                if (plugin.Assembly == null ||
                    plugin.LoadContext == null ||
                    plugin.WeakReference == null)
                {
                    SDK.SDK.logger.WriteLine(
                        SDK.Logger.ILogger.Level.Log,
                        $"Plugin with the ID of \"{registry}\" cannot be unloaded, as it is not loaded"
                    );

                    RegisteredPlugins.Remove(registry);
                    return true;
                }

                var weakRef = plugin.WeakReference;
                var loadContext = plugin.LoadContext;

                try
                {
                    plugin.Assembly.OnShutdown();
                }
                catch (Exception ex)
                {
                    SDK.SDK.logger.WriteLine(
                        SDK.Logger.ILogger.Level.Error,
                        $"Plugin \"{registry}\" threw during OnShutdown: {ex}"
                    );
                }
                finally
                {
                    plugin.Assembly = null;
                }

                try
                {
                    loadContext.Unload();
                }
                finally
                {
                    plugin.LoadContext = null;
                }

                for (int i = 0; weakRef.IsAlive && i < MaxUnloadGcAttempts; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                bool leaked = weakRef.IsAlive;

                if (leaked)
                {
                    SDK.SDK.logger.WriteLine(
                        SDK.Logger.ILogger.Level.Warn,
                        $"Could not unload Plugin \"{registry}\", possible reference leak"
                    );

                    if (soft)
                        IEvent<bool>.Invoke("openpuppet.restart", null, false);
                    else
                        IEvent<EventArgs>.Invoke("openpuppet.unstable", null, EventArgs.Empty);
                }

                RegisteredPlugins.Remove(registry);
                return !leaked;
            }
        }

        static void LoadAssembly(string path, string registry)
        {
            if (!RegisteredPlugins.ContainsKey(registry))
                throw new ArgumentException($"Plugin with the ID of \"{registry}\" has not been registered");

            // Remove the logs once fixed

            SDK.SDK.logger.WriteLine($"Loading assembly {path}");
            RegisteredPlugins[registry].LoadContext = new PluginLoadContext(path);
            Assembly asm = RegisteredPlugins[registry].LoadContext!.LoadFromAssemblyPath(path);
            SDK.SDK.logger.WriteLine($"Initializing Plugin {registry}");
            asm.DefinedTypes.Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass).ToList().ForEach(t =>
            {
                SDK.SDK.logger.WriteLine($"Found IPlugin in Plugin {registry}");
                RegisteredPlugins[registry].Assembly = (IPlugin)Activator.CreateInstance(t.AsType())!;
                RegisteredPlugins[registry].Assembly!.OnInitialized();
                SDK.SDK.logger.WriteLine($"Initialized Plugin {registry}");
            });
            /*foreach(Type t in asm.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && t.IsClass)
                {
                    SDK.SDK.logger.WriteLine($"Found IPlugin in Plugin {registry}");
                    RegisteredPlugins[registry].Assembly = (IPlugin)Activator.CreateInstance(t)!;
                    RegisteredPlugins[registry].Assembly!.OnInitialized();
                    SDK.SDK.logger.WriteLine($"Initialised Plugin {registry}");
                }
            }*/
            RegisteredPlugins[registry].WeakReference = new(
                RegisteredPlugins[registry].LoadContext,
                trackResurrection: true
            );

            /*var asm = Assembly.LoadFrom(path);
            asm.DefinedTypes.Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass).ToList().ForEach(t =>
            {
                var plugin = (IPlugin)Activator.CreateInstance(t.AsType())!;
                //PluginManager.LoadedPlugins.Add(plugin, path);
                RegisteredPlugins[registry].Assembly = plugin;
                RegisteredPlugins[registry].Enabled = true;

                plugin.OnInitialized();
            });*/

            RegisteredPlugins[registry].Enabled = true;
        }

        /// <summary>
        /// Load a plugin from a directory.
        /// This was previously in Program.cs, but has been moved here so that
        /// plugins can be loaded later on.
        /// </summary>
        /// <param name="path">The path of the directory</param>
        public static void LoadPluginDirectory(string path, bool enabled = true)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException($"\"{path}\" does not exist");

            string meta = Path.Combine(path, "meta.inf");
            if (!File.Exists(meta))
                throw new ArgumentException($"No metadata present in \"{path}\" (\"{meta}\" does not exist)");

            PluginMetadata? metadata = JsonConvert.DeserializeObject<PluginMetadata>(File.ReadAllText(meta));
            if(metadata == null)
                throw new ArgumentException($"Metadata in \"{meta}\" is invalid");

            RegisterPlugin(metadata, path, null);
            if(enabled) EnablePlugin(metadata.ID);
        }

        /// <summary>
        /// The plugin ID
        /// </summary>
        public const string PluginID = "com.unknown.unknown";

        public SDK.Logger.PluginLogger Logger { get; }

        /// <summary>
        /// OnInitialized
        /// This is called when the plugin is loaded, use this to prepare the plugin
        /// </summary>
        abstract void OnInitialized();

        /// <summary>
        /// OnShutdown
        /// This is called when the plugin is being unloaded, use this to save any data or configuration
        /// </summary>
        abstract void OnShutdown();
    }

    /// <summary>
    /// Plugin metadata file
    /// </summary>
    public class PluginMetadata
    {
        public string ID                { get; set; } = string.Empty;
        public string Name              { get; set; } = string.Empty;
        public string Description       { get; set; } = string.Empty;
        public string Author            { get; set; } = string.Empty;
        public string Version           { get; set; } = string.Empty;
        public string Icon              { get; set; } = string.Empty;
    }

    public class RegisteredPlugin
    {
        public PluginMetadata Metadata { get; set; }
        public string Path { get; set; }
        public PluginLoadContext? LoadContext { get; set; }
        public WeakReference? WeakReference { get; set; }
        public IPlugin? Assembly { get; set; }
        public bool Enabled { get; set; }

        public RegisteredPlugin(
            PluginMetadata metadata,
            string path,
            PluginLoadContext? loadContext,
            WeakReference? weakReference,
            IPlugin? assembly,
            bool enabled)
        {
            Metadata = metadata;
            Path = path;
            LoadContext = loadContext;
            WeakReference = weakReference;
            Assembly = assembly;
            Enabled = enabled;
        }
    }
}