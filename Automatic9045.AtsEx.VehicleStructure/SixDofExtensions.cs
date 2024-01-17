using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;

using BveTypes.ClassWrappers;

namespace Automatic9045.AtsEx.VehicleStructure
{
    internal static class SixDofExtensions
    {
        public static Matrix GetTranslation(this SixDof source)
            => Matrix.Translation((float)source.X, (float)source.Y, (float)source.Z);
    }
}
