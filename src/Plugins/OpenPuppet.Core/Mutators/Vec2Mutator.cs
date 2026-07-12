using OpenPuppet.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core.Mutators
{
    public class Vec2Mutator : IMutator<Vector2>
    {
        public Vector2 Mutate(Vector2 a, Vector2 b, double factor) => Vector2.Lerp(a, b, (float)factor);
    }
}
