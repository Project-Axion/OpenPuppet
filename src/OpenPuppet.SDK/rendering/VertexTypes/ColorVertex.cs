using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.rendering.VertexTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe record struct ColorVertex(Vector3 Position, Vector4 color) : IVertex<ColorVertex>
    {
        public static uint Size { get; set; } = (uint)sizeof(Vector3) + (uint)sizeof(Vector4);
        public static VertexPropLayer[] PropLayers { get; set; } =
        [
            new(3, VertexAttribPointerType.Float, false),
            new(4, VertexAttribPointerType.Float, true)
        ];

        public static List<ColorVertex> FromVec3(List<Vector3> vecs)
        {
            List<ColorVertex> vtx = new();

            foreach (var item in vecs) vtx.Add(new(item, Vector4.One));

            return vtx;
        }
    }
}
