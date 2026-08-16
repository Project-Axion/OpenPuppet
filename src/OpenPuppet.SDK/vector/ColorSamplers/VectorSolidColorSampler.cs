using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.vector.ColorSamplers
{
    public class VectorSolidColorSampler(Color4 color) : IVectorColorSampler
    {
        public Vector4 Color { get; set; } = color;
        public Color4 SampleColor(Vector3 position) => Color;
    }
}
