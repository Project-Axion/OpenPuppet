using ImGuiNET;
using OpenPuppet.Plugins;
using OpenPuppet.SDK;
using OpenPuppet.SDK.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core.Settings
{
    public class Plugins : ISettingsSection
    {
        public bool WarnRestart = false;

        public void OnOpened()
        {
            
        }

        public void OnRender(double deltaTime)
        {
            ImGui.BeginDisabled();
            ImGui.Button("Install plugin");
            ImGui.EndDisabled();
            ImGui.SameLine();
            if(ImGui.Button("Enable all"))
            {
                foreach (var plugin in IPlugin.RegisteredPlugins)
                {
                    if (plugin.Key == "com.openpuppet.core") continue;
                    PluginManager.SetPluginEnabled(plugin.Key, true);
                }
            }
            ImGui.SameLine();
            if(ImGui.Button("Disable all"))
            {
                foreach (var plugin in IPlugin.RegisteredPlugins)
                {
                    if (plugin.Key == "com.openpuppet.core") continue;
                    PluginManager.SetPluginEnabled(plugin.Key, false);
                    WarnRestart = true;
                }
            }
            ImGui.SameLine();
            if(ImGui.Button("Uninstall all"))
            {
                foreach (var plugin in IPlugin.RegisteredPlugins)
                {
                    if (plugin.Key == "com.openpuppet.core") continue;
                    PluginManager.UninstallPlugin(plugin.Key);
                    WarnRestart = true;
                }
            }
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if(WarnRestart)
            {
                ImGui.TextColored(
                    System.Numerics.Vector4.Create(255, 255, 0, 255),
                    "You have made changes that require a restart to take full effect. " +
                    "Please restart now."
                );
                if(ImGui.Button("Restart"))
                {
                    IEvent<bool>.Invoke("openpuppet.restart", this, false);
                }
                ImGui.SameLine();
                if(ImGui.Button("Soft Restart (Experimental)"))
                {
                    IEvent<bool>.Invoke("openpuppet.restart", this, true);
                }

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }

            ImGui.BeginChild(
                "Plugins",
                new System.Numerics.Vector2(0, 0),
                false,
                ImGuiWindowFlags.AlwaysVerticalScrollbar
            );

            // This can probably be redone as a table at some point
            foreach (var plugin in IPlugin.RegisteredPlugins)
            {
                bool disabled = plugin.Key == "com.openpuppet.core";
                bool enabled = plugin.Value.Enabled;
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
                if (disabled)
                    ImGui.BeginDisabled(true);
                if(ImGui.Button(enabled ? "Disable" : "Enable") && !disabled)
                {
                    PluginManager.SetPluginEnabled(plugin.Key, !enabled);
                    if(enabled) WarnRestart = true;
                }
                ImGui.SameLine();
                if(ImGui.Button("Uninstall") && !disabled)
                {
                    PluginManager.UninstallPlugin(plugin.Key);
                    WarnRestart = true;
                }
                if(disabled)
                {
                    ImGui.EndDisabled();
                    ImGui.SameLine();
                    ImGui.TextColored(
                        System.Numerics.Vector4.Create(255, 255, 0, 255),
                        "Protected Plugin"
                    );
                }
                ImGui.Columns(1);
                //ImGui.EndChild();
                ImGui.Separator();
            }

            ImGui.EndChild();
        }
    }
}