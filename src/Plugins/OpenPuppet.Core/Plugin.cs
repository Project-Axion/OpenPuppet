using ImGuiNET;
using Newtonsoft.Json;
using OpenPuppet.Core.Dialogs;
using OpenPuppet.Core.Mutators;
using OpenPuppet.SDK;
using OpenPuppet.SDK.Events;
using OpenPuppet.SDK.Projects;
using System.Numerics;
using System.Xml.Linq;

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

            IMutator<Vector3>.RegisterMutator(typeof(Vec3Mutator));
            IMutator<Vector2>.RegisterMutator(typeof(Vec2Mutator));

            if (!File.Exists("projcache"))
                File.WriteAllText("projcache","");

            WelcomeDialog.RecentProjects = File.ReadAllLines("projcache").ToList();

            ContextMenu.AddMenuList("File");
            ContextMenu.AddMenuList("File.Import");
            ContextMenu.AddMenuList("View");
            ContextMenu.AddMenuList("View.Layouts");
            ContextMenu.AddMenuList("Project");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering windows");
            IUIWindow.Register("openpuppet.core.timeline", typeof(Timeline));
            IUIWindow.Register("openpuppet.core.editor", typeof(Editor));
            IUIWindow.Register("openpuppet.core.viewport", typeof(Viewport));
            IUIWindow.Register("openpuppet.core.properties", typeof(Properties));
            IUIWindow.Register("openpuppet.settings", typeof(SettingsWindow));
            IUIWindow.Register("openpuppet.core.project", typeof(Project));
            IUIWindow.Register("openpuppet.core.hierarchy", typeof(Hierarchy));
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all windows successfully");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering dialogs");
            IUIDialog.Register("openpuppet.core.createproject", typeof(CreateProject));
            IUIDialog.Register("openpuppet.core.welcome", typeof(WelcomeDialog));
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all dialogs successfully");

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Registering context menu items");
            ContextMenu.AddMenuItem("File.Save", () =>
            {
                IEvent<EventArgs>.Invoke("project.save",this,EventArgs.Empty);

                File.WriteAllText(
                    Path.Combine(ProjectManager.ActiveProject!.Directory, ProjectManager.ActiveProject.Name + ".opp"),
                    JsonConvert.SerializeObject(ProjectManager.ActiveProject)
                );
            });

            ContextMenu.AddMenuItem("View.Timeline", () => IUIWindow.Open("openpuppet.core.timeline"));
            ContextMenu.AddMenuItem("View.Editor", () => IUIWindow.Open("openpuppet.core.editor"));
            ContextMenu.AddMenuItem("View.Viewport", () => IUIWindow.Open("openpuppet.core.viewport"));
            ContextMenu.AddMenuItem("View.Properties", () => IUIWindow.Open("openpuppet.core.properties"));
            ContextMenu.AddMenuItem("View.Settings", () => IUIWindow.Open("openpuppet.settings"));
            ContextMenu.AddMenuItem("Project.Settings", () => IUIWindow.Open("openpuppet.core.project"));
            ContextMenu.AddMenuItem("View.Hierarchy", () => IUIWindow.Open("openpuppet.core.hierarchy"));
            Logger.WriteLine(SDK.Logger.ILogger.Level.OK, "Registered all context menu items successfully");

            ISettingsSection.RegisterSection("General", new TestSection());
            ISettingsSection.RegisterSection("Appearance", new TestSection());
            ISettingsSection.RegisterSection("Plugins", new Settings.Plugins());

            Events.Subscribe();

            IUIDialog.Open("openpuppet.core.welcome");
        }

        public void OnShutdown()
        {
            Logger.Dispose();
        }
    }

    public class TestSection : ISettingsSection
    {
        public void OnOpened() { }

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