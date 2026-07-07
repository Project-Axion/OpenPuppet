using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Projects
{
    public static class SceneManager
    {
        public static Dictionary<string, object> RegisteredScenes { get; } = new();
        public static object? ActiveScene { get; internal set; }
    }
}
