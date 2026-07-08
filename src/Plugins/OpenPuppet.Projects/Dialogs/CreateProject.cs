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
    public class CreateProject : IUIDialog
    {
        public string Title { get; set; } = "Create Project";
        public ImGuiWindowFlags? Flags { get; set; } = null;
        public Vector2? Size { get; set; } = null;

        // TODO: Update this so that it gets fetch from and pushed into preferences
        public static bool LoadTemplates = false;

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
            if(LoadTemplates)
            {

            } else
            {

            }
        }
    }
}
