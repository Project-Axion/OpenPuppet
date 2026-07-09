using ImGuiNET;
using OpenPuppet.SDK;
using OpenPuppet.SDK.Events;
using OpenPuppet.SDK.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core.Dialogs
{
    public class WelcomeDialog : IUIDialog
    {
        public string Title { get; set; } = "Welcome";
        public ImGuiWindowFlags? Flags { get; set; } = ImGuiWindowFlags.NoResize;
        public Vector2? Size { get; set; } = new Vector2(640, 480);

        bool NoProjectMode = false;

        const string NoProjString = "continue without a project";

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
            ImGui.PushStyleColor(ImGuiCol.Button,0);

            var cpos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(ImGui.GetContentRegionAvail() - ImGui.CalcTextSize(NoProjString));

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
