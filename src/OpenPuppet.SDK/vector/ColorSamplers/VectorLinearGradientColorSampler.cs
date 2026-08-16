using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.vector.ColorSamplers
{
    public class VectorLinearGradientColorSampler(Color4 a, float startA, Color4 b, float startB, double angleDeg) : IVectorColorSampler
    {
        public Color4 A { get; set; } = a;
        public Color4 B { get; set; } = b;

        public float StartA { get; set; } = startA;
        public float StartB { get; set; } = startB;

        public double Angle { get; set; } = angleDeg;

        public Color4 SampleColor(Vector3 position)
        {
            float px = position.X - 0.5f;
            float py = position.Y - 0.5f;

            double angleRad = Angle * Math.PI / 180.0;

            float dx = (float)Math.Cos(angleRad);
            float dy = (float)Math.Sin(angleRad);

            float t = px * dx + py * dy;

            float maxExtent = Math.Abs(dx) * 0.5f + Math.Abs(dy) * 0.5f;
            float normalized01 = (t + maxExtent) / (2f * maxExtent);

            float range = StartB - StartA;
            float normalizedT = range != 0f ? (normalized01 - StartA) / range : 0f;
            normalizedT = Math.Clamp(normalizedT, 0f, 1f);

            return Vector4.Lerp(A, B, normalizedT);
        }
    }
}
