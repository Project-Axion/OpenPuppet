using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public interface IMutator<T>
    {
        public static Dictionary<Type, Type> Mutators { get; } = new();

        public static void RegisterMutator(Type mutator)
        {
            if (!typeof(IMutator<T>).IsAssignableFrom(mutator))
                throw new ArgumentException($"Mutator type must implement IMutator interface. Type: {mutator.FullName}");

            if (!Mutators.ContainsKey(typeof(T)))
                Mutators[typeof(T)] = mutator;
        }

        public static IMutator<T> GetMutator()
        {
            if (Mutators.TryGetValue(typeof(T), out var mutatorType))
                return (IMutator<T>)Activator.CreateInstance(mutatorType)!;

            return null!;
        }

        public T Mutate(T a, T b, double factor);
    }
}
