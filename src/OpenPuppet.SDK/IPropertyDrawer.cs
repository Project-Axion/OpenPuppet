using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public interface IPropertyDrawer
    {
        public static Dictionary<Type, IPropertyDrawer> Drawers { get; } = new();

        public bool Draw(string name,ref object data);

        public static void Register(Type key,IPropertyDrawer drawer) => Drawers[key] = drawer;
    }
}
