using Newtonsoft.Json;
using OpenPuppet.Core.Dialogs;
using OpenPuppet.SDK;
using OpenPuppet.SDK.Events;
using OpenPuppet.SDK.Projects;
using OpenPuppet.SDK.TimelineTracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core
{
    public static class Projects
    {
        public static List<string> RecentProjects = new();

        public static void OpenProject(ProjectMetadata meta)
        {
            foreach (var item in meta.Scenes)
            {
                foreach (var item1 in item.AnimationScene)
                    foreach (var item2 in item1.Value) item2.Scene = item;
            }

            ProjectManager.ActiveProject = meta;

            var projfile = Path.Combine(meta.Directory, meta.Name + ".opp");

            RecentProjects.Remove(projfile);
            RecentProjects.Insert(0, projfile);

            RecentProjects = RecentProjects.Take(25).ToList();

            File.WriteAllLines(Path.Combine(SDK.SDK.DataPath, "projcache"), RecentProjects);

            IEvent<string>.Invoke("openpuppet.window.modify.title", null, $"OpenPuppet - {meta.Name}");

            ContextMenu.SetEnabledAll(true);
        }

        public static void Create(string name, string path)
        {
            string dir = Path.Combine(path, name);
            int i = 0;
            while(Directory.Exists(dir))
            {
                i++;
                dir = Path.Combine(dir, name + (i > 0 ? $"({i})" : ""));
            }
            Directory.CreateDirectory(dir);
            var proj = new ProjectMetadata()
            {
                Name = name + $"({i})",
                Directory = dir,
            };

            proj.Scenes.Add(new());

            proj.Scenes[0].SceneObjects.Add(ISceneGameObject.Scene);
            proj.Scenes[0].AnimationScene.Add(ISceneGameObject.Scene.ID, new());

            proj.Scenes[0].AnimationScene[ISceneGameObject.Scene.ID].Add(
                new PropertyTimeline<Vector3>(
                    ISceneGameObject.Scene.ID,
                    proj.Scenes[0],
                    "Letterbox color",
                    () => proj.Scenes[0].LetterboxColor
                )
                {
                    Keyframes = new()
                    {
                            {new(0,0,0),Vector3.Zero},
                            {new(0,0,1),Vector3.One},
                    }
                }
            );

            OpenProject(proj);

            File.WriteAllText(
                Path.Combine(dir, name + ".opp"),
                JsonConvert.SerializeObject(ProjectManager.ActiveProject, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented
                })
            );

            Directory.CreateDirectory(Path.Combine(dir, "Meshes"));
        }
    }
}
