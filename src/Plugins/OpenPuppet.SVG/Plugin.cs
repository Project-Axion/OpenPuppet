using ImGuiNET;
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
        public const string PluginID = "com.openpuppet.svg";

        public Logger.PluginLogger Logger { get; internal set; } = SDK.Logger.LogManager.RequestPluginLogger(PluginID);

        public void OnInitialized()
        {
            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Hello SVG Plugin");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering dialogs");
            IUIDialog.Register("openpuppet.svg.import", typeof(ImportSVG));
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all dialogs successfully");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering context menu items");
            ContextMenu.AddMenuItem("File.Import.SVG", () => IUIDialog.Open("openpuppet.svg.import"));
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all context menu items successfully");
        }
    }
}