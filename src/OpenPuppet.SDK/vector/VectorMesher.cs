using OpenPuppet.rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.vector
{
    public static class VectorMesher
    {
        public static VertexMesh<T> GenerateMesh<T>(IVectorASTComponent ASTComponent) where T : IVertex<T>
        {
            var prototype = ASTComponent.Flatten(200);

            List<int> idx = new();

            List<int> last = new List<int>();

            foreach (var item in prototype.FlatMap)
            {
                if (item.Count < 2)
                {
                    last = item;
                    continue;
                }

                // Upbridge
                if (item.Count > last.Count && last.Count > 0 && item.Count % 2 == 0 && last.Count % 2 == 0)
                {
                    var ypos = prototype.Positions[item[0]].Y;

                    for (int i = 0; i < last.Count; i += 2)
                    {
                        idx.Add(last[i]);
                        idx.Add(prototype.Positions.Count);
                        idx.Add(last[i + 1]);

                        idx.Add(prototype.Positions.Count);
                        idx.Add(prototype.Positions.Count + 1);
                        idx.Add(last[i + 1]);

                        prototype.Positions.AddRange([
                            new(prototype.Positions[last[i]].X, ypos,0),
                            new(prototype.Positions[last[i+1]].X, ypos,0),
                        ]);
                    }

                    last.Clear();
                }

                // Downbridge
                if (item.Count < last.Count && item.Count > 0 && item.Count % 2 == 0 && last.Count % 2 == 0)
                {
                    var ypos = prototype.Positions[last[0]].Y;

                    for (int i = 0; i < item.Count; i += 2)
                    {
                        idx.Add(prototype.Positions.Count);
                        idx.Add(prototype.Positions.Count + 1);
                        idx.Add(item[i]);

                        idx.Add(prototype.Positions.Count + 1);
                        idx.Add(item[i + 1]);
                        idx.Add(item[i]);

                        prototype.Positions.AddRange([
                            new(prototype.Positions[item[i]].X, ypos,0),
                            new(prototype.Positions[item[i+1]].X, ypos,0),
                        ]);
                    }

                    last.Clear();
                }

                // Quad-ize
                if (last.Count > 0 && item.Count == last.Count)
                {
                    int p = 0;

                    for (int i = 0; i < item.Count; i += 2)
                    {
                        idx.Add(last[i]);
                        idx.Add(last[i + 1]);
                        idx.Add(item[i]);

                        idx.Add(last[i + 1]);
                        idx.Add(item[i + 1]);
                        idx.Add(item[i]);

                        p++;
                    }
                }

                last = item;
            }

            return new(T.FromVec3(prototype.Positions), idx);
        }
    }
}
