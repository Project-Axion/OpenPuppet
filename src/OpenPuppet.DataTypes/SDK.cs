using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public static class SDK
    {
        internal static Logger.PluginLogger logger = Logger.LogManager.RequestPluginLogger("openpuppet.sdk");

        public static void DestroyLogger()
        {
            logger?.Dispose();
        }
    }
}
