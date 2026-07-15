using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Preferences.Settings
{
    public class Layouts : ISettingsSection
    {
        public void OnOpened() { }

        public void OnRender(double deltaTime)
        {
            if(ImGui.Button("New layout"))
            {
                IUIDialog.Open("openpuppet.layouts.new_layout");
            }
            ImGui.SameLine();
            if(ImGui.Button("Import layout"))
            {

            }
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginChild(
                "Layouts",
                new System.Numerics.Vector2(0, 0),
                false,
                ImGuiWindowFlags.AlwaysVerticalScrollbar
            );

            ImGui.EndChild();
        }
    }
}
