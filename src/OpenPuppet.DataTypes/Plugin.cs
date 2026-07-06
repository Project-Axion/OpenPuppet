namespace OpenPuppet
{
    /// <summary>
    /// Plugin interface
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// The plugin ID
        /// </summary>
        public const string PluginID = "com.unknown.unknown";

        public SDK.Logger.PluginLogger Logger { get; }

        /// <summary>
        /// OnInitialized
        /// This is called when the plugin is loaded, use this to prepare the plugin
        /// </summary>
        abstract void OnInitialized();
    }

    /// <summary>
    /// Plugin metadata file
    /// </summary>
    public class PluginMetadata
    {
        public string Name              { get; set; } = string.Empty;
        public string Description       { get; set; } = string.Empty;
        public string Author            { get; set; } = string.Empty;
        public string Version           { get; set; } = string.Empty;
        public string Icon              { get; set; } = string.Empty;
    }
}