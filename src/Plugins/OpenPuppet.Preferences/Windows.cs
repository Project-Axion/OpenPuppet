using ImGuiNET;
using OpenPuppet.SDK;
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
        private static string LayoutDir = Path.Combine(SDK.SDK.DataPath, "Preferences", "layouts");
        private static bool IsLoaded = false; // This fixes the event handling running twice

        public static void SubscribeToEvents()
        {
            SDK.Events.WindowEvents.OnWindowOpened += OnWindowOpened;
            SDK.Events.WindowEvents.OnWindowClosed += OnWindowClosed;
            SDK.Events.PluginEvents.OnFinishedLoading += OnFinishedLoading;
        }

        public static void UnsubscribeToEvents()
        {
            SDK.Events.WindowEvents.OnWindowOpened -= OnWindowOpened;
            SDK.Events.WindowEvents.OnWindowClosed -= OnWindowClosed;
            SDK.Events.PluginEvents.OnFinishedLoading -= OnFinishedLoading;
        }

        public static void OnWindowOpened(object? sender, SDK.Events.WindowEvents.WindowOpenedArgs args)
        {
            if (!OpenWindows.Contains(args.Window))
                OpenWindows.Add(args.Window);
        }

        public static void OnWindowClosed(object? sender, SDK.Events.WindowEvents.WindowClosedArgs args)
        {
            if (OpenWindows.Contains(args.Window))
                OpenWindows.Remove(args.Window);
        }

        public static void OnFinishedLoading(object? sender, EventArgs args)
        {
            if (IsLoaded) return;
            Global.MainPlugin.Logger.WriteLine(SDK.Logger.ILogger.Level.Log, "Attempting to load previous layout");
            OpenPreviousWindows();
            OpenLayout("default");
            IsLoaded = true;
        }

        public static void OpenPreviousWindows()
        {
            if (!File.Exists(WindowsFile))
            {
                SavePreviousWindows();
            }
            else
            {
                try
                {
                    string[]? data = JsonSerializer.Deserialize<string[]>(File.ReadAllText(WindowsFile));
                    if (data == null)
                    {
                        Global.MainPlugin.Logger.WriteLine(Logger.ILogger.Level.Error, "Failed to load last opened windows");
                        SavePreviousWindows();
                    }
                    else
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            IUIWindow.Open(data[i].Split("#")[0]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Global.MainPlugin.Logger.WriteLine(Logger.ILogger.Level.Error, "Failed to load last opened windows: " + ex.Message);
                    SavePreviousWindows();
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

        public static bool OpenLayout(string id)
        {
            if (!File.Exists(Path.Combine(LayoutDir, $"{id}.ini"))) return false;

            try
            {
                string file = File.ReadAllText(Path.Combine(LayoutDir, $"{id}.ini"));
                ImGui.LoadIniSettingsFromMemory(file);

                return true;
            }
            catch (Exception ex)
            {
                Global.MainPlugin.Logger.WriteLine(SDK.Logger.ILogger.Level.Error, "Failed to load ImGui layout: " + ex.Message);
                return false;
            }
        }

        public static void SaveLayout(string id)
        {
            if (!Directory.Exists(LayoutDir))
                Directory.CreateDirectory(LayoutDir);

            File.WriteAllText(Path.Combine(LayoutDir, $"{id}.ini"), ImGui.SaveIniSettingsToMemory());
        }
    }
}