using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.vector
{
    public struct VectorMeshPrototype(List<Vector3> positions, List<List<int>> flatMap)
    {
        public List<Vector3> Positions { get; set; } = positions;
        public List<List<int>> FlatMap { get; set; } = flatMap;
    }
}
