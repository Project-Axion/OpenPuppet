using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public interface ISceneGameObject
    {
        public static readonly ISceneGameObject Scene = new SceneGameObject();

        Guid ID { get; }

        public string Name { get; set; }

        public void Draw(GL gl);
    }

    public class SceneGameObject : ISceneGameObject
    {
        public Guid ID => Guid.Empty;

        public string Name { get; set; } = "Scene";

        public void Draw(GL gl) {}
    }
}
