using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using BveTypes.ClassWrappers;
using ObjectiveHarmonyPatch;
using SlimDX;

using AtsEx.PluginHost;
using AtsEx.PluginHost.Plugins;

namespace Automatic9045.AtsEx.VehicleStructure
{
    [PluginType(PluginType.VehiclePlugin)]
    public class PluginMain : AssemblyPluginBase
    {
        private static readonly string AssemblyLocation = Assembly.GetExecutingAssembly().Location;
        private static readonly string BaseDirectory = Path.GetDirectoryName(AssemblyLocation);

        private readonly Data.Config Config;
        private readonly HarmonyPatch DrawObjectsPatch;

        private List<VehicleStructure> VehicleStructures;

        public PluginMain(PluginBuilder builder) : base(builder)
        {
            BveHacker.ScenarioCreated += OnScenarioCreated;

            string path = Path.Combine(BaseDirectory, "VehicleStructure.Config.xml");
            Config = Data.Config.Deserialize(path, true);

            FastMember.FastMethod drawObjectsMethod = BveHacker.BveTypes.GetClassInfoOf<ObjectDrawer>().GetSourceMethodOf(nameof(ObjectDrawer.Draw));
            DrawObjectsPatch = HarmonyPatch.Patch(nameof(BveHacker), drawObjectsMethod.Source, PatchType.Postfix);
            DrawObjectsPatch.Invoked += (sender, e) =>
            {
                UserVehicleLocationManager locationManager = BveHacker.Scenario.LocationManager;
                Vehicle vehicle = BveHacker.Scenario.Vehicle;
                MyTrack myTrack = BveHacker.Scenario.Route.MyTrack;

                double vehicleLocation = locationManager.Location;

                Matrix transform = vehicle.CameraLocation.TransformFromBlock;
                Matrix vibration =
                    vehicle.CameraLocation.TransformFromCameraHomePosition
                    * Matrix.Invert(vehicle.CameraLocation.TransformFromBlock)
                    * vehicle.VibrationManager.Positioner.BlockToCarCenterTransform.Matrix
                    * vehicle.VibrationManager.CarBodyTransform.Matrix;

                foreach (VehicleStructure info in VehicleStructures)
                {
                    info.DrawTrains(vehicleLocation, transform, vibration);
                }

                return PatchInvokationResult.DoNothing(e);
            };
        }

        public override void Dispose()
        {
            BveHacker.ScenarioCreated -= OnScenarioCreated;

            DrawObjectsPatch.Dispose();
        }

        private void OnScenarioCreated(ScenarioCreatedEventArgs e)
        {
            TrainFactory trainFactory = new TrainFactory(e.Scenario.TimeManager, e.Scenario.LocationManager, e.Scenario.Route, e.Scenario.ObjectDrawer.DrawDistanceManager);
            MatrixCalculator matrixCalculator = new MatrixCalculator(e.Scenario.Route);

            VehicleStructures = Config.VehicleTrain.StructureGroups
                .AsParallel()
                .Select(group =>
                {
                    Train train = trainFactory.Create(group.Structures, BaseDirectory);
                    VehicleStructure result = new VehicleStructure(Direct3DProvider.Instance, train, matrixCalculator, group.Vibrate);
                    return result;
                })
                .ToList();
        }

        public override TickResult Tick(TimeSpan elapsed)
        {
            return new VehiclePluginTickResult();
        }


        private class MatrixCalculator : IMatrixCalculator
        {
            private readonly Route Route;

            public MatrixCalculator(Route route)
            {
                Route = route;
            }

            public Matrix GetTrackMatrix(LocatableMapObject mapObject, double to, double from)
                => Route.GetTrackMatrix(mapObject, to, from);
        }
    }
}
