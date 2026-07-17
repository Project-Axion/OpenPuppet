using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core.Mutators
{
    public class Vec3Mutator : IMutator<Vector3>
    {
        public Vector3 Mutate(Vector3 a, Vector3 b, double factor) => Vector3.Lerp(a, b, (float)factor);
    }
    public class Vec2Mutator : IMutator<Vector2>
    {
        public Vector2 Mutate(Vector2 a, Vector2 b, double factor) => Vector2.Lerp(a, b, (float)factor);
    }

    public class Color3Mutator : IMutator<Color3>
    {
        public Color3 Mutate(Color3 a, Color3 b, double factor) => Vector3.Lerp(a, b, (float)factor);
    }
    public class Color4Mutator : IMutator<Color4>
    {
        public Color4 Mutate(Color4 a, Color4 b, double factor) => Vector4.Lerp(a, b, (float)factor);
    }
}
