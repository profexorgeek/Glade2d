using Glade2d.Examples;
using Glade2d.Graphics;
using Glade2d.Services;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Units;

namespace Glade2d
{
    // Change F7MicroV2 to F7Micro for V1.x boards
    public class Glade2d : App<F7MicroV2, Glade2d>
    {
        RgbPwmLed onboardLed;
        Renderer renderer;
        IGraphicsDriver graphicsDevice;

        public Glade2d()
        {
            LogService.Log.Level = Logging.LogLevel.Trace;

            Initialize();
            Start();
        }

        void Initialize()
        {
            LogService.Log.Trace("Initializing glade2d...");

            onboardLed = new RgbPwmLed(
                device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue);
            onboardLed.SetColor(Color.Red);
            LogService.Log.Trace("Onboard LED initialized.");

            InitializeGraphicsDevice();
            InitializeRenderer();

            onboardLed.SetColor(Color.Green);
            LogService.Log.Trace("glade2d Initialization complete.");
        }

        void InitializeGraphicsDevice()
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
            graphicsDevice = new St7789(
                device: Device,
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                displayColorMode: ColorType.Format16bppRgb565,
                width: 240,
                height: 240);
            LogService.Log.Trace("St7789 Graphics Display initialized.");
        }

        void InitializeRenderer()
        {
            LogService.Log.Trace("Initializing Renderer...");
            renderer = Renderer.GetRendererForDevice(graphicsDevice, 2);
            renderer.BackgroundColor = new Color(40, 204, 223);
            renderer.ShowFPS = true;
            LogService.Log.Trace("Renderer Initialized.");
        }

        void Start()
        {
            GameService.Instance.Initialize();

            GameService.Instance.CurrentScreen = new Glade2dScreen();

            LogService.Log.Trace("Starting game loop.");
            while(true)
            {
                Update();
                Draw();
            }
        }

        void Update()
        {
            GameService.Instance.Time?.Update();
            GameService.Instance.CurrentScreen?.Update();
        }

        void Draw()
        {
            renderer.Reset();
            var screen = GameService.Instance.CurrentScreen;
            if (screen != null)
            {
                // TODO: this is a hack, figure out how to protect list
                // while also making it available to the renderer
                var sprites = screen.AccessSpritesForRenderingOnly();
                for (var i = 0; i < sprites.Count; i++)
                {
                    renderer.DrawSprite(sprites[i]);
                }
            }
            renderer.RenderToDisplay();
        }
    }
}
