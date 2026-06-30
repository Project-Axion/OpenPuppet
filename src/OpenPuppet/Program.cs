using ImGuiNET;
using OpenPuppet.rendering;
using OpenPuppet.rendering.VertexTypes;
using OpenPuppet.vector;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using static Silk.NET.Core.Native.WinString;
using static System.Net.Mime.MediaTypeNames;
using Shader = OpenPuppet.rendering.Shader;

namespace OpenPuppet
{
    class Program
    {
        public static Camera PlaybackCamera = null;

        static WindowOptions windowOptions = WindowOptions.Default with
        {
            Title = "OpenPuppet",
            PreferredDepthBufferBits = 24,
            PreferredStencilBufferBits = 8,
            VSync = false,
            API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(3, 3))
        };

        static IWindow window = Window.Create(windowOptions);
        static IInputContext inputContext = null!;
        static GL gl = null!;

        static Shader shader = null;

        static ImGuiController cont = null;

        static Model<ColorVertex> testmdl = null;

        static void Main(string[] args)
        {
            window.Load += Load;
            window.Update += Update;
            window.Render += Render;
            window.FramebufferResize += Resize;
            window.Closing += Closing;

            window.Run();
        }

        static void Load() 
        {
            gl = window.CreateOpenGL();
            inputContext = window.CreateInput();
            cont = new ImGuiController(gl, window, inputContext);

            float NormX(float x) => x / 102f;
            float NormY(float y) => 1 - y / 102f;

            // I'm sorry but i'm not going to manually recreate a vector path....
            // this is done by ai and is only test code... everything else tho,
            // is too hard for the poor ai

            var path = new VectorPathComponent([
                // L 68,30 (from M 45,0)
                new LineCommand(
                        start: new Vector2(NormX(45), NormY(0)),
                        end: new Vector2(NormX(68), NormY(30))
                    ),
                    // C 50,50, 55,50, 70,75
                    new CubicBezierCommand(
                        origin: new Vector2(NormX(68), NormY(30)),
                        control1: new Vector2(NormX(50), NormY(50)),
                        control2: new Vector2(NormX(55), NormY(50)),
                        destination: new Vector2(NormX(70), NormY(75))
                    ),
                    // L 67,78
                    new LineCommand(
                        start: new Vector2(NormX(70), NormY(75)),
                        end: new Vector2(NormX(67), NormY(78))
                    ),
                    // C 50,70, 40,80, 53,97
                    new CubicBezierCommand(
                        origin: new Vector2(NormX(67), NormY(78)),
                        control1: new Vector2(NormX(50), NormY(70)),
                        control2: new Vector2(NormX(40), NormY(80)),
                        destination: new Vector2(NormX(53), NormY(97))
                    ),
                    // L 50,100
                    new LineCommand(
                        start: new Vector2(NormX(53), NormY(97)),
                        end: new Vector2(NormX(50), NormY(100))
                    ),
                    // C 30,75, 30,60, 57,68
                    new CubicBezierCommand(
                        origin: new Vector2(NormX(50), NormY(100)),
                        control1: new Vector2(NormX(30), NormY(75)),
                        control2: new Vector2(NormX(30), NormY(60)),
                        destination: new Vector2(NormX(57), NormY(68))
                    ),
                    // L 35,40
                    new LineCommand(
                        start: new Vector2(NormX(57), NormY(68)),
                        end: new Vector2(NormX(35), NormY(40))
                    ),
                    // Q 60,25, 38,0
                    new QuadraticBezierCommand(
                        origin: new Vector2(NormX(35), NormY(40)),
                        control: new Vector2(NormX(60), NormY(25)),
                        destination: new Vector2(NormX(38), NormY(0))
                    ),
                    // Z (Close back to M 45,0)
                    new LineCommand(
                        start: new Vector2(NormX(38), NormY(0)),
                        end: new Vector2(NormX(45), NormY(0))
                    )
            ]);

            var mesh = VectorMesher.GenerateMesh<ColorVertex>(path);
            testmdl = new(gl, mesh);

            shader = new(gl, "shaders/vector.vertex.glsl", "shaders/vector.fragment.glsl");

            gl.Viewport(window.FramebufferSize);

            PlaybackCamera = new((Vector2)window.FramebufferSize);
        }
        static void Update(double deltaSeconds) => cont.Update((float)deltaSeconds);
        static unsafe void Render(double deltaSeconds) 
        {
            gl.ClearColor(0f, 0f, 0f, 1f);
            gl.Clear(ClearBufferMask.ColorBufferBit);

            testmdl.Bind(gl);
            shader.Use(gl);

            shader.UniformMat4(gl, "proj", PlaybackCamera.Projection);
            shader.UniformMat4(gl, "view", PlaybackCamera.View);

            unsafe
            {
                gl.DrawElements(GLEnum.Triangles, testmdl.IndexCount, GLEnum.UnsignedInt, (void*)0);
            }


            //ImGui.DockSpaceOverViewport();

            //ImGui.ShowDemoWindow();

            cont.Render(window);
        }
        static void Resize(Vector2D<int> newSize) 
        {
            gl.Viewport(newSize);
            PlaybackCamera.Resolution = (Vector2)newSize;
        }
        static void Closing() 
        {
            shader.Dispose(gl);
            testmdl.Dispose(gl);
        }
    }
}