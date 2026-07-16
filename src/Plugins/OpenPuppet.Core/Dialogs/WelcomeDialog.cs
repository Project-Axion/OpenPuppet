using ImGuiNET;
using Newtonsoft.Json;
using OpenPuppet.SDK;
using OpenPuppet.SDK.Events;
using OpenPuppet.SDK.Projects;
using Silk.NET.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenPuppet.Core.Dialogs
{
    public class WelcomeDialog : IUIDialog
    {
        public string Title { get; set; } = "Welcome";
        public ImGuiWindowFlags? Flags { get; set; } = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        public Vector2? Size { get; set; } = new Vector2(640, 480);

        bool NoProjectMode = false;

        const string NoProjString = "Continue without a project";

        public void OnLoad()
        {
            ContextMenu.SetEnabledAll(false);
            ContextMenu.SetEnabled("View.Settings", true);
        }

        public void OnPreRender()
        {
            
        }

        public void OnRender()
        {
            if (ImGui.BeginChild("projectview", ImGui.GetContentRegionAvail() / new Vector2(1,1.2f)))
            {
                var cardsize = ImGui.GetWindowSize() / new Vector2(4f,2f);

                if (ImGui.Button("+",cardsize))
                    IUIDialog.Open("openpuppet.core.createproject", false);

                ImGui.SameLine();

                if (ImGui.Button("open", cardsize))
                {
                    NativeDialogs.OpenFileResult result = NativeDialogs.OpenFileDialog("opp", null);
                    if (NativeDialogs.OpenFileDialogResultHasPath(result))
                    {
                        var json = JsonConvert.DeserializeObject<ProjectMetadata>(
                            File.ReadAllText(result.Path!),
                            new JsonSerializerSettings()
                            {
                                TypeNameHandling = TypeNameHandling.Auto
                            }
                        )!;

                        json.Directory = Directory.GetParent(result.Path!)!.FullName;

                        Projects.OpenProject(json);

                        IUIDialog.Close();
                    }
                }

                ImGui.SameLine();

                foreach (var item in Projects.RecentProjects.ToArray())
                {
                    if (ImGui.Button(Path.GetFileName(item), cardsize))
                    {
                        if (!File.Exists(item))
                        {
                            Projects.RecentProjects.Remove(item);
                            continue;
                        }

                        var json = JsonConvert.DeserializeObject<ProjectMetadata>(
                            File.ReadAllText(item),
                            new JsonSerializerSettings()
                            {
                                TypeNameHandling = TypeNameHandling.Auto
                            }
                        )!;

                        json.Directory = Directory.GetParent(item)!.FullName;

                        Projects.OpenProject(json);

                        IUIDialog.Close();
                    }

                    if (ImGui.GetCursorPosX() < ImGui.GetContentRegionAvail().X)
                        ImGui.SameLine();
                }

                ImGui.EndChild();
            }

            ImGui.PushStyleColor(ImGuiCol.Button,0);

            ImGui.SetCursorPos(
                ImGui.GetContentRegionMax() - 
                (ImGui.CalcTextSize(NoProjString) + new Vector2(5,5))
            );

            if (ImGui.Button(NoProjString))
            {
                NoProjectMode = true;
                IUIDialog.Close();
            }

            ImGui.PopStyleColor();
        }

        public void OnClose()
        {
            if (ProjectManager.ActiveProject == null && !NoProjectMode) 
                IEvent<EventArgs>.Invoke("openpuppet.quit", this, null!);

            if (!NoProjectMode) ContextMenu.SetEnabledAll(true);
        }
    }
}
