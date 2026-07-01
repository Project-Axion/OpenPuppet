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

        public void OnLoad()
        {
            Global.MainPlugin.Logger.WriteLine(Logger.ILogger.Level.OK, "Successfully created timeline window");
        }

        public void OnUpdate(double deltaTime) {}

        public void OnPreRender(double deltaTime) { }

        public void OnRender(double deltaTime)
        {
            ImGui.Text("the windowing stuff works");
        }

        public void OnPostRender(double deltaTime) { }

        public void OnClose() {}
    }
}
