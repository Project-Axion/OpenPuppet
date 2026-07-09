using ImGuiNET;
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
            ContextMenu.AddMenuList("File.Import");
            ContextMenu.AddMenuList("View");
            ContextMenu.AddMenuList("View.Layouts");
            ContextMenu.AddMenuList("Project");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering windows");
            IUIWindow.Register("openpuppet.core.timeline", typeof(Timeline));
            IUIWindow.Register("openpuppet.core.editor", typeof(Editor));
            IUIWindow.Register("openpuppet.core.properties", typeof(Properties));
            IUIWindow.Register("openpuppet.settings", typeof(SettingsWindow));
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all windows successfully");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering context menu items");
            ContextMenu.AddMenuItem("View.Timeline", () => IUIWindow.Open("openpuppet.core.timeline"));
            ContextMenu.AddMenuItem("View.Editor", () => IUIWindow.Open("openpuppet.core.editor"));
            ContextMenu.AddMenuItem("View.Properties", () => IUIWindow.Open("openpuppet.core.properties"));
            ContextMenu.AddMenuItem("View.Settings", () => IUIWindow.Open("openpuppet.settings"));

            ContextMenu.AddMenuItem("Project.Create", () => { }, "Create Project", false);
            ContextMenu.AddMenuItem("Project.Open", () => { }, "Open Project", false);
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all context menu items successfully");

            ISettingsSection.RegisterSection("General",new TestSection());
            ISettingsSection.RegisterSection("Appearance",new TestSection());
        }

        public void OnShutdown()
        {
            Logger.Dispose();
        }
    }

    public class TestSection : ISettingsSection
    {
        public void OnRender(double deltaTime)
        {
            ImGui.Text("hello world");
        }
    }

    public static class Global
    {
        public static Plugin MainPlugin;
    }
}