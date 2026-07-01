namespace OpenPuppet.Core
{
    public class Plugin : IPlugin
    {
        public string PluginID { get; } = "OpenPuppet.Core";

        public void OnInitialized()
        {
            Console.WriteLine("Hello Core");
        }
    }
}
