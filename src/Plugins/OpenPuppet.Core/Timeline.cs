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
        TimeSpan scroll = TimeSpan.Zero;

        float sidebarsize = 200;

        double zoom = 1d / 10d;

        bool isInDrag = false;

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
                    foreach (var item1 in item.Value) DrawTrack(item1);
            }
        }

        public void OnPostRender(double deltaTime) { }

        public void OnClose() {}

        void DrawTrack(ITimelineTrack track)
        {
            Vector2 rpos = ImGui.GetCursorPos();
            Vector2 pos = ImGui.GetCursorScreenPos() - ImGui.GetStyle().FramePadding;
            Vector2 textSize = ImGui.CalcTextSize(track.Name);
            Vector2 padding = new Vector2(24, 12);
            Vector2 rectSize = new Vector2(ImGui.GetContentRegionMax().X, textSize.Y + padding.Y * 2);

            float keyframeY = pos.Y + rectSize.Y / 2;

            ImGui.SetCursorScreenPos(new Vector2(pos.X + sidebarsize + 4, pos.Y));

            ImGui.InvisibleButton(
                $"##trackaddbtn{track.Name}{track.HolderID}" + InstanceIndex,
                rectSize
            );

            bool pendingSimpleClick = false;

            if (ImGui.IsItemActivated())
            {
                var kpos = TimeSpan.FromMilliseconds(
                    (ImGui.GetIO().MousePos.X - (pos.X + sidebarsize)) / zoom
                ) + scroll;

                if (
                    ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) &&
                    !track.KeyframeInRange(kpos, out _, 5f / (float)zoom)
                ) track.AddKeyframe(kpos);

                bool shiftHeld = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);
                bool hitKeyframe = track.KeyframeInRange(kpos, out var kf, padding.Y / (float)zoom);
                bool hitSelected = hitKeyframe && track.IsKeyframeSelected(kf);

                if (!shiftHeld && hitSelected) pendingSimpleClick = true;
                else
                {
                    if (!shiftHeld)
                        track.DeselectAll();
                    if (hitKeyframe)
                        track.ToggleSelectKeyframe(kf);
                    pendingSimpleClick = false;
                }

                isInDrag = false;
            }

            if (ImGui.IsItemActive())
            {
                if (!isInDrag && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                    isInDrag = true;

                if (isInDrag)
                {
                    var kdelta = TimeSpan.FromMilliseconds(ImGui.GetIO().MouseDelta.X / zoom);
                    if (kdelta != TimeSpan.Zero)
                    {
                        var selected = track.GetSelectedKeyframes().ToList();
                        foreach (var item in selected)
                        {
                            var off = item + kdelta;

                            if (!track.KeyframeExists(off))
                                track.MoveKeyframe(item, off);
                        }
                    }
                }
            }
            else
            {
                if (pendingSimpleClick && !isInDrag)
                {
                    var kpos = TimeSpan.FromMilliseconds(
                        (ImGui.GetIO().MousePos.X - (pos.X + sidebarsize)) / zoom
                    ) + scroll;

                    bool shiftHeld = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);
                    if (!shiftHeld && track.KeyframeInRange(kpos, out var kf, padding.Y / (float)zoom))
                    {
                        track.DeselectAll();
                        track.ToggleSelectKeyframe(kf);
                    }
                    pendingSimpleClick = false;
                }

                if (isInDrag) isInDrag = false;
            }

            ImGui.SetCursorScreenPos(new Vector2(pos.X + sidebarsize, pos.Y));

            ImGui.InvisibleButton("##splitter" + InstanceIndex, new(4, rectSize.Y));

            if (ImGui.IsItemActive()) 
                sidebarsize = Math.Max(Math.Min(ImGui.GetIO().MousePos.X - pos.X, rectSize.X - 10),10);

            ImGui.SetCursorPos(rpos);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(
                pos,
                new Vector2(pos.X + rectSize.X, pos.Y + rectSize.Y),
                ImGui.GetColorU32(ImGuiCol.TitleBg),
                0f
            );

            drawList.AddRect(
                pos + Vector2.One,
                new Vector2(pos.X + rectSize.X, pos.Y + rectSize.Y + 1),
                ImGui.GetColorU32(ImGuiCol.Border),
                0f
            );

            drawList.PushClipRect(pos, new Vector2(pos.X + sidebarsize, pos.Y + rectSize.Y));

            drawList.AddText(
                new Vector2(pos.X + padding.X, pos.Y + padding.Y),
                ImGui.GetColorU32(ImGuiCol.Text),
                track.Name
            );

            drawList.PopClipRect();

            drawList.AddLine(
                new Vector2(pos.X + sidebarsize, pos.Y),
                new Vector2(pos.X + sidebarsize, pos.Y + rectSize.Y),
                ImGui.GetColorU32(ImGui.IsItemHovered() ? ImGuiCol.SeparatorHovered : ImGuiCol.Separator)
            );

            drawList.PushClipRect(
                new Vector2(pos.X + sidebarsize, pos.Y), 
                new Vector2(pos.X + rectSize.X, pos.Y + rectSize.Y)
            );

            foreach (var item in track.GetKeyframes())
            {
                var ngonpos = new Vector2(pos.X + sidebarsize + (float)((item.frame - scroll).TotalMilliseconds * zoom), keyframeY);

                if (
                    ngonpos.X < pos.X + sidebarsize - padding.Y || 
                    ngonpos.X > pos.X + rectSize.X + padding.Y
                ) continue;

                var cola = ImGui.GetColorU32(item.selected ? ImGuiCol.FrameBgHovered : ImGuiCol.TableHeaderBg);
                var colb = ImGui.GetColorU32(item.selected ? ImGuiCol.FrameBgActive : ImGuiCol.Border);

                drawList.AddNgonFilled(ngonpos,padding.Y, cola, 4);
                drawList.AddNgon(ngonpos, padding.Y - 2, colb, 4);
            }

            drawList.PopClipRect();

            ImGui.Dummy(rectSize);
        }
    }
}
