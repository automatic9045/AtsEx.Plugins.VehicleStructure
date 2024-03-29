﻿using System;
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

                Matrix blockToCamera = vehicle.CameraLocation.TransformFromBlock;
                Matrix blockToVehicle =
                    vehicle.VibrationManager.Positioner.BlockToCarCenterTransform.Matrix
                    * vehicle.VibrationManager.CarBodyTransform.Matrix
                    * vehicle.VibrationManager.ViewPoint.GetTranslation();

                foreach (VehicleStructure vehicleStructure in VehicleStructures)
                {
                    vehicleStructure.DrawTrains(vehicleLocation, Matrix.Invert(blockToVehicle), blockToCamera);
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
                .Select(group =>
                {
                    Train train = trainFactory.Create(group.Structures, BaseDirectory);
                    Matrix firstCarOriginToFront = Matrix.Translation(0, 0, (float)group.FirstStructureFront);

                    VehicleStructure result = new VehicleStructure(Direct3DProvider.Instance, train, matrixCalculator, group.Vibrate, firstCarOriginToFront);
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
