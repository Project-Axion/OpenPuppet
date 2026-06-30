using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.rendering
{
    public struct VertexPropLayer(int count, VertexAttribPointerType type,bool normalized)
    {
        public int Count { get; set; } = count;
        public VertexAttribPointerType Type { get; set; } = type;
        public bool Normalize { get; set; } = normalized;
        public int SizeInBytes { get; private set; } = GatherSize(type) * count;


        static int GatherSize(VertexAttribPointerType type)
        {
            return type switch
            {
                VertexAttribPointerType.Byte => 1,
                VertexAttribPointerType.UnsignedByte => 1,
                VertexAttribPointerType.Short => 2,
                VertexAttribPointerType.UnsignedShort => 2,
                VertexAttribPointerType.Int => 4,
                VertexAttribPointerType.UnsignedInt => 4,
                VertexAttribPointerType.Float => 4,
                VertexAttribPointerType.Double => 8,
                VertexAttribPointerType.HalfFloat => 2,
                _ => 1
            };
        }
    }
}
