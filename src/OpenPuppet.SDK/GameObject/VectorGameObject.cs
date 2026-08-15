using Newtonsoft.Json;
using OpenPuppet.rendering;
using OpenPuppet.rendering.VertexTypes;
using OpenPuppet.SDK.Projects;
using OpenPuppet.vector;
using Silk.NET.Core.Native;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.GameObject
{
    public class VectorGameObject<vtx> : ISceneGameObject where vtx : IVertex<vtx>
    {
        public Guid ID => Guid.NewGuid();

        public string Name { get; set; } = "Vector Object";

        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
        public bool Visible { get; set; } = true;

        public string VectorAssetPath { get; set; } = string.Empty;
        [JsonIgnore] public IVectorASTComponent VectorASTCache { get; set; }
        [JsonIgnore] VertexMesh<vtx> MeshCache { get; set; }
        [JsonIgnore] Model<vtx> Model { get; set; } = null!;

        public VectorGameObject(string vectorAssetPath)
        {
            this.VectorAssetPath = vectorAssetPath;

            if (ProjectManager.ActiveProject != null)
                vectorAssetPath = Path.Combine(ProjectManager.ActiveProject.Directory, vectorAssetPath);

            VectorASTCache = IVectorASTComponent.LoadFromDisk(vectorAssetPath);

            MeshCache = VectorMesher.GenerateMesh<vtx>(VectorASTCache);
        }

        public unsafe void Draw(GL gl)
        {
            if (Model is null) Model = new(gl, MeshCache);

            Model.Bind(gl);

            gl.DrawElements(GLEnum.Triangles, Model.IndexCount, GLEnum.UnsignedInt, (void*)0);
        }
    }
}
