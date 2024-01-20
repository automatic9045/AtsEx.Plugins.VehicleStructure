using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BveTypes.ClassWrappers;

namespace Automatic9045.AtsEx.VehicleStructure
{
    internal class TrainFactory
    {
        private readonly TimeManager TimeManager;
        private readonly UserVehicleLocationManager LocationManager;
        private readonly Route Route;
        private readonly DrawDistanceManager DrawDistanceManager;

        public TrainFactory(TimeManager timeManager, UserVehicleLocationManager locationManager, Route route, DrawDistanceManager drawDistanceManager)
        {
            TimeManager = timeManager;
            LocationManager = locationManager;
            Route = route;
            DrawDistanceManager = drawDistanceManager;
        }

        public Train Create(IEnumerable<Structure> structures)
        {
            TrainInfo trainInfo = new TrainInfo();
            foreach (Structure structure in structures)
            {
                trainInfo.Structures.Add(structure);
            }

            Train train = new Train(TimeManager, LocationManager, Route, trainInfo, DrawDistanceManager);
            return train;
        }

        public Train Create(Data.Structure[] data, string baseDirectory)
        {
            List<Structure> structures = data
                .Select(x =>
                {
                    string modelPath = Path.Combine(baseDirectory, x.Model);
                    Model model = Model.FromXFile(modelPath);
                    Structure result = new Structure(
                        x.Distance, string.Empty,
                        0, 0, x.Z, 0, 0, 0,
                        TiltOptions.TiltsAlongGradient | TiltOptions.TiltsAlongCant, x.Span, model);
                    return result;
                })
                .ToList();

            return Create(structures);
        }
    }
}
