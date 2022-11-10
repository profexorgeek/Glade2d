using Glade2d;
using Glade2d.Services;
using Glade2dExample.Screens;
using Meadow;
using Meadow.Foundation.Graphics;
using System.Threading.Tasks;

// TODO: The namespace currently HAS to be MeadowApp or it won't launch. Once
// this is fixed, this should be updated to be a rational namespace
namespace MeadowApp
{
    public class GladeSample
    {
        public static void Main(string[] args)
        {
            MeadowOS.Main(args);
        }

    }

    public class MeadowApp : App<Meadow.Simulation.SimulatedMeadow<Meadow.Simulation.SimulatedPinout>> //App<F7FeatherV2>, IApp
    {
        Game glade;

        Meadow.Graphics.GtkDisplay display;

        public override Task Initialize()
        {
            LogService.Log.Level = Glade2d.Logging.LogLevel.Trace;
            LogService.Log.Trace("Beginning Meadow initialization...");

            // initialize display device
            display = new GtkDisplay(240 * 3, 240 * 3, ColorType.Format16bppRgb565);

            // ready to go!, set LED to green
            LogService.Log.Trace("Initialization complete");

            return base.Initialize();
        }

        public override Task Run()
        {
            glade = new Game();
            glade.Initialize(display, 4, EngineMode.GameLoop);
            glade.Renderer.RenderInSafeMode = false;
            glade.SleepMilliseconds = 20;
            _ = Task.Run(() =>
            {
                glade.Start(new MountainSceneScreen());
            });
            display.Run();

            return base.Run();
        }
    }
}
