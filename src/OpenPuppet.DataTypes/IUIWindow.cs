using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public interface IUIWindow
    {
        public static Dictionary<string,Type> RegisteredWindows { get; } = new();
        public static List<IUIWindow> ActiveWindows { get; } = new();

        public uint IstanceIndex { get; protected set; }
        public string Title { get; set; }

        void OnLoad();
        void OnClose();

        void OnUpdate(double deltaTime);
        void OnRender(double deltaTime);

        public static void Register(string registry,Type t)
        {
            if (t.IsAssignableTo(typeof(IUIWindow)) && t.IsClass)
                RegisteredWindows.Add(registry, t);
            else 
                throw new ArgumentException($"{t.FullName} is not a class that implements the IUIWindow interface.");
        }

        public static IUIWindow SpawnFromRegistry(string registry)
        {
            if (RegisteredWindows.ContainsKey(registry))
            {
                var win = (IUIWindow)Activator.CreateInstance(RegisteredWindows[registry])!;
                win.IstanceIndex = (uint)ActiveWindows.Where(w => w.GetType() == win.GetType()).Count();

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
    }
}
