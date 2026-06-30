using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.rendering
{
    public interface IVertex<TSelf> where TSelf : IVertex<TSelf>
    {
        public static abstract uint Size { get; set; }
        public static abstract VertexPropLayer[] PropLayers { get; set; }

        public static abstract List<TSelf> FromVec3(List<Vector3> vecs);
    }
}
