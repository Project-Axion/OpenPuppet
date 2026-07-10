using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core
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
            if (SDK.Projects.ProjectManager.ActiveProject == null) return;
        }

        public void OnPostRender(double deltaTime) { }

        public void OnClose() { }
    }
}
