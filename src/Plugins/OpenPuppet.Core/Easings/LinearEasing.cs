using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Easings
{
    public class LinearEasing : IEasingMode
    {
        public string Name { get; } = "Linear";

        public double Ease(double input,EasingDirection direction = EasingDirection.InOut) => input;
    }
}
