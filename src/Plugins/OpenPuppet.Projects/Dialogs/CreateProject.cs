using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Projects.Dialogs
{
    public class CreateProject : IUIDialog
    {
        public string Title { get; set; } = "Create Project";

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
            ImGui.Text("Hello world!");
        }
    }
}
