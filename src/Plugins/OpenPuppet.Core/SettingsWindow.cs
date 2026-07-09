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
    internal class SettingsWindow : IUIWindow
    {
        public uint InstanceIndex { get; set; }
        public string Title { get; set; } = "Settings";

        public ImGuiWindowFlags? Flags { get; set; } = null;
        public Vector2? Size { get; set; } = null;

        string selected = "";
        float sidebarsize = 160f;

        public void OnLoad() { }

        public void OnUpdate(double deltaTime) { }

        public void OnPreRender(double deltaTime) {}

        public void OnRender(double deltaTime)
        {
            var cats = ISettingsSection.RegisteredSections.Keys.ToArray();

            float rowHeight = ImGui.GetFontSize() * 2;

            ImGui.BeginChild("Sidebar##sidebar" + InstanceIndex, new(sidebarsize, 0),false);
            for (int i = 0; i < cats.Length; i++)
            {
                bool isSelected = (selected == cats[i]);

                Vector2 startPos = ImGui.GetCursorPos();

                if (ImGui.Selectable("##" + (i + InstanceIndex * 1000), isSelected, ImGuiSelectableFlags.None, new Vector2(0, rowHeight)))
                    selected = cats[i];

                Vector2 nextRowPos = ImGui.GetCursorPos();

                float textHeight = ImGui.GetTextLineHeight();
                float textOffsetY = (rowHeight - textHeight) * 0.5f;

                ImGui.SetCursorPos(new Vector2(startPos.X + 8, startPos.Y + textOffsetY));
                ImGui.Text(cats[i]);

                ImGui.SetCursorPos(nextRowPos);

                ImGui.PopID();
            }
            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.InvisibleButton("##splitter" + InstanceIndex, new(4, ImGui.GetContentRegionAvail().Y));

            if (ImGui.IsItemActive()) sidebarsize += ImGui.GetIO().MouseDelta.X;

            var p0 = ImGui.GetItemRectMin();
            var p1 = ImGui.GetItemRectMax();
            float midX = (p0.X + p1.X) * 0.5f;
            ImGui.GetWindowDrawList().AddLine(
                new(midX, p0.Y), new(midX, p1.Y),
                ImGui.GetColorU32(ImGui.IsItemHovered() ? ImGuiCol.SeparatorHovered : ImGuiCol.Separator),
                1.0f
            );

            ImGui.SameLine();

            ImGui.BeginChild("Content##content" + InstanceIndex, new(0, 0), false);

            if (!ISettingsSection.RegisteredSections.ContainsKey(selected))
                selected = cats.FirstOrDefault("");

            if (!string.IsNullOrEmpty(selected))
                ISettingsSection.RegisteredSections[selected].OnRender(deltaTime);

            ImGui.EndChild();
        }

        public void OnPostRender(double deltaTime) {}

        public void OnClose() { }
    }
}
