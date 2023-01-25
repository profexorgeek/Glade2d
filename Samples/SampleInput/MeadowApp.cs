using Glade2d;
using Glade2d.Services;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Leds;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SampleInput
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        IGraphicsDisplay display;
        RgbPwmLed onboardLed;
        Mcp23008 mcp;
        PushButton btnUp;
        PiezoSpeaker speaker;
        Game glade;

        public override Task Run()
        {

            return base.Run();
        }

        public override Task Initialize()
        {
            LogService.Log.Trace("Initializing...");
            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);
            onboardLed.SetColor(Color.Red);

            InitializeMcp();
            InitializeDisplay();
            InitializeGlade();
            InitializeInput();
            InitializeAudio();

            onboardLed.SetColor(Color.YellowGreen);

            return base.Initialize();
        }

        void InitializeGlade()
        {
            LogService.Log.Trace("Starting Glade2d...");
            glade = new Game();
            glade.Initialize(display, 1, EngineMode.RenderOnDemand);
            glade.Start();
        }

        void InitializeMcp()
        {
            LogService.Log.Trace("Initializing MCP...");
            var mcp_in = Device.CreateDigitalInputPort(
                Device.Pins.D09,
                InterruptMode.EdgeRising,
                ResistorMode.InternalPullDown);
            var mcp_out = Device.CreateDigitalOutputPort(Device.Pins.D14);
            mcp = new Mcp23008(Device.CreateI2cBus(), 0x20, mcp_in, mcp_out);
        }

        void InitializeDisplay()
        {
            LogService.Log.Trace("Initializing SPI bus...");
            var config = new SpiClockConfiguration(
                new Frequency(48000, Frequency.UnitType.Kilohertz),
                SpiClockConfiguration.Mode.Mode3);
            var spi = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.COPI, Device.Pins.CIPO, config);

            LogService.Log.Trace("Initializing ST7789 display...");
            var chipSelectPort = mcp.CreateDigitalOutputPort(mcp.Pins.GP5);
            var dcPort = mcp.CreateDigitalOutputPort(mcp.Pins.GP6);
            var resetPort = mcp.CreateDigitalOutputPort(mcp.Pins.GP7);
            var st7789 = new St7789(
                spiBus: spi,
                chipSelectPort: chipSelectPort,
                dataCommandPort: dcPort,
                resetPort: resetPort,
                width: 240, height: 240,
                colorMode: ColorType.Format16bppRgb565
                );
            
            st7789.SetRotation(TftSpiBase.Rotation.Rotate_90);
            display = st7789;
        }

        void InitializeInput()
        {
            var btnPort = mcp.CreateDigitalInputPort(mcp.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            btnUp = new PushButton(btnPort);
            btnUp.PressStarted += BtnUp_PressStarted;
            btnUp.PressEnded += BtnUp_PressEnded;
        }

        void InitializeAudio()
        {
            speaker = new PiezoSpeaker(Device, Device.Pins.D11);
        }

        private void BtnUp_PressEnded(object sender, EventArgs e)
        {
            LogService.Log.Trace("Button Up press ended.");
            glade.Renderer.BackgroundColor = Color.YellowGreen;
            glade.Tick();
            onboardLed.SetColor(Color.YellowGreen);
        }

        private async void BtnUp_PressStarted(object sender, EventArgs e)
        {
            LogService.Log.Trace("Button Up press started.");
            onboardLed.SetColor(Color.Purple);
            glade.Renderer.BackgroundColor = Color.Purple;
            glade.Tick();
            await speaker.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(250));
        }
    }
}