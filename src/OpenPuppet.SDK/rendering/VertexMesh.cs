using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.rendering
{
    public class VertexMesh<T>(List<T> verticies, List<int> indicies) where T : IVertex<T>
    {
        public T[] Verticies { get; } = verticies.ToArray();
        public int[] Indices { get; } = indicies.ToArray();
    }
}
