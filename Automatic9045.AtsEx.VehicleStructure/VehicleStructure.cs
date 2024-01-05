using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;

using BveTypes.ClassWrappers;

namespace Automatic9045.AtsEx.VehicleStructure
{
    internal class VehicleStructure
    {
        private readonly Direct3DProvider Direct3DProvider;
        private readonly Train Train;

        public VehicleStructure(Direct3DProvider direct3DProvider, Train train)
        {
            Direct3DProvider = direct3DProvider;
            Train = train;
        }

        public void DrawTrains(double vehicleLocation, Matrix viewMatrix)
        {
            Train.Location = vehicleLocation;
            Train.DrawCars(Direct3DProvider, viewMatrix);
        }
    }
}
