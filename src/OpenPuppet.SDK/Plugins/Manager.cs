using Newtonsoft.Json;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenPuppet.Plugins
{
    public static class PluginManager
    {
        //public static List<IPlugin> LoadedPlugins { get; internal set; } = new();
        public static List<PluginItem> PluginList { get; set; } = new();
        public static string? PluginListPath
        {
            get
            {
                return Path.Combine(PluginsPath.PluginPath, "plugins.json");
            }
        }

        public class PluginItem
        {
            public string ID        { get; set; }
            public string Path      { get; set; }

            public bool Enabled     { get; set; }
        }

        public static void LoadPluginList()
        {
            if (PluginListPath == null)
                throw new Exception("\"PluginListPath\" is null");
            if(File.Exists(PluginListPath))
            {
                PluginItem[]? plugins = JsonConvert.DeserializeObject<PluginItem[]>(File.ReadAllText(PluginListPath));
                if(plugins == null)
                {
                    SDK.SDK.logger.WriteLine(
                        Logger.ILogger.Level.Warn,
                        "Plugins list is null, rewriting with default values. " +
                        "The user will need to reinstall plugins."
                    );

                    GenerateDefaultPluginList();
                    SavePluginList();
                } else
                {
                    PluginList.AddRange(plugins);
                }
            } else
            {
                GenerateDefaultPluginList();
                SavePluginList();
            }
        }

        public static void SavePluginList()
        {
            if (PluginListPath == null)
                throw new Exception("\"PluginListPath\" is null");
            File.WriteAllText(
                PluginListPath,
                JsonConvert.SerializeObject(PluginList, Formatting.Indented)
            );
        }

        public static void GenerateDefaultPluginList()
        {
            // This just puts the currently available/loaded plugins into the list
            foreach (var item in Directory.GetDirectories(PluginsPath.PluginPath!))
            {
                string meta = Path.Combine(item, "meta.inf");
                if (!File.Exists(meta))
                    throw new ArgumentException($"No metadata present in \"{item}\" (\"{meta}\" does not exist)");

                PluginMetadata? metadata = JsonConvert.DeserializeObject<PluginMetadata>(File.ReadAllText(meta));
                if (metadata == null)
                    throw new ArgumentException($"Metadata in \"{meta}\" is invalid");

                PluginList.Add(new PluginItem
                {
                    ID = metadata.ID,
                    Path = item,
                    Enabled = true
                });
            }
        }

        /// <summary>
        /// Installs a plugin from a path, the path can either be a directory
        /// or a ZIP file.
        /// </summary>
        /// <param name="path">The path to directory/ZIP file</param>
        public static InstallPluginResult InstallPlugin(string path)
        {
            if (Directory.Exists(path)) return InstallPluginDir(path);
            else return InstallPluginArchive(path);
        }

        static internal InstallPluginResult InstallPluginDir(string path)
        {
            throw new NotImplementedException();
        }

        static internal InstallPluginResult InstallPluginArchive(string path)
        {
            throw new NotImplementedException();
        }

        public class InstallPluginResult
        {
            public int InstalledPlugins;
            public int FailedPlugins;
            public int TotalPlugins;

            public string[] Logs = [];
        }

        public static void SetPluginEnabled(string ID, bool enabled)
        {
            if(enabled) IPlugin.EnablePlugin(ID);
            else IPlugin.DisablePlugin(ID);

            var plugin = PluginList.Find(p => p.ID == ID);
            if(plugin == null)
            {
                SDK.SDK.logger.WriteLine(
                    Logger.ILogger.Level.Warn,
                    "Plugin with ID \"{ID}\" is not present in the plugin list. " +
                    "The plugin list, and the currently loaded plugins may be desynced."
                );
            } else
            {
                plugin.Enabled = enabled;
            }
        }

        public static void UninstallPlugin(string ID)
        {
            IPlugin.DisablePlugin(ID);
            IPlugin.RemovePlugin(ID);
        }
    }
}