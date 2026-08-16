using OpenPuppet.rendering;
using OpenPuppet.SDK.vector;
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
        public static VertexMesh<T> GenerateUnifiedMesh<T>(
            UnifiedVector vector, uint density = 200
        ) where T : IVertex<T>
        {
            List<VertexMesh<T>> meshes = new();

            for (int i = 0; i < vector.Components.Count; i++)
                meshes.Add(GenerateMesh<T>(vector.Components[i].AST, vector.Components[i].ColorSampler, density));

            return VertexMesh<T>.Unify(meshes);
        }

        public static VertexMesh<T> GenerateMesh<T>(
            IVectorASTComponent ASTComponent, 
            IVectorColorSampler colorSampler,
            uint density = 200
        ) where T : IVertex<T>
        {
            var prototype = ASTComponent.Flatten(density);

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

            for (int i = 0; i < prototype.Positions.Count; i++)
            {
                var p = prototype.Positions[i];
                prototype.Positions[i] = new Vector3(
                    Math.Clamp(p.X, 0f, 1f),
                    Math.Clamp(p.Y, 0f, 1f),
                    p.Z
                );
            }

            return new(
                T.FromGeneralData(
                    prototype.Positions.Select(
                        x => new GeneralVertexData(
                            x, Vector3.Zero, Vector2.Zero, 
                            colorSampler.SampleColor(x)
                        )
                    ).ToList()
                ), idx
            );
        }
    }
}
