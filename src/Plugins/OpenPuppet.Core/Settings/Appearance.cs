using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core.Settings
{
    public class Appearance : ISettingsSection
    {
        public void OnOpened() { }

        public void OnRender(double deltaTime)
        {
            ImGui.Text("Customize how OpenPuppet looks for you");
        }
    }
}
