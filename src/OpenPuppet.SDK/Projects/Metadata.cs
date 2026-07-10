using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Projects
{
    public class ProjectMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string Directory { get; set; } = string.Empty;

        public List<AssetMetadata> Assets { get; set; } = new();
        public List<SceneMetadata> Scenes { get; set; } = new();

    }

    public class AssetMetadata
    {
        public string File { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class SceneMetadata
    {

    }
}
