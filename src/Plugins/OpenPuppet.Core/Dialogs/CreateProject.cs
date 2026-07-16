using ImGuiNET;
using Newtonsoft.Json;
using OpenPuppet.SDK;
using OpenPuppet.SDK.Events;
using OpenPuppet.SDK.Projects;
using OpenPuppet.SDK.TimelineTracks;
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
                var result = NativeDialogs.OpenDirectoryDialog(openDisabled ? null : path);
                if (NativeDialogs.OpenDirectoryDialogResultHasPath(result))
                {
                    path = result.Path ?? "";
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.SetCursorPosX(200);

            if (ImGui.Button("Cancel")) IUIDialog.Close();

            ImGui.SameLine();

            if (openDisabled) ImGui.BeginDisabled();

            if (ImGui.Button("Create"))
            {
                var proj = new ProjectMetadata()
                {
                    Name = name,
                    Directory = path,
                };

                proj.Scenes.Add(new());

                proj.Scenes[0].SceneObjects.Add(ISceneGameObject.Scene);
                proj.Scenes[0].AnimationScene.Add(ISceneGameObject.Scene.ID, new());

                proj.Scenes[0].AnimationScene[ISceneGameObject.Scene.ID].Add(
                    new PropertyTimeline<Vector3>(
                        ISceneGameObject.Scene.ID,
                        proj.Scenes[0],
                        "Letterbox color",
                        () => proj.Scenes[0].LetterboxColor
                    )
                    {
                        Keyframes = new()
                        {
                            {new(0,0,0),Vector3.Zero},
                            {new(0,0,1),Vector3.One},
                        }
                    }
                );

                WelcomeDialog.OpenProject(proj);

                File.WriteAllText(
                    Path.Combine(path,name+".opp"),
                    JsonConvert.SerializeObject(ProjectManager.ActiveProject,new JsonSerializerSettings() 
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    })
                );

                IUIDialog.Close();
            }

            if (openDisabled) ImGui.EndDisabled();
        }

        public void OnClose()
        {

        }
    }
}
