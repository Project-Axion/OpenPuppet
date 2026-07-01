using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.rendering
{
    public class Shader
    {
        uint shader;

        public Shader(GL gl,string vtx, string frag)
        {
            uint vs = gl.CreateShader(ShaderType.VertexShader);
            gl.ShaderSource(vs, File.ReadAllText(vtx));
            gl.CompileShader(vs);

            uint fs = gl.CreateShader(ShaderType.FragmentShader);
            gl.ShaderSource(fs, File.ReadAllText(frag));
            gl.CompileShader(fs);

            shader = gl.CreateProgram();
            gl.AttachShader(shader, vs);
            gl.AttachShader(shader, fs);
            gl.LinkProgram(shader);

            gl.DetachShader(shader, vs);
            gl.DetachShader(shader, fs);
            gl.DeleteShader(vs);
            gl.DeleteShader(fs);
        }

        public void Use(GL gl) => gl.UseProgram(shader);

        public void UniformMat4(GL gl,string uniform,Matrix4x4 mat)
        {
            Use(gl);

            gl.UniformMatrix4(
                gl.GetUniformLocation(shader, uniform),
                1, false,
                in mat.M11
            );
        }

        public void Dispose(GL gl) => gl.DeleteProgram(shader);
    }
}
