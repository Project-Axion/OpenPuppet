using ImGuiNET;
using OpenPuppet.rendering.VertexTypes;
using OpenPuppet.SDK;
using OpenPuppet.SDK.GameObject;
using OpenPuppet.SDK.Projects;
using OpenPuppet.vector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SVG
{
    public class ImportSVG : IUIDialog
    {
        public string Title { get; set; } = "Import SVG";
        public ImGuiWindowFlags? Flags { get; set; } = null;
        public Vector2? Size { get; set; } = null;

        private static string path = string.Empty;
        private static bool openDisabled = true;

        public void OnClose() { }

        public void OnLoad() { }

        public void OnPreRender() { }

        public void OnRender()
        {
            ImGui.Text("SVG Path:");
            ImGui.InputText("", ref path, 1024);
            openDisabled = string.IsNullOrEmpty(path) || !File.Exists(path);
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
            if (ImGui.Button("Open") && !openDisabled)
            {
                var vector = SVGImporter.ImportSVG(path);

                var scene = ProjectManager.ActiveProject!.Scenes[ProjectManager.ActiveProject.ActiveScene];

                var vecpath = Path.Combine(
                    ProjectManager.ActiveProject.Directory,
                    "Vectors", $"{Path.GetFileNameWithoutExtension(path)}.ovec"
                );

                IVectorASTComponent.SaveToDisk(vector, vecpath);
                scene.SceneObjects.Add(new VectorGameObject<ColorVertex>(vecpath));
            }
            if (openDisabled) ImGui.EndDisabled();
        }
    }
}
