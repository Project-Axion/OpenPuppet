using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core
{
    public class Properties : IUIWindow
    {
        public uint IstanceIndex { get; set; }
        public string Title { get; set; } = "Properties";

        public void OnLoad() { }

        public void OnUpdate(double deltaTime) { }

        public void OnPreRender(double deltaTime) { }

        public void OnRender(double deltaTime)
        {
            
        }

        public void OnPostRender(double deltaTime) { }

        public void OnClose() { }
    }
}
