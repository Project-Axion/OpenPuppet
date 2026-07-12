using ImGuiNET;
using OpenPuppet.SDK;
using OpenPuppet.SDK.Projects;
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

        public ImGuiWindowFlags? Flags { get; set; } = ImGuiWindowFlags.MenuBar;
        public Vector2? Size { get; set; } = null;

        Timer timer = null!;

        public void OnLoad() {}

        public void OnUpdate(double deltaTime) {}

        public void OnPreRender(double deltaTime) { }

        public void OnRender(double deltaTime)
        {
            if (
                ProjectManager.ActiveProject == null ||
                ProjectManager.ActiveProject!.ActiveScene >= ProjectManager.ActiveProject!.Scenes.Count
            )
            {
                ImGui.SetCursorPos(ImGui.GetContentRegionAvail() / 2 - ImGui.CalcTextSize("No active scene") / 2);

                ImGui.Text("No active scene");

                return;
            }

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.Button("Play")) {}

                ImGui.EndMenuBar();
            }

            var scene = ProjectManager.ActiveProject!.Scenes[ProjectManager.ActiveProject!.ActiveScene];

            foreach (var item in scene.AnimationScene)
            {
                if (ImGui.CollapsingHeader(scene.SceneObjects.First(x => x.ID == item.Key).Name))
                    foreach (var item1 in item.Value) DrawTrack(item1.Name);
            }
        }

        public void OnPostRender(double deltaTime) { }

        public void OnClose() {}

        static void DrawTrack(string name)
        {
            Vector2 pos = ImGui.GetCursorScreenPos();
            Vector2 textSize = ImGui.CalcTextSize(name);
            Vector2 padding = new Vector2(6, 3);
            Vector2 rectSize = new Vector2(textSize.X + padding.X * 2, textSize.Y + padding.Y * 2);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(
                pos,
                new Vector2(pos.X + rectSize.X, pos.Y + rectSize.Y),
                ImGui.GetColorU32(ImGuiCol.TitleBg),
                0f
            );
            drawList.AddText(
                new Vector2(pos.X + padding.X, pos.Y + padding.Y),
                ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f)),
                name
            );

            ImGui.Dummy(rectSize);
        }
    }
}
