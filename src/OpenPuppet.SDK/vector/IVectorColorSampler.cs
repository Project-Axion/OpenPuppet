using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.vector
{
    public interface IVectorColorSampler
    {
        public Color4 SampleColor(Vector3 position);
    }
}
