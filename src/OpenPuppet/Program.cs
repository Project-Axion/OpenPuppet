using ImGuiNET;
using OpenPuppet.Plugins;
using OpenPuppet.rendering;
using OpenPuppet.rendering.VertexTypes;
using OpenPuppet.SDK;
using OpenPuppet.SDK.Events;
using OpenPuppet.vector;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
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

        public static Logger.PluginLogger logger = Logger.LogManager.RequestPluginLogger("openpuppet");

        // Just so we can see how long it takes for it to start up
        internal static Stopwatch startupSW;

        static void Main(string[] args)
        {
            if(args.Length > 0)
            {
                if (args[0] == "clean")
                {
                    logger.Dispose();
                    if (Directory.Exists(Logger.LogPath))
                        Directory.Delete(Logger.LogPath, true);
                    Environment.Exit(0);
                }
            }

            startupSW = new();
            startupSW.Start();

            window.Load += Load;
            window.Update += Update;
            window.Render += Render;
            window.FramebufferResize += Resize;
            window.Closing += Closing;

            IEvent<string>.Subscribe(
                "openpuppet.window.modify.title",
                (object sender, string e) =>
                {
                    window.Title = e;
                }
            );

            IEvent<bool>.Subscribe(
                "openpuppet.restart",
                (object sender, bool soft) =>
                {
                    if(soft) SoftRestart();
                    else
                    {
                        logger.WriteLine($"Launching {Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe")}");
                        window.Close();
                        Process.Start(Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe"));
                        Environment.Exit(0);
                    }
                }
            );

            IEvent<EventArgs>.Subscribe("openpuppet.quit", (_, _) => window.Close());

            SDK.Dialogs.Dialogs.RegisterBuiltInDialogs();

            try
            {
                window.Run();
            } catch (Exception ex)
            {
                logger.WriteLine(
                    Logger.ILogger.Level.Error,
                    $"Exception occurred while running main window: {ex.Message} ({ex.GetType()})"
                );

                if(ex.GetType() == typeof(Silk.NET.GLFW.GlfwException))
                {
                    // Temporary message
                    logger.WriteLine(
                        Logger.ILogger.Level.Warn,
                        "The above exception may occur if you are using an extremely low-end " +
                        "GPU, or integrated graphics. If you have multiple GPUs, please modify " +
                        "the operating system settings to ensure that OpenPuppet uses the " +
                        "correct GPU."
                    );
                }
            }
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

            RenderSurface.Init(gl);

            LoadPlugins();

            startupSW.Stop();
            logger.WriteLine(Logger.ILogger.Level.OK, $"Started up in {startupSW.Elapsed}");
        }
        static void Update(double deltaSeconds)
        {
            cont.Update((float)deltaSeconds);

            foreach (var item in IUIWindow.ActiveWindows)
                item.OnUpdate(deltaSeconds);
        }
        static unsafe void Render(double deltaSeconds) 
        {
            gl.ClearColor(0f, 0f, 0f, 1f);

            testmdl.Bind(gl);
            shader.Use(gl);

            // This list may need copying (.ToList())
            foreach (var item in RenderSurface.Surfaces)
            {
                item.Bind();
                gl.Viewport(0,0,(uint)item.Camera.Resolution.X, (uint)item.Camera.Resolution.Y);

                gl.Clear(ClearBufferMask.ColorBufferBit);

                shader.UniformMat4(gl, "proj", item.Camera.Projection);
                shader.UniformMat4(gl, "view", item.Camera.View);

                gl.DrawElements(GLEnum.Triangles, testmdl.IndexCount, GLEnum.UnsignedInt, (void*)0);
            }

            gl.BindFramebuffer(FramebufferTarget.Framebuffer,0);
            gl.Viewport(window.FramebufferSize);

            ImGui.DockSpaceOverViewport();

            if(ImGui.BeginMainMenuBar())
            {
                void RenderMenuNode(IContextMenuNode node)
                {
                    if (!node.Enabled) ImGui.BeginDisabled();
                    if (node is ContextMenuList list)
                    {
                        if (ImGui.BeginMenu(list.DisplayName))
                        {
                            foreach (var item in list.Nodes) RenderMenuNode(item);
                            ImGui.EndMenu();
                        }
                    }
                    else if (node is ContextMenuItem item)
                        if (ImGui.MenuItem(item.DisplayName) && node.Enabled) item.OnClick();
                    if (!node.Enabled) ImGui.EndDisabled();
                }

                foreach (var item in ContextMenu.Root.Nodes) RenderMenuNode(item);

                ImGui.EndMainMenuBar();
            }

            foreach (var item in IUIWindow.ActiveWindows.ToList())
            {
                bool open = true;

                item.OnPreRender(deltaSeconds);

                if (item.Size != null)
                    ImGui.SetNextWindowSize(item.Size ?? Vector2.PositiveInfinity);

                ImGui.Begin(
                    item.Title + "##" + item.InstanceIndex,
                    ref open,
                    item.Flags ?? ImGuiWindowFlags.None
                );

                item.OnRender(deltaSeconds);

                ImGui.End();

                item.OnPostRender(deltaSeconds);

                if (!open)
                {
                    IUIWindow.Close(item);
                }
            }

            if(IUIDialog.ActiveDialog != null)
            {
                var dialog = IUIDialog.ActiveDialog;

                ImGui.OpenPopup(
                    dialog.Title + "##openpuppet.dialog",
                    ImGuiPopupFlags.None
                );

                dialog.OnPreRender();

                bool open = true;

                ImGuiViewportPtr viewport = ImGui.GetMainViewport();

                ImGui.SetNextWindowPos(
                    viewport.GetCenter(),
                    ImGuiCond.Always,
                    new Vector2(0.5f, 0.5f)
                );

                if (dialog.Size != null)
                    ImGui.SetNextWindowSize(dialog.Size ?? Vector2.PositiveInfinity);

                if (ImGui.BeginPopupModal(
                    dialog.Title + "##openpuppet.dialog",
                    ref open,
                    dialog.Flags ?? ImGuiWindowFlags.AlwaysAutoResize
                ))
                {
                    ImGui.SetWindowFocus();
                    dialog.OnRender();
                    ImGui.EndPopup();
                }

                if(!open) IUIDialog.Close();
            }

            //ImGui.ShowDemoWindow();

            cont.Render(window);
        }
        static void Resize(Vector2D<int> newSize) 
        {
            PlaybackCamera.Resolution = (Vector2)newSize;
        }
        static void Closing() 
        {
            shader.Dispose(gl);
            testmdl.Dispose(gl);

            //for (int i = 0; i < PluginManager.LoadedPlugins.Count; i++)
            //    PluginManager.LoadedPlugins[i].OnShutdown();

            PluginManager.SavePluginList();
            foreach (var plugin in IPlugin.RegisteredPlugins)
            {
                try
                {
                    plugin.Value.Assembly?.OnShutdown();
                } catch (Exception ex)
                {
                    logger.WriteLine(
                        Logger.ILogger.Level.Warn,
                        $"\"{plugin.Key}\" failed to shut down: {ex.Message}"
                    );
                }
            }

            SDK.SDK.DestroyLogger();
            logger.Dispose();

            // Some plugins may still be loaded if there were reference leaks,
            // so just forcefully exit
            Environment.Exit(0);
        }

        static void LoadPlugins()
        {
            PluginManager.LoadPluginList();
            List<string> errors = new();
            /*foreach (var item in Directory.GetDirectories(PluginsPath.PluginPath!))
            {
                try
                {
                    IPlugin.LoadPluginDirectory(item);
                } catch (Exception ex)
                {
                    // In the future, this should be a popup with all the errors
                    errors.Add(ex.Message);
                    logger.WriteLine(ex.Message);
                }
            }*/

            foreach(var item in PluginManager.Plugins)
            {
                try
                {
                    IPlugin.LoadPluginDirectory(item.Value.Path, item.Value.Enabled);
                } catch (Exception ex)
                {
                    errors.Add(ex.Message);
                    logger.WriteLine(ex.Message);
                }
            }

            logger.WriteLine(Logger.ILogger.Level.Warn, $"{errors.Count} plugins failed to load");

            PluginEvents.InvokeFinishedLoading(null);
        }

        static void SoftRestart()
        {
            logger.WriteLine("Starting soft restart");

            ContextMenu.Root.Nodes.Clear();
            IUIWindow.CloseAll();
            IUIDialog.Close();
            IUIWindow.RegisteredWindows.Clear();
            IUIDialog.RegisteredWindows.Clear();

            foreach (var plugin in IPlugin.RegisteredPlugins.ToList())
            {
                try
                {
                    //plugin.Value.Assembly?.OnShutdown();
                    IPlugin.UnloadPlugin(plugin.Key, false); // Temporarily false
                }
                catch (Exception ex)
                {
                    logger.WriteLine(
                        Logger.ILogger.Level.Warn,
                        $"\"{plugin.Key}\" failed to shut down: {ex.Message}"
                    );
                }
            }

            IPlugin.RegisteredPlugins.Clear();

            SDK.Dialogs.Dialogs.RegisterBuiltInDialogs();

            LoadPlugins();

            logger.WriteLine("Finished soft restart");
        }
    }
}