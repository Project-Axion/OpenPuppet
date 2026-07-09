using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Projects
{
    public class Project : IUIWindow
    {
        public uint InstanceIndex { get; set; }
        public string Title { get; set; } = "Project";
        public ImGuiWindowFlags? Flags { get; set; } = null;
        public Vector2? Size { get; set; } = null;

        public void OnLoad() { }

        public void OnUpdate(double deltaTime) { }

        public void OnPreRender(double deltaTime) { }

        public void OnRender(double deltaTime)
        {
            if (SDK.Projects.ProjectManager.ActiveProject == null)
            {
                ImGui.Spacing();
                ImGui.Text("No project loaded");
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.TextWrapped("Create or load an existing project to get started");
                ImGui.Spacing();
                if (ImGui.Button("Create project"))
                {
                    SDK.Events.IEvent<EventArgs>.Invoke("openpuppet.projects.create", this, EventArgs.Empty);
                }
                ImGui.SameLine();
                if (ImGui.Button("Open existing project"))
                {
                    SDK.Events.IEvent<EventArgs>.Invoke("openpuppet.projects.open", this, EventArgs.Empty);
                }
            }
        }

        public void OnPostRender(double deltaTime) { }

        public void OnClose() { }
    }
}
