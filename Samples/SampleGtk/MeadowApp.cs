using Glade2d;
using Glade2d.Graphics.SelfRenderer;
using Glade2d.Graphics;
using Glade2d.Profiling;
using Glade2d.Services;
using GladeSampleShared.Screens;
using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Peripherals.Displays;
using System.Threading.Tasks;


namespace SampleGtk
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
        GtkDisplay display;

        public override Task Initialize()
        {
            LogService.Log.Level = Glade2d.Logging.LogLevel.Trace;
            LogService.Log.Trace("Beginning Meadow initialization...");
            display = new GtkDisplay(240, 240, ColorMode.Format16bppRgb565);
            LogService.Log.Trace("Initialization complete");
            return base.Initialize();
        }

        public override Task Run()
        {
            var textureManager = new TextureManager(MeadowOS.FileSystem.UserFileSystemRoot);
            var layerManager = new LayerManager();
            var profiler = new Profiler();
            var renderer = new GladeSelfRenderer(display, textureManager, layerManager, profiler);

            LogService.Log.Trace("Initializing Glade game engine...");
            glade = new Game();
            glade.Initialize(renderer, textureManager, layerManager, profiler);

            //glade.Renderer.RenderInSafeMode = false;
            glade.SleepMilliseconds = 20;
            _ = Task.Run(() =>
            {
                glade.Start(() => new GladeDemoScreen());
            });
            display.Run();

            return base.Run();
        }
    }
}
