using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        void OnPreRender(double deltaTime);
        void OnRender(double deltaTime);
        void OnPostRender(double deltaTime);

        public static void Register(string registry,Type t)
        {
            SDK.logger.WriteLine(Logger.ILogger.Level.Log, $"Registering window with ID {registry}");

            if (t.IsAssignableTo(typeof(IUIWindow)) && t.IsClass)
                RegisteredWindows.Add(registry, t);
            else 
                throw new ArgumentException($"{t.FullName} is not a class that implements the IUIWindow interface.");
        }

        public static string RegistryFromType(Type t)
        {
            var dat = RegisteredWindows.FirstOrDefault(x => x.Value == t,new("null",null!));

            if (dat.Value != null) return dat.Key;

            string errstring = $"Could not retrive Registry from {t.FullName}";

            SDK.logger.WriteLine(Logger.ILogger.Level.Error, errstring);
            throw new Exception(errstring);
        }

        public static IUIWindow SpawnFromRegistry(string registry)
        {
            SDK.logger.WriteLine(Logger.ILogger.Level.Log, $"Spawning window from registry with ID {registry}");
            if (RegisteredWindows.ContainsKey(registry))
            {
                var win = (IUIWindow)Activator.CreateInstance(RegisteredWindows[registry])!;
                win.IstanceIndex = (uint)ActiveWindows.Where(w => w.GetType() == win.GetType()).Count();

                Events.WindowEvents.InvokeOnWindowOpened(null, new(registry + "##" + win.IstanceIndex));

                return win;
            }
            else throw new ArgumentException($"No window registered under the registry: '{registry}'.");
        }

        public static void Open(string registry)
        {
            SDK.logger.WriteLine(Logger.ILogger.Level.Log, $"Opening window with ID {registry}");
            var win = SpawnFromRegistry(registry);
            win.OnLoad();

            ActiveWindows.Add(win);
        }
    }
}
