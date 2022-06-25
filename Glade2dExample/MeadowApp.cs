using Glade2d;
using Glade2d.Services;
using Glade2dExample.Screens;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

// TODO: The namespace currently HAS to be MeadowApp or it won't launch. Once
// this is fixed, this should be updated to be a rational namespace
namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>, IApp
    {
        RgbPwmLed onboardLed;
        Game glade;

        async Task IApp.Initialize()
        {
            LogService.Log.Level = Glade2d.Logging.LogLevel.Trace;
            LogService.Log.Trace("Beginning Meadow initialization...");

            // set onboard LED to red
            onboardLed = new RgbPwmLed(
                device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue);
            onboardLed.SetColor(Color.Red);

            // initialize display device
            var display = GetDisplayDevice();

            // initialize Glade2d game engine by providing the display
            // device and setting a render scale
            glade = new Game();
            glade.Initialize(display, 4);

            // ready to go!, set LED to green
            onboardLed.SetColor(Color.Green);
            LogService.Log.Trace("Initialization complete");
        }

        async Task IApp.Run()
        {
            // start glade game with a custom screen
            glade.Start(new MountainSceneScreen());
        }

        St7789 GetDisplayDevice()
        {
            LogService.Log.Trace("Initializing St7789 Graphics Display.");

            var config = new SpiClockConfiguration(
                speed: new Frequency(48000, Frequency.UnitType.Kilohertz),
                mode: SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(
                clock: Device.Pins.SCK,
                copi: Device.Pins.MOSI,
                cipo: Device.Pins.MISO,
                config: config);
            var graphicsDevice = new St7789(
                device: Device,
                spiBus: spiBus,
                chipSelectPin: Device.Pins.A03,
                dcPin: Device.Pins.A04,
                resetPin: Device.Pins.A05,
                displayColorMode: ColorType.Format16bppRgb565,
                width: 240,
                height: 240);
            LogService.Log.Trace("St7789 Graphics Display initialized.");

            return graphicsDevice;
        }
    }
}
