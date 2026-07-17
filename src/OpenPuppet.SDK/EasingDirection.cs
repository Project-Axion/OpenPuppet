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
        In,
        Out,
        InOut = In & Out,
    }
}
