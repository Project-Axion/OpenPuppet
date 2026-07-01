using OpenPuppet;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SVG
{
    public class Plugin : IPlugin
    {
        public string PluginID { get; } = "OpenPuppet.SVG";

        public Logger.PluginLogger Logger { get; internal set; } = SDK.Logger.LogManager.RequestPluginLogger("OpenPuppet.SVG");

        public void OnInitialized()
        {
            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Hello Plugin");
        }
    }
}