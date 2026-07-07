using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Preferences.Layouts
{
    public class NewLayout : IUIDialog
    {
        public string Title { get; set; } = "New Layout";

        private static string path = string.Empty;
        private static bool createDisabled = true;

        public void OnClose() { }

        public void OnLoad() { }

        public void OnPreRender() { }

        public void OnRender()
        {
            ImGui.Text("Layout name:");
            ImGui.InputText("", ref path, 1024);
            createDisabled = string.IsNullOrEmpty(path);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.SetCursorPosX(200);
            if (ImGui.Button("Cancel")) { ImGui.CloseCurrentPopup(); IUIDialog.Close(); }
            ImGui.SameLine();
            if (createDisabled) ImGui.BeginDisabled();
            if (ImGui.Button("Create") && !createDisabled)
            {

            }
            if (createDisabled) ImGui.EndDisabled();
        }
    }
}
