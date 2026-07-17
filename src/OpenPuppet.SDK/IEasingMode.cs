using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public interface IEasingMode
    {
        public static Dictionary<string, IEasingMode> EasingModes { get; } = new();

        public string Name { get; }

        public double Ease(double input, EasingDirection direction = EasingDirection.InOut);

        public static void Register(string registry, IEasingMode mode) => EasingModes[registry] = mode;
        public static double Ease(string registry,double input, EasingDirection direction)
        {
            if (registry == null || !EasingModes.ContainsKey(registry)) return 0;

            return EasingModes[registry].Ease(input,direction);
        }

        public static double Ease(Easing easing, double input) => Ease(easing.Mode,input,easing.Direction);
    }

    public struct Easing(string mode,EasingDirection direction = EasingDirection.InOut)
    {
        public string Mode { get; set; } = mode;
        public EasingDirection Direction { get; set; } = direction;
    }
}
