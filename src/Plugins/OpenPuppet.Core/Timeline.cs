using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core
{
    public class Timeline : IUIWindow
    {
        public uint IstanceIndex { get; set; }
        public string Title { get; set; } = "Timeline";

        public void OnLoad() {}

        public void OnUpdate(double deltaTime) {}

        public void OnRender(double deltaTime) 
        {
            ImGui.Text("the windowing stuff works");
        }

        public void OnClose() {}
    }
}
