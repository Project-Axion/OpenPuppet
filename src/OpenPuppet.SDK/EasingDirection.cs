using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    [Flags]
    public enum EasingDirection
    {
        In = 1,
        Out = 2,
        InOut = In | Out,
    }
}
