using ImGuiNET;
using OpenPuppet.SDK;
using OpenPuppet.SDK.Projects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.Core
{
    public class Properties : IUIWindow
    {
        public uint InstanceIndex { get; set; }
        public string Title { get; set; } = "Properties";
        public ImGuiWindowFlags? Flags { get; set; } = null;
        public Vector2? Size { get; set; } = null;

        readonly ConcurrentDictionary<PropertyInfo, Func<object, object>> _getters = new();
        readonly ConcurrentDictionary<PropertyInfo, Action<object, object>> _setters = new();

        List<Func<object, object>> Getters = new();
        List<Action<object, object>> Setters = new();

        Type CurrentType = null!;

        public void OnLoad() { }

        public void OnUpdate(double deltaTime) { }

        public void OnPreRender(double deltaTime) { }

        public void OnRender(double deltaTime)
        {
            if (
                ProjectManager.ActiveProject == null ||
                ProjectManager.ActiveProject!.ActiveScene >= ProjectManager.ActiveProject!.Scenes.Count
            )
            {
                ImGui.SetCursorPos(ImGui.GetContentRegionAvail() / 2 - ImGui.CalcTextSize("No active scene") / 2);

                ImGui.Text("No active scene");

                return;
            }

            var scene = ProjectManager.ActiveProject.Scenes[ProjectManager.ActiveProject!.ActiveScene];

            var type = scene.SelectedObject < 0 ? scene.GetType() :
                       scene.SceneObjects[scene.SelectedObject].GetType();

            object actor = scene.SelectedObject < 0 ? scene :
                           scene.SceneObjects[scene.SelectedObject];

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(x => x.IsDefined(typeof(InspectorPublicAttribute), false))
                         .ToArray();

            if (type != CurrentType)
            {
                CurrentType = type;
                BakeFieldAccessors(properties);
            }

            for (int i = 0; i < properties.Length; i++)
            {
                if (IPropertyDrawer.Drawers.TryGetValue(properties[i].PropertyType, out var drawer))
                {
                    var data = Getters[i](actor);

                    if (drawer.Draw(properties[i].Name, ref data))
                        Setters[i](actor, data);
                }
            }
        }

        public void OnPostRender(double deltaTime) { }

        public void OnClose() { }

        public void BakeFieldAccessors(PropertyInfo[] fields)
        {
            Getters = fields.Select(GetGetter).ToList();
            Setters = fields.Select(GetSetter).ToList();
        }

        Func<object, object> GetGetter(MemberInfo member)
        {
            var objParam = Expression.Parameter(typeof(object), "obj");
            var declaringType = member.DeclaringType;
            var castObj = declaringType!.IsValueType
                ? Expression.Unbox(objParam, declaringType)
                : Expression.Convert(objParam, declaringType);

            Expression accessExpr = member switch
            {
                FieldInfo f => Expression.Field(castObj, f),
                PropertyInfo p => Expression.Property(castObj, p),
                _ => throw new ArgumentException("Member must be a field or property")
            };

            var boxed = Expression.Convert(accessExpr, typeof(object));
            return Expression.Lambda<Func<object, object>>(boxed, objParam).Compile();
        }

        Action<object, object> GetSetter(PropertyInfo property)
        {
            return _setters.GetOrAdd(property, p =>
            {
                if (!p.CanWrite)
                    throw new InvalidOperationException($"Property '{p.Name}' has no setter.");

                var objParam = Expression.Parameter(typeof(object), "obj");
                var valueParam = Expression.Parameter(typeof(object), "value");

                var castObj = p.DeclaringType!.IsValueType
                    ? Expression.Unbox(objParam, p.DeclaringType!)
                    : Expression.Convert(objParam, p.DeclaringType!);

                var propertyAccess = Expression.Property(castObj, p);
                var castValue = Expression.Convert(valueParam, p.PropertyType);
                var assign = Expression.Assign(propertyAccess, castValue);

                return Expression.Lambda<Action<object, object>>(assign, objParam, valueParam).Compile();
            });
        }
    }
}
