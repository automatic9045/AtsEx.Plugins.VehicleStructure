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

        private readonly List<float> VibrationCoefficients = new List<float>();

        public VehicleStructure(Direct3DProvider direct3DProvider, Train train, IMatrixCalculator matrixCalculator, bool vibrate)
        {
            Direct3DProvider = direct3DProvider;
            Train = train;
            MatrixCalculator = matrixCalculator;
            Vibrate = vibrate;

            Random Random = new Random();
            for (int i = 0; i < Train.TrainInfo.Structures.Count; i++)
            {
                float coefficient = i == 0 ? 1 : 0.2f + (float)Random.NextDouble();
                VibrationCoefficients.Add(coefficient);
            }
        }

        public void DrawTrains(double vehicleLocation, Matrix viewMatrix, Matrix vibrationMatrix)
        {
            Train.Location = vehicleLocation;

            for (int i = 0; i < Train.TrainInfo.Structures.Count; i++)
            {
                Structure car = Train.TrainInfo.Structures[i];

                Matrix carVibrationMatrix = vibrationMatrix;
                carVibrationMatrix.M41 *= VibrationCoefficients[i];
                carVibrationMatrix.M42 *= VibrationCoefficients[i];
                carVibrationMatrix.M43 *= VibrationCoefficients[i];

                Matrix trackMatrix = MatrixCalculator.GetTrackMatrix(car, vehicleLocation + car.Location, (int)vehicleLocation / 25 * 25);
                Matrix transform = (Vibrate ? carVibrationMatrix : Matrix.Identity) * trackMatrix * viewMatrix;
                Direct3DProvider.Device.SetTransform(TransformState.World, transform);

                car.Model.Draw(Direct3DProvider, false);
                car.Model.Draw(Direct3DProvider, true);
            }
        }
    }
}
