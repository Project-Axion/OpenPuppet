using OpenPuppet.rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Projects
{
    public class ProjectMetadata
    {
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public string Directory { get; set; } = string.Empty;

        public List<AssetMetadata> Assets { get; set; } = new();
        public List<SceneMetadata> Scenes { get; set; } = [new()];

        [JsonIgnore]
        public int ActiveScene { get; set; } = 0;

    }

    public class AssetMetadata
    {
        public string File { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class SceneMetadata
    {
        public Camera SceneCamera { get; set; } = new Camera(new(1920,1080));
    }
}
