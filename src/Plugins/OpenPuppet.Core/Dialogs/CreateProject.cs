using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core.Dialogs
{
    public class CreateProject : IUIDialog
    {
        public string Title { get; set; } = "Create Project";
        public ImGuiWindowFlags? Flags { get; set; } = null;
        public Vector2? Size { get; set; } = null;

        private static string name = "New project";
        private static string path = string.Empty;
        private static bool openDisabled = true;

        public void OnLoad()
        {
            
        }

        public void OnPreRender()
        {
            
        }

        public void OnRender()
        {
            ImGui.Text("Project name:");
            ImGui.InputText("##nameinp", ref name, 1024);

            ImGui.Text("Project Path:");
            ImGui.InputText("##pathinp", ref path, 1024);

            openDisabled = string.IsNullOrEmpty(path) || File.Exists(path);

            ImGui.SameLine();

            if (ImGui.Button("Browse..."))
            {
                NativeDialogs.OpenFileResult result = NativeDialogs.OpenFileDialog(null, openDisabled ? null : path);
                if (NativeDialogs.OpenFileDialogResultHasPath(result))
                {
                    path = result.Path ?? "";
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.SetCursorPosX(200);
            if (ImGui.Button("Cancel")) { ImGui.CloseCurrentPopup(); IUIDialog.Close(); }

            ImGui.SameLine();

            if (openDisabled) ImGui.BeginDisabled();

            if (ImGui.Button("Create"))
            {
                
            }

            if (openDisabled) ImGui.EndDisabled();
        }

        public void OnClose()
        {

        }
    }
}
