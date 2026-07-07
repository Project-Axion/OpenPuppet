using OpenPuppet.rendering;
using OpenPuppet.rendering.VertexTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Projects
{
    public static class MeshManager
    {
        public static Dictionary<string, object> RegisteredMeshes { get; } = new();

        public static void Register(string registry, object model)
        {
            RegisteredMeshes.Add(registry, model);
        }

        public static VertexMesh<T> Fetch<T>(string registry) where T : IVertex<T>
        {
            if (RegisteredMeshes.TryGetValue(registry, out var mesh))
            {
                if(mesh is VertexMesh<T> typedMesh)
                {
                    return typedMesh;
                } else
                {
                    throw new ArgumentException($"Mesh registered under the registry '{registry}' is not {typeof(T).Name}.");
                }
            }
            else throw new ArgumentException($"No mesh registered under the registry: '{registry}'.");
        }

        public static void Remove(string register)
        {
            if (RegisteredMeshes.ContainsKey(register))
                RegisteredMeshes.Remove(register);
        }
    }
}