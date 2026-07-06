using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public static class SDK
    {
        internal static Logger.PluginLogger logger = Logger.LogManager.RequestPluginLogger("com.openpuppet.sdk");

        public static void DestroyLogger()
        {
            logger?.Dispose();
        }

        public static string LocalDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenPuppet");
        public static string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenPuppet");
    }
}
