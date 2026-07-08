using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public interface ISettingsSection
    {
        public static Dictionary<string, ISettingsSection> RegisteredSections { get; } = new();

        public static void RegisterSection(string name, ISettingsSection section) => RegisteredSections[name] = section;

        public void OnRender(double deltaTime);
    }
}
