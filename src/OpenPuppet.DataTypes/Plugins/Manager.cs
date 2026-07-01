using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Plugins
{
    public static class PluginManager
    {
        public static List<IPlugin> LoadedPlugins { get; internal set; } = new();

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
    }
}