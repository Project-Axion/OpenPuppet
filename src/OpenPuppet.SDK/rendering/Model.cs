using OpenPuppet.rendering.VertexTypes;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.rendering
{
    public class Model<T> where T : IVertex<T>
    {
        public readonly VertexMesh<T> Mesh;
        public readonly uint IndexCount;

        uint vbo = 0;
        uint vao = 0;
        uint ebo = 0;

        public unsafe Model(GL gl,VertexMesh<T> mesh) 
        {
            Mesh = mesh;

            Span<T> vertexData = mesh.Verticies;

            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

            fixed (void* ptr = &MemoryMarshal.GetReference(vertexData))
                gl.BufferData(
                    BufferTargetARB.ArrayBuffer,
                    (nuint)(T.Size * vertexData.Length), 
                    ptr, BufferUsageARB.StaticDraw
                );

            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);

            nuint off = 0;

            for (int i = 0; i < T.PropLayers.Length; i++)
            {
                gl.VertexAttribPointer(
                    (uint)i, T.PropLayers[i].Count, 
                    T.PropLayers[i].Type, 
                    T.PropLayers[i].Normalize, 
                    T.Size, (void*)off
                );
                gl.EnableVertexAttribArray((uint)i);

                off += (nuint)T.PropLayers[i].SizeInBytes;
            }

            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            gl.BufferData<int>(BufferTargetARB.ElementArrayBuffer, mesh.Indices, BufferUsageARB.StaticDraw);

            IndexCount = (uint)mesh.Indices.Length;
        }

        public void Bind(GL gl) => gl.BindVertexArray(vao);

        public void Dispose(GL gl)
        {
            gl.DeleteVertexArray(vao);
            gl.DeleteBuffer(vbo);
        }
    }
}
