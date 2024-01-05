using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using BveTypes.ClassWrappers;
using ObjectiveHarmonyPatch;

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
        private VehicleStructure VehicleStructure;

        public PluginMain(PluginBuilder builder) : base(builder)
        {
            BveHacker.ScenarioCreated += OnScenarioCreated;

            string path = Path.Combine(BaseDirectory, "VehicleStructure.Config.xml");
            Config = Data.Config.Deserialize(path, true);

            FastMember.FastMethod drawObjectsMethod = BveHacker.BveTypes.GetClassInfoOf<ObjectDrawer>().GetSourceMethodOf(nameof(ObjectDrawer.Draw));
            DrawObjectsPatch = HarmonyPatch.Patch(nameof(BveHacker), drawObjectsMethod.Source, PatchType.Postfix);
            DrawObjectsPatch.Invoked += (sender, e) =>
            {
                double vehicleLocation = BveHacker.Scenario.LocationManager.Location;
                VehicleStructure.DrawTrains(vehicleLocation, BveHacker.Scenario.Vehicle.CameraLocation.TransformFromBlock);
                return PatchInvokationResult.DoNothing(e);
            };
        }

        public override void Dispose()
        {
            BveHacker.ScenarioCreated -= OnScenarioCreated;
        }

        private void OnScenarioCreated(ScenarioCreatedEventArgs e)
        {
            TrainFactory trainFactory = new TrainFactory(e.Scenario.TimeManager, e.Scenario.LocationManager, e.Scenario.Route, e.Scenario.ObjectDrawer.DrawDistanceManager);
            Train train = trainFactory.Create(Config.VehicleTrain, BaseDirectory);
            VehicleStructure = new VehicleStructure(Direct3DProvider.Instance, train);
        }

        public override TickResult Tick(TimeSpan elapsed)
        {
            return new VehiclePluginTickResult();
        }
    }
}
