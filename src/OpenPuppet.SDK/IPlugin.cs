using ImGuiNET;
using OpenPuppet.Plugins;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenPuppet
{
    /// <summary>
    /// Plugin interface
    /// </summary>
    public interface IPlugin
    {
        public static Dictionary<string, RegisteredPlugin> RegisteredPlugins { get; } = new();
        public static void RegisterPlugin(PluginMetadata metadata, string path, IPlugin? plugin)
        {
            if (RegisteredPlugins.ContainsKey(metadata.ID))
                throw new ArgumentException($"Plugin with the ID of \"{metadata.ID}\" is already registered");
            RegisteredPlugins[metadata.ID] = new(metadata, path, plugin, false);
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

                SDK.SDK.logger.WriteLine(
                    SDK.Logger.ILogger.Level.Warn,
                    "Disabling plugins has not yet been implemented"
                );
                RegisteredPlugins[registry].Assembly.OnShutdown();
                RegisteredPlugins[registry].Enabled = false; // Temporary, move to unloading method
                // Unload plugin
            }
            else throw new ArgumentException($"Plugin with the ID of\"{registry}\" has not been registered");
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
            else throw new ArgumentException($"Plugin with the ID of\"{registry}\" has not been registered");
        }

        static void LoadAssembly(string path, string registry)
        {
            var asm = Assembly.LoadFrom(path);
            asm.DefinedTypes.Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass).ToList().ForEach(t =>
            {
                var plugin = (IPlugin)Activator.CreateInstance(t.AsType())!;
                //PluginManager.LoadedPlugins.Add(plugin, path);
                RegisteredPlugins[registry].Assembly = plugin;
                RegisteredPlugins[registry].Enabled = true;

                plugin.OnInitialized();
            });
        }

        /// <summary>
        /// Load a plugin from a directory.
        /// This was previously in Program.cs, but has been moved here so that
        /// plugins can be loaded later on.
        /// </summary>
        /// <param name="path">The path of the directory</param>
        public static void LoadPluginDirectory(string path)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException($"\"{path}\" does not exist");

            string meta = Path.Combine(path, "meta.inf");
            if (!File.Exists(meta))
                throw new ArgumentException($"No metadata present in \"{path}\" (\"{meta}\" does not exist)");

            PluginMetadata? metadata = JsonSerializer.Deserialize<PluginMetadata>(File.ReadAllText(meta));
            if(metadata == null)
                throw new ArgumentException($"Metadata in \"{meta}\" is invalid");

            RegisterPlugin(metadata, path, null);
            EnablePlugin(metadata.ID);
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
        // E.g. "com.openpuppet.core"
        [JsonPropertyName("id")]
        public string ID                { get; set; } = string.Empty;
        // E.g. "OpenPuppet Core"
        [JsonPropertyName("name")]
        public string Name              { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description       { get; set; } = string.Empty;
        [JsonPropertyName("author")]
        public string Author            { get; set; } = string.Empty;
        [JsonPropertyName("version")]
        public string Version           { get; set; } = string.Empty;
        [JsonPropertyName("icon")]
        public string Icon              { get; set; } = string.Empty;
    }

    public class RegisteredPlugin
    {
        public PluginMetadata Metadata { get; set; }
        public string Path { get; set; }
        public IPlugin? Assembly { get; set; }
        public bool Enabled { get; set; }

        public RegisteredPlugin(PluginMetadata metadata, string path, IPlugin? assembly, bool enabled)
        {
            Metadata = metadata;
            Path = path;
            Assembly = assembly;
            Enabled = enabled;
        }
    }
}