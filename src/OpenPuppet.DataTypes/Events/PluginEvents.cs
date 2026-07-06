using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Events
{
    public static class PluginEvents
    {
        public static EventHandler OnFinishedLoading;
        public static void InvokeFinishedLoading(object? sender)
        {
            OnFinishedLoading?.Invoke(sender, EventArgs.Empty);
        }
    }
}
