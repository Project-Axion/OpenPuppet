using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Projects.Dialogs
{
    public class OpenProject : IUIDialog
    {
        public string Title { get; set; } = "Open Project";
        public ImGuiWindowFlags? Flags { get; set; } = ImGuiWindowFlags.NoResize;
        public Vector2? Size { get; set; } = new Vector2(600, 450);

        public void OnClose()
        {
            
        }

        public void OnLoad()
        {
            
        }

        public void OnPreRender()
        {
            
        }

        public void OnRender()
        {
            ImGui.Columns(2, "openpuppet.projects.openproject.recentprojects", false);
            ImGui.SetColumnWidth(0, 450);
            ImGui.SetColumnWidth(1, 150);

            ImGui.Text("Recent projects");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.NextColumn();

            if(ImGui.Button("Create project", Vector2.Create(120, 20)))
            {
                IUIDialog.Close();
                SDK.Events.IEvent<EventArgs>.Invoke("openpuppet.projects.create", this, EventArgs.Empty);
            }
            ImGui.Button("Open folder", Vector2.Create(120, 20));
            ImGui.Button("Open project", Vector2.Create(120, 20));
        }
    }
}
