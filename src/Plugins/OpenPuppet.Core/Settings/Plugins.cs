using ImGuiNET;
using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core.Settings
{
    public class Plugins : ISettingsSection
    {
        public void OnRender(double deltaTime)
        {
            ImGui.BeginDisabled();
            ImGui.Button("Install plugin");
            ImGui.EndDisabled();
            ImGui.SameLine();
            ImGui.Button("Enable all");
            ImGui.SameLine();
            ImGui.Button("Disable all");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginChild(
                "Plugins",
                new System.Numerics.Vector2(0, 0),
                false,
                ImGuiWindowFlags.AlwaysVerticalScrollbar
            );

            foreach (var plugin in IPlugin.RegisteredPlugins)
            {
                //ImGui.BeginChild("Plugins-" + plugin.Key);
                ImGui.Columns(2);
                ImGui.SetColumnWidth(0, 100);
                // Draw icon
                ImGui.NextColumn();
                ImGui.Spacing();
                ImGui.Text($"{plugin.Value.Metadata.Name} ({plugin.Key})");
                ImGui.TextWrapped($"Description: {plugin.Value.Metadata.Description}");
                ImGui.Text($"Version: {plugin.Value.Metadata.Version}");
                ImGui.Text($"Author: {plugin.Value.Metadata.Author}");
                if (plugin.Key == "com.openpuppet.core") ImGui.BeginDisabled(true);
                ImGui.Button("Disable");
                ImGui.SameLine();
                ImGui.Button("Remove");
                if (plugin.Key == "com.openpuppet.core") ImGui.EndDisabled();
                ImGui.Columns(1);
                //ImGui.EndChild();
                ImGui.Separator();
            }

            ImGui.EndChild();
        }
    }
}