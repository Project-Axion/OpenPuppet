using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Projects.Dialogs
{
    public class OpenProject : IUIDialog
    {
        public string Title { get; set; } = "Open Project";

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
            ImGui.Text("Yes");
            ImGui.Columns();
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 300);
            ImGui.SetColumnWidth(1, 100);

            ImGui.Text("Recent projects");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.NextColumn();

            ImGui.SetNextItemWidth(100);
            ImGui.Button("Open folder");
            ImGui.SetNextItemWidth(100);
            ImGui.Button("Open project file");
        }
    }
}
