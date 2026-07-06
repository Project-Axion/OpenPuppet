using OpenPuppet.SDK;

namespace OpenPuppet.Core
{
    public class Plugin : IPlugin
    {
        public const string PluginID = "com.openpuppet.core";

        public Logger.PluginLogger Logger { get; internal set; } = SDK.Logger.LogManager.RequestPluginLogger(PluginID);

        public void OnInitialized()
        {
            Global.MainPlugin = this;
            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Hello Core");

            ContextMenu.AddMenuList("File");
            ContextMenu.AddMenuList("View");

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

        public void OnShutdown()
        {
            Logger.Dispose();
        }
    }

    public static class Global
    {
        public static Plugin MainPlugin;
    }
}