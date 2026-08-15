using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public interface ISceneGameObject
    {
        public static readonly ISceneGameObject Scene = new SceneGameObject();

        public Guid ID { get; }

        public string Name { get; set; }

        public Matrix4x4 Transform { get; set; }
        public bool Visible { get; set; }

        public void Draw(GL gl);
    }

    public class SceneGameObject : ISceneGameObject
    {
        public Guid ID => Guid.Empty;

        public string Name { get; set; } = "Scene";

        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
        public bool Visible { get; set; } = false;

        public void Draw(GL gl) { }
    }
}
