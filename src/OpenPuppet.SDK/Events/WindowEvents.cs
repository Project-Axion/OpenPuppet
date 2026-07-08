using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Events
{
    public static class WindowEvents
    {
        public class WindowOpenedArgs : EventArgs
        {
            public string Window;

            public WindowOpenedArgs(string window)
            {
                Window = window;
            }
        }
        public static EventHandler<WindowOpenedArgs> OnWindowOpened;
        public static void InvokeOnWindowOpened(object? sender, WindowOpenedArgs args)
        {
            OnWindowOpened?.Invoke(sender, args);
        }

        public class WindowClosedArgs : EventArgs
        {
            public string Window;

            public WindowClosedArgs(string window)
            {
                Window = window;
            }
        }
        public static EventHandler<WindowClosedArgs> OnWindowClosed;
        public static void InvokeOnWindowClosed(object? sender, WindowClosedArgs args)
        {
            OnWindowClosed?.Invoke(sender, args);
        }
    }
}