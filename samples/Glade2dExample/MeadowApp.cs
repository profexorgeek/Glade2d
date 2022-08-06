using Glade2d;
using Glade2d.Services;
using Glade2dExample.Screens;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

// TODO: The namespace currently HAS to be MeadowApp or it won't launch. Once
// this is fixed, this should be updated to be a rational namespace
namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>, IApp
    {
        RgbPwmLed onboardLed;
        Game glade;
        PiezoSpeaker piezo;
        IGraphicsDisplay display;

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

            //LogService.Log.Trace("Initializing piezo and playing tone...");
            //piezo = new PiezoSpeaker(Device, Device.Pins.D11);
            //await piezo.PlayTone(new Frequency(440), 1000);
            //var button = new PushButton(Device, Device.Pins.D10, ResistorMode.InternalPullDown);


            //LogService.Log.Trace($"Button debounce is: {button.DebounceDuration}");
            //button.PressStarted += Button_PressStarted;
            //button.PressEnded += Button_PressEnded;
            //button.DebounceDuration = TimeSpan.FromMilliseconds(10);

            //LogService.Log.Trace($"New button debounce is: {button.DebounceDuration}");


            // initialize display device
            display = GetDisplayDeviceMeadowV2Ili9341();

            // ready to go!, set LED to green
            onboardLed.SetColor(Color.Green);
            LogService.Log.Trace("Initialization complete");
        }

        async void Button_PressStarted(object sender, System.EventArgs e)
        {
            LogService.Log.Trace("Button press started!");
            onboardLed.SetColor(Color.Green);
        }

        void Button_PressEnded(object sender, System.EventArgs e)
        {
            LogService.Log.Trace("Button press ended!");
            onboardLed.SetColor(Color.Red);

            if(glade.Mode == EngineMode.RenderOnDemand)
            {
                LogService.Log.Trace("Rendering scene on demand...");
                glade.Tick();
            }
        }

        async Task IApp.Run()
        {
            glade = new Game();
            glade.Initialize(display, 4, EngineMode.GameLoop);
            glade.Renderer.RenderInSafeMode = false;
            glade.SleepMilliseconds = 0;
            glade.Start(new MountainSceneScreen());
        }

        IGraphicsDisplay GetDisplayDeviceProjectLabs()
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

        IGraphicsDisplay GetDisplayDeviceMeadowV2St7789()
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
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                displayColorMode: ColorType.Format16bppRgb565,
                width: 240,
                height: 240);
            LogService.Log.Trace("St7789 Graphics Display initialized.");

            return graphicsDevice;
        }

        IGraphicsDisplay GetDisplayDeviceMeadowV2Ili9341()
        {
            LogService.Log.Trace("Initializing ILI9341 Graphics Display.");

            var config = new SpiClockConfiguration(
                speed: new Frequency(48000, Frequency.UnitType.Kilohertz),
                mode: SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(
                clock: Device.Pins.SCK,
                copi: Device.Pins.MOSI,
                cipo: Device.Pins.MISO,
                config: config);
            var graphicsDevice = new Ili9341(
                device: Device,
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                displayColorMode: ColorType.Format16bppRgb565,
                width: 240,
                height: 320);
            LogService.Log.Trace("ILI9341 Graphics Display initialized.");
            return graphicsDevice;
        }
    }
}
