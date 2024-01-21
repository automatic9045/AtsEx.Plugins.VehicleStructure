using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;
using SlimDX.Direct3D9;

using BveTypes.ClassWrappers;

namespace Automatic9045.AtsEx.VehicleStructure
{
    internal class VehicleStructure
    {
        private readonly Direct3DProvider Direct3DProvider;
        private readonly Train Train;
        private readonly IMatrixCalculator MatrixCalculator;
        private readonly bool Vibrate;
        private readonly Matrix FirstCarOriginToFront;

        private readonly List<float> VibrationCoefficients = new List<float>();

        public VehicleStructure(Direct3DProvider direct3DProvider, Train train, IMatrixCalculator matrixCalculator, bool vibrate, Matrix firstCarOriginToFront)
        {
            Direct3DProvider = direct3DProvider;
            Train = train;
            MatrixCalculator = matrixCalculator;
            Vibrate = vibrate;
            FirstCarOriginToFront = firstCarOriginToFront;

            Random Random = new Random();
            for (int i = 0; i < Train.TrainInfo.Structures.Count; i++)
            {
                float coefficient = i == 0 ? 1 : 0.2f + (float)Random.NextDouble();
                VibrationCoefficients.Add(coefficient);
            }
        }

        public void DrawTrains(double vehicleLocation, Matrix vehicleToBlock, Matrix blockToCamera)
        {
            Train.Location = vehicleLocation;

            int vehicleBlockLocation = (int)vehicleLocation / 25 * 25;
            Matrix vibration = default;

            for (int i = 0; i < Train.TrainInfo.Structures.Count; i++)
            {
                Structure car = Train.TrainInfo.Structures[i];

                double location = vehicleLocation + car.Location;
                Matrix carToBlock = MatrixCalculator.GetTrackMatrix(car, location, vehicleBlockLocation);
                if (i == 0)
                {
                    Matrix firstCarOriginToBlock = carToBlock;
                    Matrix blockToFirstCarOrigin = Matrix.Invert(firstCarOriginToBlock);
                    Matrix blockToFirstCarFront = blockToFirstCarOrigin * FirstCarOriginToFront;
                    Matrix vehicleFrontToFirstCarFront = vehicleToBlock * blockToFirstCarFront;

                    vibration = vehicleFrontToFirstCarFront;
                    vibration.M41 *= VibrationCoefficients[i];
                    vibration.M42 *= VibrationCoefficients[i];
                    vibration.M43 *= VibrationCoefficients[i];
                }

                Matrix transform = (Vibrate ? vibration : Matrix.Identity) * carToBlock * blockToCamera;
                Direct3DProvider.Device.SetTransform(TransformState.World, transform);

                car.Model.Draw(Direct3DProvider, false);
                car.Model.Draw(Direct3DProvider, true);
            }
        }
    }
}
