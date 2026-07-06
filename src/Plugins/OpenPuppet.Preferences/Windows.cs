using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace OpenPuppet.Preferences
{
    public static class Windows
    {
        private static List<string> OpenWindows = new();
        private static string WindowsFile = Path.Combine(SDK.SDK.DataPath, "Preferences", "last_windows.json");

        public static void SubscribeToEvents()
        {
            SDK.Events.WindowEvents.OnWindowOpened += OnWindowOpened;
            SDK.Events.WindowEvents.OnWindowClosed += OnWindowClosed;
            SDK.Events.PluginEvents.OnFinishedLoading += OnFinishedLoading;
        }

        public static void OnWindowOpened(object? sender, SDK.Events.WindowEvents.WindowOpenedArgs args)
        {
            Global.MainPlugin.Logger.WriteLine("Window opened: " + args.Window);
            if(!OpenWindows.Contains(args.Window))
                OpenWindows.Add(args.Window);
        }

        public static void OnWindowClosed(object? sender, SDK.Events.WindowEvents.WindowClosedArgs args)
        {
            if(OpenWindows.Contains(args.Window))
                OpenWindows.Remove(args.Window);
        }

        public static void OnFinishedLoading(object? sender, EventArgs args)
        {
            OpenPreviousWindows();
        }

        public static void OpenPreviousWindows()
        {
            if(!File.Exists(WindowsFile))
            {
                OpenWindows.Add("openpuppet.core.editor");
                OpenWindows.Add("openpuppet.core.timeline");
                OpenWindows.Add("openpuppet.core.properties");
                SavePreviousWindows();
                OpenPreviousWindows();
            } else
            {
                string[]? data = JsonSerializer.Deserialize<string[]>(File.ReadAllText(WindowsFile));
                if(data == null)
                {
                    Global.MainPlugin.Logger.WriteLine(SDK.Logger.ILogger.Level.Error, "Failed to load last opened windows");
                } else
                {
                    for(int i = 0; i < data.Length; i++)
                    {
                        Global.MainPlugin.Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Opening window " + data[i]);
                        SDK.IUIWindow.Open(data[i].Split("#")[0]);
                    }
                }
            }
        }

        public static void SavePreviousWindows()
        {
            if (!Directory.Exists(Path.GetDirectoryName(WindowsFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(WindowsFile));

            File.WriteAllText(
                WindowsFile,
                JsonSerializer.Serialize(
                    OpenWindows.ToArray(),
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                )
            );
        }
    }
}