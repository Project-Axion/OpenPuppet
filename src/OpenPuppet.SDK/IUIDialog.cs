using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public interface IUIDialog
    {
        public static Dictionary<string, Type> RegisteredWindows { get; } = new();
        public static IUIDialog? ActiveDialog { get; internal set; }

        public string Title { get; set; }
        public ImGuiWindowFlags? Flags { get; set; }
        public Vector2? Size { get; set; }

        void OnLoad();
        void OnClose();

        void OnPreRender();
        void OnRender();

        public static void Register(string registry, Type t)
        {
            if (t.IsAssignableTo(typeof(IUIDialog)) && t.IsClass)
                RegisteredWindows.Add(registry, t);
            else
                throw new ArgumentException($"{t.FullName} is not a class that implements the IUIDialog interface.");
        }

        public static IUIDialog SpawnFromRegistry(string registry)
        {
            if (RegisteredWindows.ContainsKey(registry))
            {
                var win = (IUIDialog)Activator.CreateInstance(RegisteredWindows[registry])!;

                return win;
            }
            else throw new ArgumentException($"No dialog registered under the registry: '{registry}'.");
        }

        public static void Open(string registry,bool causeUpdates = true)
        {
            SDK.logger.WriteLine(Logger.ILogger.Level.Log, $"Opening dialog with ID {registry}");
            var win = SpawnFromRegistry(registry);
            win.OnLoad();

            if (ActiveDialog != null && causeUpdates) ActiveDialog.OnClose();
            ActiveDialog = win;
        }

        public static void Close()
        {
            ActiveDialog?.OnClose();
            ActiveDialog = null;
        }
    }
}