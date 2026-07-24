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
        private static Dictionary<string, Type> RegisteredWindows { get; } = new();
        /// <summary>
        /// The currently active dialog
        /// </summary>
        public static IUIDialog? ActiveDialog { get; internal set; }

        /// <summary>
        /// The dialog window title.
        /// Set as readonly unless you need to change it
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The dialog window flags, read the ImGui documentation for more
        /// information.
        /// Leave as null to use the default flags
        /// </summary>
        public ImGuiWindowFlags? Flags { get; set; }
        /// <summary>
        /// The dialog window default size.
        /// Leave as null to use the default size
        /// </summary>
        public Vector2? Size { get; set; }

        /// <summary>
        /// The OnLoad method is called when the dialog is loaded
        /// </summary>
        void OnLoad();
        /// <summary>
        /// The OnClose method is called when the dialog is being closed
        /// </summary>
        void OnClose();

        /// <summary>
        /// The OnPreRender method is called before the OnRender method
        /// </summary>
        void OnPreRender();
        /// <summary>
        /// The OnRender method is called during rendering, you must use
        /// this to render something.
        /// Dialogs do not work properly unless you render something
        /// </summary>
        void OnRender();

        /// <summary>
        /// Registers a dialog class to the registry
        /// </summary>
        /// <param name="registry">The registry key</param>
        /// <param name="t">The dialog class</param>
        /// <exception cref="ArgumentException">If the dialog class provided does not implement the IUIDialog interface</exception>
        public static void Register(string registry, Type t)
        {
            if (t.IsAssignableTo(typeof(IUIDialog)) && t.IsClass)
            {
                if (RegisteredWindows.ContainsKey(registry))
                    SDK.logger.WriteLine(
                        Logger.ILogger.Level.Warn,
                        $"A dialog with registry: '{registry}' already exists, overriding"
                    );

                RegisteredWindows[registry] = t;
            }
            else throw new ArgumentException($"{t.FullName} is not a class that implements the IUIDialog interface.");
        }

        /// <summary>
        /// Spawns a dialog from an already-registered dialog
        /// </summary>
        /// <param name="registry">The dialog ID in the registry (provided in Register)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If there is no dialog with the ID provided</exception>
        public static IUIDialog SpawnFromRegistry(string registry)
        {
            if (RegisteredWindows.TryGetValue(registry, out Type? item))
            {
                var win = (IUIDialog)Activator.CreateInstance(item)!;

                return win;
            }
            else throw new ArgumentException($"No dialog registered under the registry: '{registry}'.");
        }

        /// <summary>
        /// Opens a dialog from a registry key
        /// </summary>
        /// <param name="registry">The dialog ID in the registry</param>
        /// <param name="causeUpdates"></param>
        /// <returns>The newly created dialog</returns>
        public static IUIDialog Open(string registry, bool causeUpdates = true)
        {
            var win = SpawnFromRegistry(registry);
            win.OnLoad();

            if (ActiveDialog != null && causeUpdates) ActiveDialog.OnClose();
            ActiveDialog = win;

            return win;
        }

        /// <summary>
        /// Close the currently active dialog
        /// </summary>
        public static void Close()
        {
            ActiveDialog?.OnClose();
            ActiveDialog = null;
        }

        /// <summary>
        /// Deregister all currently registered dialogs
        /// </summary>
        public static void DeregisterAll()
        {
            RegisteredWindows.Clear();
        }

        /// <summary>
        /// Tries to get a dialog from the registry
        /// </summary>
        /// <param name="registry">The ID of the dialog in the registry</param>
        /// <param name="dialog">The dialog (if it exists)</param>
        /// <returns>Whether the dialog exists in the registry or not</returns>
        public static bool TryGetDialog(string registry, out Type? dialog)
        {
            return RegisteredWindows.TryGetValue(registry, out dialog);
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