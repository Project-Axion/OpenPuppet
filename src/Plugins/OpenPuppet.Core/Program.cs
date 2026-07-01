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
        }
    }
}
