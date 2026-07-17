using Newtonsoft.Json;
using OpenPuppet.SDK.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public interface ITimelineTrack
    {
        public string Name { get; set; }
        public Guid HolderID { get; set; }
        [JsonIgnore] public SceneMetadata Scene { get; set; }

        public void AddKeyframe(TimeSpan timestamp);
        public void UpdateKeyframe(TimeSpan timestamp);
        public bool RemoveKeyframe(TimeSpan timestamp);
        public void MoveKeyframe(TimeSpan timestamp, TimeSpan newFrame);
        public void SetKeyframeEasing(TimeSpan timestamp, Easing easing);
        public List<(TimeSpan frame, bool selected, Easing easing)> GetKeyframes();
        public List<(TimeSpan frame, Easing easing)> GetSelectedKeyframes();

        public bool KeyframeInRange(TimeSpan range,out TimeSpan keyframe, float radius = 12);
        public bool KeyframeExists(TimeSpan keyframe);
        public bool IsKeyframeSelected(TimeSpan keyframe);

        public void ToggleSelectKeyframe(TimeSpan timestamp);
        public void DeselectAll();
        public void SelectAll();

        public void Mutate(TimeSpan timestamp);

        public static (TimeSpan? prev, TimeSpan? next) GetSurroundingKeyframes(TimeSpan timestamp, IList<TimeSpan> keys)
        {
            int index = BinarySearchIndex(keys, timestamp);

            if (index >= 0) return (keys[index], keys[index]);

            int insertAt = ~index;
            TimeSpan? prev = insertAt > 0 ? keys[insertAt - 1] : null;
            TimeSpan? next = insertAt < keys.Count ? keys[insertAt] : null;

            return (prev, next);
        }

        private static int BinarySearchIndex(IList<TimeSpan> keys, TimeSpan target)
        {
            int lo = 0, hi = keys.Count - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                int cmp = keys[mid].CompareTo(target);
                if (cmp == 0) return mid;
                if (cmp < 0) lo = mid + 1; else hi = mid - 1;
            }
            return ~lo;
        }
    }
}
