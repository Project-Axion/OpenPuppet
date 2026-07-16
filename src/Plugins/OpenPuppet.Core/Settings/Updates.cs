using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core.Settings
{
    public class Updates : ISettingsSection
    {
        public void OnOpened() { }

        public void OnRender(double deltaTime)
        {
            ImGui.Text("Current version: N/A");
        }
    }
}
