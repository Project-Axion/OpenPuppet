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
    public class Editor : IUIWindow
    {
        public uint InstanceIndex { get; set; }
        public string Title { get; set; } = "Editor";
        public ImGuiWindowFlags? Flags { get; set; } = null;
        public Vector2? Size { get; set; } = null;

        RenderSurface surface = null!;
        Camera camera = null!;

        Vector3 pan = Vector3.Zero;
        Vector3 worldPan = Vector3.Zero;
        float zoom = 0.8f;

        Vector2 size = Vector2.Zero;

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

                camera.Pan = scamera.Pan + worldPan;

                camera.Zoom = scamera.Zoom * zoom;
            }
        }

        public void OnPreRender(double deltaTime) 
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
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

            var scenecamera = ProjectManager.ActiveProject!.Scenes[ProjectManager.ActiveProject!.ActiveScene].SceneCamera;
            var theoreticalSize = new Vector2(camera.Resolution.Y * scenecamera.Aspect, camera.Resolution.Y) * zoom;

            if (ImGui.IsWindowFocused())
            {
                zoom += ImGui.GetIO().MouseWheel * (0.1f * Math.Min((float)Math.Pow(camera.Zoom, 2),1));

                if (ImGui.IsMouseDragging(ImGuiMouseButton.Middle))
                {
                    var mouseDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Middle, 0.0f);
                    pan += new Vector3(mouseDelta.X, mouseDelta.Y, 0);

                    ImGui.ResetMouseDragDelta(ImGuiMouseButton.Middle);
                }

                var units = Camera.CAMERA_UNITS_BASIS / theoreticalSize.Y;

                worldPan = pan * new Vector3(units,-units,1);
            }

            var contentRegion = ImGui.GetContentRegionAvail();

            if (contentRegion.X > 0 && contentRegion.Y > 0)
            {
                const float resizeEpsilon = 1.0f;
                if (Vector2.Distance(contentRegion, size) > resizeEpsilon)
                {
                    size = contentRegion;
                    surface.Resize(contentRegion);
                }
            }

            ImGui.Image(surface.GetImage(), camera.Resolution, new(0,1), new(1, 0));

            var drawlist = ImGui.GetWindowDrawList();

            var min = contentRegion / 2 - theoreticalSize / 2 + new Vector2(pan.X, pan.Y) + ImGui.GetWindowPos() + ImGui.GetCursorStartPos(); ;

            drawlist.AddRect(min, min + theoreticalSize, ImGui.GetColorU32(ImGuiCol.Border, 0.7f), 1.0f);
        }

        public void OnPostRender(double deltaTime) 
        {
            ImGui.PopStyleVar();
        }

        public void OnClose() => RenderSurface.Unregister(surface);
    }
}
