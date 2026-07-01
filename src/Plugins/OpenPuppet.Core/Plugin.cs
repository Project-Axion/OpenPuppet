using OpenPuppet.SDK;

namespace OpenPuppet.Core
{
    public class Plugin : IPlugin
    {
        public string PluginID { get; } = "OpenPuppet.Core";

        public Logger.PluginLogger Logger { get; internal set; } = SDK.Logger.LogManager.RequestPluginLogger("OpenPuppet.Core");

        public void OnInitialized()
        {
            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Hello Core");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering windows");
            IUIWindow.Register("openpuppet.core.timeline", typeof(Timeline));
            IUIWindow.Register("openpuppet.core.editor", typeof(Editor));
            IUIWindow.Register("openpuppet.core.properties", typeof(Properties));
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all windows successfully");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering context menu items");
            ContexMenu.AddMenuItem("View.Timeline", () => IUIWindow.Open("openpuppet.core.timeline"));
            ContexMenu.AddMenuItem("View.Editor", () => IUIWindow.Open("openpuppet.core.editor"));
            ContexMenu.AddMenuItem("View.Properties", () => IUIWindow.Open("openpuppet.core.properties"));
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all context menu items successfully");
        }
    }
}
