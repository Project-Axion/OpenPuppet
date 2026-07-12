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
}
