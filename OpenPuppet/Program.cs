using System;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

WindowOptions windowOptions = WindowOptions.Default with
{
    Title = "OpenPuppet",
    PreferredDepthBufferBits = 24,
    PreferredStencilBufferBits = 8,
    API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(3, 3))
};

using IWindow window = Window.Create(windowOptions);
IInputContext inputContext = null!;
GL gl = null!;

uint vbo = 0;
uint vao = 0;
uint shader = 0;

const string VertexShaderCode = @"
#version 330 core
layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec4 vColor;

out vec4 fColor;

void main() {
    gl_Position = vec4(vPosition, 1.0);
    fColor = vColor;
}";

const string FragmentShaderCode = @"
#version 330 core
in vec4 fColor;

out vec4 FragColor;

void main() {
    FragColor = fColor;
}";

ImGuiController cont = null;

window.Load += () =>
{
    gl = window.CreateOpenGL();
    inputContext = window.CreateInput();
    cont = new ImGuiController(gl, window, inputContext);

    Span<Vertex> vertexData =
    [
        new Vertex(new Vector3(-0.5f, -0.5f, 0), 255, 0, 0, 255),
        new Vertex(new Vector3(0, 0.5f, 0), 0, 255, 0, 255),
        new Vertex(new Vector3(0.5f, -0.5f, 0), 0, 0, 255, 255)
    ];

    vbo = gl.GenBuffer();
    gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
    gl.BufferData<Vertex>(BufferTargetARB.ArrayBuffer, vertexData, BufferUsageARB.StaticDraw);

    vao = gl.GenVertexArray();
    gl.BindVertexArray(vao);
    unsafe
    {
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Size, (void*)0);
        gl.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, Vertex.Size, (void*)12);
    }
    gl.EnableVertexAttribArray(0);
    gl.EnableVertexAttribArray(1);

    uint vs = gl.CreateShader(ShaderType.VertexShader);
    gl.ShaderSource(vs, VertexShaderCode);
    gl.CompileShader(vs);

    uint fs = gl.CreateShader(ShaderType.FragmentShader);
    gl.ShaderSource(fs, FragmentShaderCode);
    gl.CompileShader(fs);

    shader = gl.CreateProgram();
    gl.AttachShader(shader, vs);
    gl.AttachShader(shader, fs);
    gl.LinkProgram(shader);

    gl.DetachShader(shader, vs);
    gl.DetachShader(shader, fs);
    gl.DeleteShader(vs);
    gl.DeleteShader(fs);

    gl.Viewport(window.FramebufferSize);
};

window.Update += deltaSeconds =>
{
    cont.Update((float)deltaSeconds);
};

window.Render += deltaSeconds =>
{
    gl.ClearColor(0f, 0f, 0f, 1f);
    gl.Clear(ClearBufferMask.ColorBufferBit);

    gl.BindVertexArray(vao);
    gl.UseProgram(shader);
    gl.DrawArrays(PrimitiveType.Triangles, 0, 3);

    ImGui.DockSpaceOverViewport();

    ImGui.ShowDemoWindow();

    cont.Render(window);
};

window.FramebufferResize += newSize =>
{
    gl.Viewport(newSize);
};

window.Closing += () =>
{
    gl.DeleteProgram(shader);
    gl.DeleteVertexArray(vao);
    gl.DeleteBuffer(vbo);
};

window.Run();

[StructLayout(LayoutKind.Sequential)]
internal record struct Vertex(Vector3 Position, byte R, byte G, byte B, byte A)
{
    public static uint Size = 3 * 4 + 4;
}