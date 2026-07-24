using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Events
{
    public interface IEvent<T>
    {
        private static Dictionary<string, Action<object?, T>> RegisteredEvents { get; } = new();

        void OnLoad();
        void OnFired(object? sender, T e);

        public static void Invoke(string registry, object? sender, T e)
        {
            if (RegisteredEvents.TryGetValue(registry, out var callback))
                callback?.Invoke(sender, e);
        }

        public static void Subscribe(string registry, Action<object?, T> callback)
        {
            if (RegisteredEvents.TryGetValue(registry, out var existing))
                RegisteredEvents[registry] = existing + callback;
            else
                RegisteredEvents[registry] = callback;
        }

        public static void Unsubscribe(string registry, Action<object?, T> callback)
        {
            if (RegisteredEvents.TryGetValue(registry, out var existing))
            {
                existing -= callback;

                if (existing == null)
                    RegisteredEvents.Remove(registry);
                else
                    RegisteredEvents[registry] = existing;
            }
        }

        public static void Clear()
        {
            RegisteredEvents.Clear();
        }
    }
}