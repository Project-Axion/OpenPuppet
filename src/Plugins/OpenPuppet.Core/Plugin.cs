using OpenPuppet.SDK;

namespace OpenPuppet.Core
{
    public class Plugin : IPlugin
    {
        public string PluginID { get; } = "OpenPuppet.Core";

        public Logger.PluginLogger Logger { get; internal set; } = SDK.Logger.LogManager.RequestPluginLogger("OpenPuppet.Core");

        public void OnInitialized()
        {
            Global.MainPlugin = this;
            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Hello Core");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering windows");
            IUIWindow.Register("openpuppet.core.timeline", typeof(Timeline));
            IUIWindow.Register("openpuppet.core.editor", typeof(Editor));
            IUIWindow.Register("openpuppet.core.properties", typeof(Properties));
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all windows successfully");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering context menu items");
            ContextMenu.AddMenuItem("View.Timeline", () => IUIWindow.Open("openpuppet.core.timeline"));
            ContextMenu.AddMenuItem("View.Editor", () => IUIWindow.Open("openpuppet.core.editor"));
            ContextMenu.AddMenuItem("View.Properties", () => IUIWindow.Open("openpuppet.core.properties"));
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all context menu items successfully");
        }
    }

    public static class Global
    {
        public static Plugin MainPlugin;
    }
}
