using ImGuiNET;
using OpenPuppet.rendering;
using OpenPuppet.SDK;
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
        public uint IstanceIndex { get; set; }
        public string Title { get; set; } = "Editor";

        RenderSurface surface = null!;
        Camera camera = null!;

        Vector2 size = Vector2.Zero;

        public void OnLoad() 
        {
            camera = new Camera(Vector2.One);

            surface = new RenderSurface(camera);
            RenderSurface.Register(surface);
        }

        public void OnUpdate(double deltaTime) {}

        public void OnPreRender(double deltaTime) 
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        }

        public void OnRender(double deltaTime)
        {
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
        }

        public void OnPostRender(double deltaTime) 
        {
            ImGui.PopStyleVar();
        }

        public void OnClose() => RenderSurface.Unregister(surface);
    }
}
