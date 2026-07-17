using ImGuiNET;
using Microsoft.Win32;
using OpenPuppet.SDK.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public interface IUIWindow
    {
        private static Dictionary<string, Type> RegisteredWindows { get; } = new();
        public static List<IUIWindow> ActiveWindows { get; } = new();

        public uint InstanceIndex { get; protected set; }
        public string Title { get; set; }
        public ImGuiWindowFlags? Flags { get; set; }
        public Vector2? Size { get; set; }

        void OnLoad();
        void OnClose();

        void OnUpdate(double deltaTime);
        void OnPreRender(double deltaTime);
        void OnRender(double deltaTime);
        void OnPostRender(double deltaTime);

        public static void Register(string registry,Type t)
        {
            if (t.IsAssignableTo(typeof(IUIWindow)) && t.IsClass)
                RegisteredWindows.Add(registry, t);
            else 
                throw new ArgumentException($"{t.FullName} is not a class that implements the IUIWindow interface.");
        }

        public static string RegistryFromType(Type t)
        {
            var dat = RegisteredWindows.FirstOrDefault(x => x.Value == t,new("null", null!));

            if (dat.Value != null) return dat.Key;

            string errstring = $"Could not retrive Registry from {t.FullName}";

            SDK.logger.WriteLine(Logger.ILogger.Level.Error, errstring);
            throw new Exception(errstring);
        }

        public static IUIWindow SpawnFromRegistry(string registry)
        {
            if (RegisteredWindows.TryGetValue(registry, out Type? item))
            {
                var win = (IUIWindow)Activator.CreateInstance(item)!;
                win.InstanceIndex = (uint)ActiveWindows.Where(w => w.GetType() == win.GetType()).Count();

                WindowEvents.InvokeOnWindowOpened(null, new(registry + "##" + win.InstanceIndex));

                return win;
            }
            else throw new ArgumentException($"No window registered under the registry: '{registry}'.");
        }

        public static void Open(string registry)
        {
            var win = SpawnFromRegistry(registry);
            win.OnLoad();

            ActiveWindows.Add(win);
        }

        public static void Close(string registry, uint id)
        {
            var item = ActiveWindows.Find(w => w.InstanceIndex == id);
            if (item == null)
                throw new ArgumentException($"Window instance \"{registry}##{id}\" does not exist");
            
            item.OnClose();
            ActiveWindows.Remove(item);
            WindowEvents.InvokeOnWindowClosed(null, new(RegistryFromType(item.GetType()) + "##" + item.InstanceIndex));
        }

        public static void Close(IUIWindow window)
        {
            if (ActiveWindows.Contains(window))
            {
                window.OnClose();
                ActiveWindows.Remove(window);
                WindowEvents.InvokeOnWindowClosed(null, new(RegistryFromType(window.GetType()) + "##" + window.InstanceIndex));
            }
            else
                throw new ArgumentException($"Window \"{window.InstanceIndex}\" does not exist");
        }

        public static void CloseAll()
        {
            ActiveWindows.ToList().ForEach(Close);
        }

        public static void DeregisterAll()
        {
            RegisteredWindows.Clear();
        }
    }
}
