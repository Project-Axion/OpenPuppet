using ImGuiNET;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace silkgltriangle.Imgui
{
    public static class ImPlatform
    {
        public static Dictionary<uint, ImGuiWindow> windows = new();

        public static ImGuiController platformController;

        static WindowOptions windowOptions = WindowOptions.Default with
        {
            PreferredDepthBufferBits = 24,
            PreferredStencilBufferBits = 8,
            IsVisible = false,
            WindowBorder = WindowBorder.Hidden,
            API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(3, 3))
        };

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void Viewport_Del(ImGuiViewportPtr vp);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void ViewportString_Del(ImGuiViewportPtr vp, byte* str);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void ViewportFloat_Del(ImGuiViewportPtr vp, float alpha);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate byte ByteViewport_Del(ImGuiViewportPtr vp);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate float FloatViewport_Del(ImGuiViewportPtr vp);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void SetVecDel_Del(ImGuiViewportPtr vp, Vector2 pos);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate Vector2* GetVecDel_Del(Vector2* outPos, ImGuiViewportPtr vp);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void IntPtr_Del(ImGuiViewportPtr viewport, IntPtr renderArg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void General_Del();

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void ImeData_Del(IntPtr ctx,ImGuiViewportPtr viewport, ImGuiPlatformImeDataPtr data);


        public static Viewport_Del _createWindow;
        public static Viewport_Del _showWindow;
        public static Viewport_Del _destroyWindow;

        public static ByteViewport_Del _getWindowFocus;
        public static Viewport_Del _setWindowFocus;

        public static ByteViewport_Del _getWindowMinimized;

        public static ViewportString_Del _setWindowTitle;
        public static ViewportFloat_Del _setWindowAlpha;
        public static Viewport_Del _updateWindow;

        public static FloatViewport_Del _getWindowDpi;

        public static Viewport_Del _onViewportChange;

        //public static ImeData_Del _setImeData;

        public static SetVecDel_Del _setWindowPos;
        public static GetVecDel_Del _getWindowPos;

        public static SetVecDel_Del _setWindowSize;
        public static GetVecDel_Del _getWindowSize;

        public static IntPtr_Del _renderWindow;
        public static IntPtr_Del _swapBuffers;

        static unsafe ImPlatform()
        {
            _createWindow = CreateWindow;
            _showWindow = ShowWindow;
            _destroyWindow = DestroyWindow;

            _setWindowPos = SetWindowPos;
            _getWindowPos = GetWindowPos;

            _setWindowSize = SetWindowSize;
            _getWindowSize = GetWindowSize;

            _renderWindow = RenderWindow;
            _swapBuffers = SwapBuffers;

            _getWindowFocus = GetWindowFocus;
            _setWindowFocus = SetWindowFocus;
            _getWindowMinimized = GetWindowMinimized;
            _setWindowAlpha = SetWindowAlpha;
            _setWindowTitle = SetWindowTitle;
            _updateWindow = UpdateWindow;
            _getWindowDpi = GetWindowDpiScale;
            _onViewportChange = OnChangedViewport;
        }

        public static unsafe void CreateWindow(ImGuiViewportPtr viewport)
        {
            if (windows.ContainsKey(viewport.ID)) return;

            var window = new ImGuiWindow();

            WindowOptions sizedopts = windowOptions with
            {
                SharedContext = platformController._view.GLContext,
                Size = new Silk.NET.Maths.Vector2D<int>((int)viewport.Size.X, (int)viewport.Size.Y),
                Position = new Silk.NET.Maths.Vector2D<int>((int)viewport.Pos.X, (int)viewport.Pos.Y)
            };

            window.window = Window.Create(sizedopts);
            window.window.Initialize();

            window.window.FocusChanged += (a) => window.focus = a;

            window.viewport = viewport;
            window.gl = window.window.CreateOpenGL();
            window.inputContext = window.window.CreateInput();

            var kb = window.inputContext.Keyboards[0];
            kb.KeyChar += platformController.OnKeyChar;
            kb.KeyUp += ImGuiController.OnKeyUp;
            kb.KeyDown += ImGuiController.OnKeyDown;

            var native = window.window.Native;
            var kind = native.Kind;
            nint rawHandle = IntPtr.Zero;

            if (kind.HasFlag(NativeWindowFlags.Sdl)) rawHandle = native.Sdl ?? IntPtr.Zero;
            else if (kind.HasFlag(NativeWindowFlags.Glfw)) rawHandle = native.Glfw ?? IntPtr.Zero;
            else rawHandle = native.DXHandle ?? IntPtr.Zero;

            viewport.PlatformHandleRaw = rawHandle;
            viewport.PlatformHandle = viewport.PlatformHandleRaw;

            windows.Add(viewport.ID, window);
        }
        public static unsafe void ShowWindow(ImGuiViewportPtr viewport)
        {
            if (windows.TryGetValue(viewport.ID, out var window))
            {
                window.window.IsVisible = true;
            }
        }
        public static unsafe void DestroyWindow(ImGuiViewportPtr viewport)
        {
            if (windows.TryGetValue(viewport.ID, out var window))
            {
                window.inputContext.Dispose();
                window.gl.Dispose();
                window.window.Dispose();

                viewport.PlatformHandle = 0;

                windows.Remove(viewport.ID);
            }
        }

        public static unsafe Vector2* GetWindowPos(Vector2* outPos, ImGuiViewportPtr viewport)
        {
            if (windows.TryGetValue(viewport.ID, out var window))
            {
                var pos = window.window.Position;
                *outPos = new Vector2(pos.X, pos.Y);
            }
            else
            {
                *outPos = new Vector2(0, 0);
                Console.WriteLine("warning, id not found for position");
            }

            return outPos;
        }
        public static unsafe void SetWindowPos(ImGuiViewportPtr viewport, Vector2 pos)
        {
            if (windows.TryGetValue(viewport.ID, out var window))
                window.window.Position = new Silk.NET.Maths.Vector2D<int>((int)pos.X, (int)pos.Y);
        }

        public static unsafe Vector2* GetWindowSize(Vector2* outSize, ImGuiViewportPtr viewport)
        {
            if (windows.TryGetValue(viewport.ID, out var window))
            {
                var size = window.window.Size;
                *outSize = new Vector2(size.X, size.Y);
            }
            else
            {
                *outSize = new Vector2(1, 1);
                Console.WriteLine("warning, id not found for size");
            }

            return outSize;
        }
        public static unsafe void SetWindowSize(ImGuiViewportPtr viewport, Vector2 size)
        {
            if (windows.TryGetValue(viewport.ID, out var window))
                window.window.Size = new Silk.NET.Maths.Vector2D<int>((int)size.X, (int)size.Y);
        }

        public static unsafe void RenderWindow(ImGuiViewportPtr viewport, IntPtr renderArg)
        {
            if (windows.TryGetValue(viewport.ID, out var window) && window.window.IsInitialized)
            {
                window.window.GLContext.MakeCurrent();

                window.gl.Viewport(window.window.FramebufferSize);

                window.gl.ClearColor(0, 0, 0, 0);
                window.gl.Clear(ClearBufferMask.ColorBufferBit);

                platformController.DrawImData(window.gl, viewport.DrawData);
            }
        }
        public static unsafe void SwapBuffers(ImGuiViewportPtr viewport, IntPtr renderArg)
        {
            if (windows.TryGetValue(viewport.ID, out var window) && window.window.IsInitialized)
            {
                window.window.GLContext.SwapBuffers();
                window.window.GLContext.Clear();
            }
        }

        public static unsafe byte GetWindowFocus(ImGuiViewportPtr viewport)
        {
            if (windows.TryGetValue(viewport.ID, out var window))
                return (byte)(window.focus ? 1 : 0);

            return 0;
        }

        public static unsafe void SetWindowFocus(ImGuiViewportPtr viewport)
        {
            if (windows.TryGetValue(viewport.ID, out var window))
                window.window.Focus();
        }

        public static unsafe byte GetWindowMinimized(ImGuiViewportPtr viewport)
        {
            if (windows.TryGetValue(viewport.ID, out var window))
                return (byte)(window.window.WindowState == WindowState.Minimized ? 1 : 0);

            return 0;
        }

        public static unsafe void SetWindowTitle(ImGuiViewportPtr viewport, byte* str)
        {
            if (windows.TryGetValue(viewport.ID, out var window))
                window.window.Title = Marshal.PtrToStringAnsi((nint)str);
        }

        public static unsafe void SetWindowAlpha(ImGuiViewportPtr viewport, float alpha) { }

        public static unsafe void UpdateWindow(ImGuiViewportPtr viewport)
        {
            if (windows.TryGetValue(viewport.ID, out var window))
                window.window.DoEvents();
        }

        public static unsafe float GetWindowDpiScale(ImGuiViewportPtr viewport) { return 1; }

        public static unsafe void OnChangedViewport(ImGuiViewportPtr viewport)
        {
            if (!windows.TryGetValue(viewport.ID, out var window)) return;

            var newPos = viewport.Pos;
            var newSize = viewport.Size;

            if (window.lastPos != newPos || window.lastSize != newSize)
            {
                window.lastPos = newPos;
                window.lastSize = newSize;
            }
        }
    }

    public unsafe class ImGuiWindow
    {
        public IWindow window;
        public ImGuiViewportPtr viewport;
        public GL gl;
        public IInputContext inputContext;

        public bool focus = false;

        public Vector2 lastPos;
        public Vector2 lastSize;
    }
}
