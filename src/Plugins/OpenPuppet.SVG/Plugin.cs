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

        private bool shutdown = false;

        public void OnInitialized()
        {
            bool hasProject = SDK.Projects.ProjectManager.ActiveProject != null;

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Hello SVG Plugin");

            IUIDialog.Register("openpuppet.svg.import", typeof(ImportSVG));

            ContextMenu.AddMenuItem("File.Import.SVG", () => IUIDialog.Open("openpuppet.svg.import"), "Import SVG", hasProject);
        }

        public void OnShutdown()
        {
            if (shutdown) return;
            try
            {
                ContextMenu.Remove("File.Import.SVG");
            } catch { }
            Logger.Dispose();
            shutdown = true;
        }

        public void Dispose()
        {
            OnShutdown();
        }
    }
}