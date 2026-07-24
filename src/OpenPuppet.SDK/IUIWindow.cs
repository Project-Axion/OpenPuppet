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
        /// <summary>
        /// The currently active windows
        /// </summary>
        public static List<IUIWindow> ActiveWindows { get; } = new();

        /// <summary>
        /// The instance index. This is used alongside the registry key.<br />
        /// For example, this would be the '0' in "openpuppet.core.welcome##0"
        /// </summary>
        public uint InstanceIndex { get; protected set; }
        /// <summary>
        /// The window title.
        /// Set as readonly unless you need to change it
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The window flags, read the ImGui documentation for more
        /// information.
        /// Leave as null to use the default flags
        /// </summary>
        public ImGuiWindowFlags? Flags { get; set; }
        /// <summary>
        /// The window default size.
        /// Leave as null to use the default size
        /// </summary>
        public Vector2? Size { get; set; }

        /// <summary>
        /// The OnLoad method is called when the window is loaded
        /// </summary>
        void OnLoad();
        /// <summary>
        /// The OnClose method is called when the window is being closed
        /// </summary>
        void OnClose();

        /// <summary>
        /// The OnUpdate method is called when an update occurs
        /// </summary>
        /// <param name="deltaTime"></param>
        void OnUpdate(double deltaTime);
        /// <summary>
        /// The OnPreRender method is called before the OnRender method
        /// </summary>
        /// <param name="deltaTime"></param>
        void OnPreRender(double deltaTime);
        /// <summary>
        /// The OnRender method is called during rendering, you must use
        /// this to render anything you need to render in the window.<br />
        /// Never attempt to render outside of the render loop
        /// </summary>
        /// <param name="deltaTime"></param>
        void OnRender(double deltaTime);
        /// <summary>
        /// The OnPostRender method is called after the OnRender method
        /// </summary>
        /// <param name="deltaTime"></param>
        void OnPostRender(double deltaTime);

        /// <summary>
        /// Registers a window class to the registry
        /// </summary>
        /// <param name="registry">The registry key</param>
        /// <param name="t">The window class</param>
        /// <exception cref="ArgumentException">If the window class provided does not implement the IUIWindow interface</exception>
        public static void Register(string registry, Type t)
        {
            if (t.IsAssignableTo(typeof(IUIWindow)) && t.IsClass)
                RegisteredWindows.Add(registry, t);
            else
                throw new ArgumentException($"{t.FullName} is not a class that implements the IUIWindow interface.");
        }

        /// <summary>
        /// Gets the registry name (key) from a window class.<br />
        /// For example, if you register a window class called
        /// "TestWindow" with the ID of "test.window", if you used
        /// this method with the class, you would get the ID.
        /// </summary>
        /// <param name="t">The window class</param>
        /// <returns>The registry key</returns>
        /// <exception cref="Exception"></exception>
        public static string RegistryFromType(Type t)
        {
            var dat = RegisteredWindows.FirstOrDefault(x => x.Value == t, new("null", null!));

            if (dat.Value != null) return dat.Key;

            string errstring = $"Could not retrieve Registry from {t.FullName}";

            SDK.logger.WriteLine(Logger.ILogger.Level.Error, errstring);
            throw new Exception(errstring);
        }

        /// <summary>
        /// Spawns a window from an already-registered window
        /// </summary>
        /// <param name="registry">The window ID in the registry (provided in Register)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If there is no dialog with the ID provided</exception>
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

        /// <summary>
        /// Opens a window from a registry key
        /// </summary>
        /// <param name="registry">The registry key</param>
        /// <returns>The newly created window</returns>
        public static IUIWindow Open(string registry)
        {
            var win = SpawnFromRegistry(registry);
            win.OnLoad();

            ActiveWindows.Add(win);

            return win;
        }

        /// <summary>
        /// Attempt to close a currently open window
        /// </summary>
        /// <param name="registry">The registry key (See RegistryFromType)</param>
        /// <param name="id">The instance index</param>
        /// <exception cref="ArgumentException"></exception>
        public static void Close(string registry, uint id)
        {
            var item = ActiveWindows.Find(w => w.InstanceIndex == id);
            if (item == null)
                throw new ArgumentException($"Window instance \"{registry}##{id}\" does not exist");

            item.OnClose();
            ActiveWindows.Remove(item);
            WindowEvents.InvokeOnWindowClosed(null, new(RegistryFromType(item.GetType()) + "##" + item.InstanceIndex));
        }

        /// <summary>
        /// Attempt to close a currently open window
        /// </summary>
        /// <param name="window">The window</param>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Close all currently open windows
        /// </summary>
        public static void CloseAll()
        {
            ActiveWindows.ToList().ForEach(Close);
        }

        /// <summary>
        /// Deregister all currently open windows
        /// </summary>
        public static void DeregisterAll()
        {
            RegisteredWindows.Clear();
        }

        /// <summary>
        /// Tries to get a window from the registry
        /// </summary>
        /// <param name="registry">The ID of the window in the registry</param>
        /// <param name="window">The window (if it exists)</param>
        /// <returns>Whether the window exists in the registry or not</returns>
        public static bool TryGetWindow(string registry, out Type? window)
        {
            return RegisteredWindows.TryGetValue(registry, out window);
        }

        /// <summary>
        /// Returns if the registry contains the registry key
        /// </summary>
        /// <param name="registry">The registry key to check</param>
        /// <returns>Whether the registry key exists within the registry</returns>
        public static bool Contains(string registry)
        {
            return RegisteredWindows.ContainsKey(registry);
        }
    }
}
