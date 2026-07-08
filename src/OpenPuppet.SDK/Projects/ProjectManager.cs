using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Projects
{
    public static class ProjectManager
    {
        public static Dictionary<string, ProjectMetadata> RecentProjects { get; } = new();
        public static ProjectMetadata? ActiveProject { get; set; } = null;
    }
}