using OpenPuppet.SDK;

namespace OpenPuppet.Preferences
{
    public class Plugin : IPlugin
    {
        public const string PluginID = "com.openpuppet.preferences";

        public Logger.PluginLogger Logger { get; internal set; } = SDK.Logger.LogManager.RequestPluginLogger(PluginID);

        public void OnInitialized()
        {
            Global.MainPlugin = this;
            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Hello Preferences Plugin");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Subscribing to window events");
            Windows.SubscribeToEvents();
        }

        public void OnShutdown()
        {
            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Saving current window layout");
            Windows.SavePreviousWindows();
            Logger.Dispose();
        }
    }

    public static class Global
    {
        public static Plugin MainPlugin;
    }
}