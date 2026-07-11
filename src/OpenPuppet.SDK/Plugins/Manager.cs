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
                PluginItem[]? plugins = JsonSerializer.Deserialize<PluginItem[]>(File.ReadAllText(PluginListPath));
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
                JsonSerializer.Serialize(PluginList)
            );
        }

        public static void GenerateDefaultPluginList()
        {
            // This just puts the currently available/loaded plugins into the list
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
        }

        public static void UninstallPlugin(string ID)
        {
            IPlugin.DisablePlugin(ID);
            IPlugin.RemovePlugin(ID);
        }
    }
}