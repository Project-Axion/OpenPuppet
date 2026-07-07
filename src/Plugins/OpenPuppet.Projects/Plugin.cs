using OpenPuppet.SDK;
using OpenPuppet.SDK.Events;

namespace OpenPuppet.Projects
{
    public class Plugin : IPlugin
    {
        public const string PluginID = "com.openpuppet.projects";

        public Logger.PluginLogger Logger { get; internal set; } = SDK.Logger.LogManager.RequestPluginLogger(PluginID);

        public void OnInitialized()
        {
            Global.MainPlugin = this;
            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Hello Projects Plugin");

            ContextMenu.AddMenuList("project");

            IEvent<string>.Invoke("main_window.change_name", this, "OpenPuppet [No Project]");
        }

        public void OnShutdown()
        {
            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Saving current window layout");
            Logger.Dispose();
        }
    }

    public static class Global
    {
        public static Plugin MainPlugin;
    }
}