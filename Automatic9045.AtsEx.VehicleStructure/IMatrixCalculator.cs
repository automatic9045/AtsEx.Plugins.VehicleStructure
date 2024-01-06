using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BveTypes.ClassWrappers;
using SlimDX;

namespace Automatic9045.AtsEx.VehicleStructure
{
    internal interface IMatrixCalculator
    {
        Matrix GetTrackMatrix(LocatableMapObject mapObject, double to, double from);
    }
}
