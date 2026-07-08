using OpenPuppet.Projects.Dialogs;
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

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering windows");
            IUIWindow.Register("openpuppet.projects.project", typeof(Project));
            IUIWindow.Register("openpuppet.projects.hierarchy", typeof(Hierarchy));
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all windows successfully");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering dialogs");
            IUIDialog.Register("openpuppet.projects.createproject", typeof(CreateProject));
            IUIDialog.Register("openpuppet.projects.openproject", typeof(OpenProject));
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Reigstered all dialogs successfully");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering context menu items");
            ContextMenu.AddMenuItem("View.Project", () => IUIWindow.Open("openpuppet.projects.project"));
            ContextMenu.AddMenuItem("View.Hierarchy", () => IUIWindow.Open("openpuppet.projects.hierarchy"));

            ContextMenu.AddMenuList("project");

            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all context menu items successfully");

            Events.Subscribe();
            IEvent<string>.Invoke("openpuppet.window.modify.title", this, "OpenPuppet [No Project]");
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