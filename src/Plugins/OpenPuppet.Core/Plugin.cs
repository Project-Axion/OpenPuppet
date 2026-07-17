using ImGuiNET;
using Newtonsoft.Json;
using OpenPuppet.Core.Dialogs;
using OpenPuppet.Core.Mutators;
using OpenPuppet.Core.PropertyDrawers;
using OpenPuppet.SDK;
using OpenPuppet.SDK.Easings;
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

        private bool shutdown = false;

        public void OnInitialized()
        {
            Global.MainPlugin = this;
            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Hello Core");

            IMutator<Vector3>.RegisterMutator(typeof(Vec3Mutator));
            IMutator<Vector2>.RegisterMutator(typeof(Vec2Mutator));

            IMutator<Color3>.RegisterMutator(typeof(Color3Mutator));
            IMutator<Color4>.RegisterMutator(typeof(Color4Mutator));

            if (!File.Exists(Path.Combine(SDK.SDK.DataPath,"projcache")))
                File.WriteAllText(Path.Combine(SDK.SDK.DataPath, "projcache"), "");

            Projects.RecentProjects = File.ReadAllLines(Path.Combine(SDK.SDK.DataPath, "projcache")).ToList();

            ContextMenu.AddMenuList("File");
            ContextMenu.AddMenuList("File.Import");
            ContextMenu.AddMenuList("View");
            ContextMenu.AddMenuList("View.Layouts");
            ContextMenu.AddMenuList("Project");

            IUIWindow.Register("openpuppet.core.timeline", typeof(Timeline));
            IUIWindow.Register("openpuppet.core.editor", typeof(Editor));
            IUIWindow.Register("openpuppet.core.viewport", typeof(Viewport));
            IUIWindow.Register("openpuppet.core.properties", typeof(Properties));
            IUIWindow.Register("openpuppet.settings", typeof(SettingsWindow));
            IUIWindow.Register("openpuppet.core.project", typeof(Project));
            IUIWindow.Register("openpuppet.core.hierarchy", typeof(Hierarchy));

            IUIDialog.Register("openpuppet.core.createproject", typeof(CreateProject));
            IUIDialog.Register("openpuppet.core.welcome", typeof(WelcomeDialog));

            IEasingMode.Register("openpuppet.core.linear", new LinearEasing());

            IPropertyDrawer.Register(typeof(Vector3),new Vector3Drawer());
            IPropertyDrawer.Register(typeof(Vector2),new Vector2Drawer());
            IPropertyDrawer.Register(typeof(Vector4),new Vector4Drawer());

            IPropertyDrawer.Register(typeof(Color3),new Color3Drawer());
            IPropertyDrawer.Register(typeof(Color4),new Color4Drawer());

            IPropertyDrawer.Register(typeof(float),new FloatDrawer());
            IPropertyDrawer.Register(typeof(int),new IntDrawer());

            ContextMenu.AddMenuItem("File.Save", () =>
            {
                IEvent<EventArgs>.Invoke("project.save",this,EventArgs.Empty);

                File.WriteAllText(
                    Path.Combine(ProjectManager.ActiveProject!.Directory, ProjectManager.ActiveProject.Name + ".opp"),
                    JsonConvert.SerializeObject(ProjectManager.ActiveProject, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    })
                );
            });

            ContextMenu.AddMenuItem("View.Timeline", () => IUIWindow.Open("openpuppet.core.timeline"));
            ContextMenu.AddMenuItem("View.Editor", () => IUIWindow.Open("openpuppet.core.editor"));
            ContextMenu.AddMenuItem("View.Viewport", () => IUIWindow.Open("openpuppet.core.viewport"));
            ContextMenu.AddMenuItem("View.Properties", () => IUIWindow.Open("openpuppet.core.properties"));
            ContextMenu.AddMenuItem("View.Settings", () => IUIWindow.Open("openpuppet.settings"));
            ContextMenu.AddMenuItem("Project.Settings", () => IUIWindow.Open("openpuppet.core.project"));
            ContextMenu.AddMenuItem("View.Hierarchy", () => IUIWindow.Open("openpuppet.core.hierarchy"));

            ISettingsSection.RegisterSection("General", new Settings.General());
            ISettingsSection.RegisterSection("Appearance", new Settings.Appearance());
            ISettingsSection.RegisterSection("Plugins", new Settings.Plugins());
            ISettingsSection.RegisterSection("Updates", new Settings.Updates());

            Events.Subscribe();

            if(ProjectManager.ActiveProject == null)
                IUIDialog.Open("openpuppet.core.welcome");
        }

        public void OnShutdown()
        {
            if (shutdown) return;
            Events.Unsubscribe();
            Logger.Dispose();
            shutdown = true;
        }

        public void Dispose()
        {
            OnShutdown();
        }
    }

    public static class Global
    {
        public static Plugin MainPlugin;
    }
}