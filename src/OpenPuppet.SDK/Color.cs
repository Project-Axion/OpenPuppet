using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public struct Color3(float r, float g, float b)
    {
        public float X = r, Y = g, Z = b;

        public static implicit operator Vector3(Color3 v)
        => new(v.X, v.Y, v.Z);

        public static implicit operator Color3(Vector3 v)
        => new(v.X, v.Y, v.Z);
    }

    public struct Color4(float r, float g, float b, float a)
    {
        public float X = r, Y = g, Z = b, W = a;

        public static implicit operator Vector4(Color4 v)
        => new(v.X, v.Y, v.Z, v.W);

        public static implicit operator Color4(Vector4 v)
        => new(v.X, v.Y, v.Z, v.W);
    }
}
