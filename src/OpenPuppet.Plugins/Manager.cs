using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenPuppet.Plugins.Manager;

namespace OpenPuppet.Plugins
{
    public static class Manager
    {
        /// <summary>
        /// Installs a plugin from a path, the path can either be a directory
        /// or a ZIP file.
        /// </summary>
        /// <param name="path">The path to directory/ZIP file</param>
        /*public static InstallPluginResult InstallPlugin(string path)
        {
            if (Directory.Exists(path)) return InstallPluginDir(path);
            else return InstallPluginArchive(path);
        }

        public static async Task<InstallPluginResult> InstallPluginAsync(string path)
        {
            if (Directory.Exists(path)) return await InstallPluginDirAsync(path);
            else return await InstallPluginArchiveAsync(path);
        }

        static internal InstallPluginResult InstallPluginDir(string path)
        {
            
        }

        static internal InstallPluginResult InstallPluginArchive(string path)
        {

        }

        static internal async Task<InstallPluginResult> InstallPluginDirAsync(string path)
        {

        }

        static internal async Task<InstallPluginResult> InstallPluginArchiveAsync(string path)
        {

        }

        public class InstallPluginResult
        {
            public int InstalledPlugins;
            public int FailedPlugins;
            public int TotalPlugins;

            public string[] Logs;
        }*/
    }
}