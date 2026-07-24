using Newtonsoft.Json;
using OpenPuppet.SDK.Projects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.TimelineTracks
{
    public class PropertyTimeline<T> : ITimelineTrack
    {
        public string Name { get; set; }
        public Guid HolderID { get; set; }

        SceneMetadata _scene;

        [JsonIgnore]
        public SceneMetadata Scene
        {
            get => _scene; set
            {
                _scene = value;

                var (gtr, str) = ResolveLocal();

                GetValue = gtr;
                SetValue = str;
            }
        }

        public string Property { get; set; }

        Func<T> GetValue;
        Action<T> SetValue;

        [JsonConstructor]
        public PropertyTimeline(Guid holder, SceneMetadata scene, string name, string property)
        {
            Name = name;
            HolderID = holder;
            Property = property;
            Scene = scene;
        }

        public PropertyTimeline(Guid holder, SceneMetadata scene, string name, Expression<Func<T>> property) :
            this(holder, scene, name, GetPropertyPath(property))
        { }

        IMutator<T> Mutator = IMutator<T>.GetMutator();

        [JsonIgnore]
        public Dictionary<TimeSpan, bool> SelectedKeyframes { get; set; } = new();
        public Dictionary<TimeSpan, Easing> KeyframeEasings { get; set; } = new();
        public SortedList<TimeSpan, T> Keyframes { get; set; } = new();

        public void AddKeyframe(TimeSpan timestamp) => Keyframes.Add(timestamp, GetValue());
        public void UpdateKeyframe(TimeSpan timestamp) => Keyframes[timestamp] = GetValue();
        public bool KeyframeExists(TimeSpan keyframe) => Keyframes.ContainsKey(keyframe);

        public bool RemoveKeyframe(TimeSpan timestamp)
        {
            KeyframeEasings.Remove(timestamp);
            SelectedKeyframes.Remove(timestamp);

            return Keyframes.Remove(timestamp);
        }

        public void SetKeyframeEasing(TimeSpan timestamp, Easing easing) =>
            KeyframeEasings[timestamp] = easing;

        public void MoveKeyframe(TimeSpan timestamp, TimeSpan newFrame)
        {
            if (!Keyframes.TryGetValue(timestamp, out var keyframeValue)) return;

            if (KeyframeEasings.ContainsKey(timestamp))
            {
                KeyframeEasings[newFrame] = KeyframeEasings[timestamp];
                KeyframeEasings.Remove(timestamp);
            }

            if (SelectedKeyframes.ContainsKey(timestamp))
            {
                SelectedKeyframes[newFrame] = SelectedKeyframes[timestamp];
                SelectedKeyframes.Remove(timestamp);
            }

            Keyframes[newFrame] = Keyframes[timestamp];
            Keyframes.Remove(timestamp);
        }

        public List<(TimeSpan, Easing)> GetSelectedKeyframes() =>
            SelectedKeyframes.Keys.Where(x => SelectedKeyframes[x]).Select(x => (x, GetEasing(x))).ToList();

        public List<(TimeSpan frame, bool selected, Easing easing)> GetKeyframes() =>
            Keyframes.Keys.Select(x => (x, GetKeyframeSelection(x), GetEasing(x))).ToList();

        public bool IsKeyframeSelected(TimeSpan keyframe) => GetKeyframeSelection(keyframe);

        Easing GetEasing(TimeSpan keyframe)
        {
            if (!KeyframeEasings.ContainsKey(keyframe)) KeyframeEasings.Add(keyframe, new(null!));

            return KeyframeEasings[keyframe];
        }

        bool GetKeyframeSelection(TimeSpan kf)
        {
            if (!SelectedKeyframes.ContainsKey(kf)) SelectedKeyframes.Add(kf, false);

            return SelectedKeyframes[kf];
        }

        public void Mutate(TimeSpan timestamp)
        {
            var (prev, next) = ITimelineTrack.GetSurroundingKeyframes(timestamp, Keyframes.Keys);

            if (!prev.HasValue && !next.HasValue) return;

            if (Mutator == null || !next.HasValue) SetValue(Keyframes[prev!.Value]);
            else if (!prev.HasValue) SetValue(Keyframes[next!.Value]);
            else SetValue(
                Mutator.Mutate(
                    Keyframes[prev!.Value], Keyframes[next!.Value],
                    IEasingMode.Ease(
                        GetEasing(prev!.Value),
                        (timestamp - prev.Value) / (next.Value - prev.Value)
                    )
                )
            );
        }

        public bool KeyframeInRange(TimeSpan range, out TimeSpan keyframe, float radius = 12)
        {
            foreach (var kvp in Keyframes)
            {
                if (Math.Abs((kvp.Key - range).TotalMilliseconds) <= radius)
                {
                    keyframe = kvp.Key;
                    return true;
                }
            }

            keyframe = default;
            return false;
        }

        public void ToggleSelectKeyframe(TimeSpan timestamp)
        {
            if (!SelectedKeyframes.ContainsKey(timestamp)) return;

            SelectedKeyframes[timestamp] = !SelectedKeyframes[timestamp];
        }

        public void DeselectAll()
        {
            foreach (var item in SelectedKeyframes.Keys)
                SelectedKeyframes[item] = false;
        }

        public void SelectAll()
        {
            foreach (var item in SelectedKeyframes.Keys)
                SelectedKeyframes[item] = true;
        }

        (Func<T> getter, Action<T> setter) ResolveLocal()
        {
            if (Scene == null) return (null!, null!);

            object root = HolderID == Guid.Empty ? Scene : Scene.SceneObjects.First(x => x.ID == HolderID);

            return ResolvePath<T>(root, Property);
        }

        static (Func<T1> getter, Action<T1> setter) ResolvePath<T1>(object root, string path)
        {
            var parts = path.Split('.');

            object owner = root;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var prop = owner.GetType().GetProperty(parts[i])
                    ?? throw new InvalidOperationException($"'{parts[i]}' not found on {owner.GetType()}");
                owner = prop.GetValue(owner)!;
            }

            var lastProp = owner.GetType().GetProperty(parts[^1])
                ?? throw new InvalidOperationException($"'{parts[^1]}' not found on {owner.GetType()}");

            return (
                () => (T1)lastProp.GetValue(owner)!,
                v => lastProp.SetValue(owner, v)
            );
        }

        static string GetPropertyPath<T1>(Expression<Func<T1>> expr)
        {
            var names = new List<string>();
            var current = expr.Body;

            while (current is MemberExpression member)
            {
                names.Add(member.Member.Name);
                current = member.Expression;
            }

            names.Reverse();
            return string.Join(".", names);
        }
    }
}
