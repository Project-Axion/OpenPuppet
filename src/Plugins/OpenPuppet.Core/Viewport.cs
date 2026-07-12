using ImGuiNET;
using OpenPuppet.rendering;
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
    public class Viewport : IUIWindow
    {
        public uint InstanceIndex { get; set; }
        public string Title { get; set; } = "Viewport";
        public ImGuiWindowFlags? Flags { get; set; } = null;
        public Vector2? Size { get; set; } = null;

        RenderSurface surface = null!;
        Camera camera = null!;

        public void OnLoad() 
        {
            camera = new Camera(Vector2.One);

            surface = new RenderSurface(camera);
            RenderSurface.Register(surface);
        }

        public void OnUpdate(double deltaTime) 
        {
            if (
                ProjectManager.ActiveProject != null && 
                ProjectManager.ActiveProject!.ActiveScene < ProjectManager.ActiveProject!.Scenes.Count
            )
            {
                var scamera = ProjectManager.ActiveProject!.Scenes[ProjectManager.ActiveProject!.ActiveScene].SceneCamera;

                camera.Pan = scamera.Pan;

                camera.Zoom = scamera.Zoom;

                if (camera.Resolution != scamera.Resolution)
                {
                    camera.Resolution = scamera.Resolution;
                    surface.Resize(camera.Resolution);
                }
            }
        }

        public void OnPreRender(double deltaTime) 
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            if (
                ProjectManager.ActiveProject != null &&
                ProjectManager.ActiveProject!.ActiveScene < ProjectManager.ActiveProject!.Scenes.Count
            ) ImGui.PushStyleColor(
                ImGuiCol.WindowBg, 
                new Vector4(
                    ProjectManager.ActiveProject!.Scenes[ProjectManager.ActiveProject!.ActiveScene].LetterboxColor,
                    1
                )
            );
        }

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

            surface.Active = !ImGui.IsWindowCollapsed();

            var contentRegion = ImGui.GetContentRegionMax();

            var size = GetBestFitSize(contentRegion, camera.Aspect);

            ImGui.SetCursorPos(contentRegion / 2 - size / 2);

            ImGui.Image(surface.GetImage(), size, new(0,1), new(1, 0));
        }

        public void OnPostRender(double deltaTime) 
        {
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
        }

        Vector2 GetBestFitSize(Vector2 container, float aspect)
        {
            var height = container.Y;
            var width = height * aspect;

            if (width > container.X)
            {
                width = container.X;
                height = width / aspect;
            }

            return new(width, height);
        }

        public void OnClose() => RenderSurface.Unregister(surface);
    }
}
