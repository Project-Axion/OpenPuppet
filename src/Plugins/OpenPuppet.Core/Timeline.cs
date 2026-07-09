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
    public class Timeline : IUIWindow
    {
        public uint InstanceIndex { get; set; }
        public string Title { get; set; } = "Timeline";

        public ImGuiWindowFlags? Flags { get; set; } = null;
        public Vector2? Size { get; set; } = null;

        public void OnLoad()
        {
            Global.MainPlugin.Logger.WriteLine(Logger.ILogger.Level.OK, "Successfully created timeline window");
        }

        public void OnUpdate(double deltaTime) {}

        public void OnPreRender(double deltaTime) { }

        public void OnRender(double deltaTime)
        {
            Vector2 vec = ImGui.GetCursorPos();
            ImGui.Columns(3, null, false);
            ImGui.Separator();
            ImGui.NextColumn();
            ImGui.Button("Play");
        }

        public void OnPostRender(double deltaTime) { }

        public void OnClose() {}
    }
}
