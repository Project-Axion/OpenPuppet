using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Plugins
{
    public static class Manager
    {
        /// <summary>
        /// Installs a plugin from a path, the path can either be a directory
        /// or a ZIP file.
        /// </summary>
        /// <param name="path">The path to directory/ZIP file</param>
        public static void InstallPlugin(string path)
        {
            if (Directory.Exists(path)) InstallPluginDir(path);
            else InstallPluginArchive(path);
        }

        public static async Task InstallPluginAsync(string path)
        {
            return;
        }

        static internal void InstallPluginDir(string path)
        {
            
        }

        static internal void InstallPluginArchive(string path)
        {

        }
    }
}