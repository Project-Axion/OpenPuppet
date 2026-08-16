using Newtonsoft.Json;
using OpenPuppet.Core.Dialogs;
using OpenPuppet.rendering.VertexTypes;
using OpenPuppet.SDK;
using OpenPuppet.SDK.Events;
using OpenPuppet.SDK.GameObject;
using OpenPuppet.SDK.Projects;
using OpenPuppet.SDK.TimelineTracks;
using OpenPuppet.SDK.vector.ColorSamplers;
using OpenPuppet.vector;
using Silk.NET.OpenGL;
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

        public static void SaveProject(ProjectMetadata meta) => SaveProject(meta, meta.Directory);
        public static void SaveProject(ProjectMetadata meta,string dir)
        {
            meta.Directory = dir;

            File.WriteAllText(
                Path.Combine(dir, meta.Name + ".opp"),
                JsonConvert.SerializeObject(meta, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    SerializationBinder = SDK.SDK.JsonTypeBinder
                })
            );
        }

        public static void OpenProject(string path)
        {
            var dRestore = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(path)!);

            var json = JsonConvert.DeserializeObject<ProjectMetadata>(
                File.ReadAllText(path),
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    SerializationBinder = SDK.SDK.JsonTypeBinder
                }
            )!;
            json.Directory = Path.GetDirectoryName(path)!;

            Directory.SetCurrentDirectory(dRestore);

            OpenProject(json);
        }

        public static void Create(string name, string path)
        {
            string dir = Path.Combine(path, name);

            int i = 0;
            for (; Directory.Exists(dir); i++) dir = Path.Combine(path, name + $"({i})");

            Directory.CreateDirectory(dir);
            Directory.CreateDirectory(Path.Combine(dir, "Vectors"));

            var proj = new ProjectMetadata()
            {
                Name = name + (i > 0 ? $"({i})" : ""),
                Directory = dir,
            };

            float NormX(float x) => x / 102f;
            float NormY(float y) => y / 102f;

            // I'm sorry but i'm not going to manually recreate a vector path....
            // this is done by ai and is only test code... everything else tho,
            // is too hard for the poor ai

            var vecpath = new VectorPathComponent([
                // L 68,30 (from M 45,0)
                new LineCommand(
                        start: new Vector2(NormX(45), NormY(0)),
                        end: new Vector2(NormX(68), NormY(30))
                    ),
                    // C 50,50, 55,50, 70,75
                    new CubicBezierCommand(
                        origin: new Vector2(NormX(68), NormY(30)),
                        control1: new Vector2(NormX(50), NormY(50)),
                        control2: new Vector2(NormX(55), NormY(50)),
                        destination: new Vector2(NormX(70), NormY(75))
                    ),
                    // L 67,78
                    new LineCommand(
                        start: new Vector2(NormX(70), NormY(75)),
                        end: new Vector2(NormX(67), NormY(78))
                    ),
                    // C 50,70, 40,80, 53,97
                    new CubicBezierCommand(
                        origin: new Vector2(NormX(67), NormY(78)),
                        control1: new Vector2(NormX(50), NormY(70)),
                        control2: new Vector2(NormX(40), NormY(80)),
                        destination: new Vector2(NormX(53), NormY(97))
                    ),
                    // L 50,100
                    new LineCommand(
                        start: new Vector2(NormX(53), NormY(97)),
                        end: new Vector2(NormX(50), NormY(100))
                    ),
                    // C 30,75, 30,60, 57,68
                    new CubicBezierCommand(
                        origin: new Vector2(NormX(50), NormY(100)),
                        control1: new Vector2(NormX(30), NormY(75)),
                        control2: new Vector2(NormX(30), NormY(60)),
                        destination: new Vector2(NormX(57), NormY(68))
                    ),
                    // L 35,40
                    new LineCommand(
                        start: new Vector2(NormX(57), NormY(68)),
                        end: new Vector2(NormX(35), NormY(40))
                    ),
                    // Q 60,25, 38,0
                    new QuadraticBezierCommand(
                        origin: new Vector2(NormX(35), NormY(40)),
                        control: new Vector2(NormX(60), NormY(25)),
                        destination: new Vector2(NormX(38), NormY(0))
                    ),
                    // Z (Close back to M 45,0)
                    new LineCommand(
                        start: new Vector2(NormX(38), NormY(0)),
                        end: new Vector2(NormX(45), NormY(0))
                    )
            ]);

            UnifiedVector vector = new([
                new(
                    new EllipseComponent(Vector2.One / 2f,Vector2.One / 2f),
                    new VectorLinearGradientColorSampler(new(1,0,0,1),0,Vector4.One,1,0)
                ),
                new(
                    vecpath,
                    new VectorLinearGradientColorSampler(new(1,0,1,1),0,Vector4.One,1,0)
                ),
            ]);

            IVectorASTComponent.SaveToDisk(vector, Path.Combine(dir, "Vectors", "vecpath.ovec"));

            proj.Scenes.Add(new());

            proj.Scenes[0].SceneObjects.Add(ISceneGameObject.Scene);
            proj.Scenes[0].AnimationScene.Add(ISceneGameObject.Scene.ID, new());

            proj.Scenes[0].AnimationScene[ISceneGameObject.Scene.ID].Add(
                new PropertyTimeline<Color3>(
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
                    },
                    KeyframeEasings = new()
                    {
                        {new(0,0,0),new("openpuppet.core.linear")},
                        {new(0,0,1),new("openpuppet.core.linear")},
                    }
                }
            );

            OpenProject(proj);
            proj.Scenes[0].SceneObjects.Add(new VectorGameObject<ColorVertex>(Path.Combine("Vectors", "vecpath.ovec")));

            File.WriteAllText(
                Path.Combine(dir, proj.Name + ".opp"),
                JsonConvert.SerializeObject(ProjectManager.ActiveProject, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                    SerializationBinder = SDK.SDK.JsonTypeBinder
                })
            );
        }
    }
}
