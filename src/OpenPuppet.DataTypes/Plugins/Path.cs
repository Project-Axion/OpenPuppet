using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenPuppet.Plugins
{
    public static class PluginsPath
    {
        internal static string? InstallPath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public static string? PluginPath
        { 
            get
            {
                return InstallPath != null ? Path.Combine(InstallPath, "Plugins") : null;
            }
        }

        public static string SafePluginName(string name)
        {
            name = name.ToLower().Replace(' ', '-');
            name = Regex.Replace(name, @"[^a-zA-Z0-9.]", "");

            return name;
        }

        public static string GetPluginPath(string name)
        {
            if (PluginPath == null) return null!;
            return System.IO.Path.Combine(PluginPath, SafePluginName(name));
        }

        public static string GetPluginPath(PluginMetadata metadata)
        {
            return GetPluginPath(metadata.Name);
        }
    }
}