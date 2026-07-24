using ImGuiNET;
using OpenPuppet.SDK;
using OpenPuppet.SDK.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core.Settings
{
    public class General : ISettingsSection
    {
        public void OnOpened() { }

        public void OnRender(double deltaTime)
        {
            ImGui.Text("No settings here yet");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.TextColored(new(1, 0.25f, 0.25f, 1), "Actions");
            if (ImGui.Button("Restart"))
            {
                IEvent<bool>.Invoke("openpuppet.restart", this, false);
            }
            ImGui.SameLine();
            if (ImGui.Button("Soft Restart"))
            {
                IEvent<bool>.Invoke("openpuppet.restart", this, true);
            }
            ImGui.SameLine();
            if (ImGui.Button("Reset"))
            {

            }
        }
    }
}
