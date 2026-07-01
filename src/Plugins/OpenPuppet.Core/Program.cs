using OpenPuppet.SDK;

namespace OpenPuppet.Core
{
    public class Plugin : IPlugin
    {
        public string PluginID { get; } = "OpenPuppet.Core";

        public Logger.PluginLogger Logger { get; internal set; } = SDK.Logger.LogManager.RequestPluginLogger("OpenPuppet.Core");

        public void OnInitialized()
        {
            Console.WriteLine("Hello Core");

            IUIWindow.Register("openpuppet.core.timeline", typeof(Timeline));

            ContexMenu.AddMenuItem("View.Timeline", () => IUIWindow.Open("openpuppet.core.timeline"));

            Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Hello Core");
        }
    }
}
