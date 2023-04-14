using Glade2d;
using Glade2d.Graphics;
using Glade2d.Input;
using Glade2d.Services;
using GladePlatformer.Shared;
using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Units;

namespace GladePlatformer.Juego;

public class MeadowApp : App<Meadow.Devices.F7CoreComputeV2>
{
    private IGraphicsDisplay _display = default!;
    private Mcp23008 _mcp1 = default!;
    private Mcp23008 _mcp2 = default!;

    public override Task Initialize()
    {
        InitializeBoard();
        LogService.Log.Trace("Board initialized");
        return base.Initialize();
    }

    public override Task Run()
    {
        LogService.Log.Trace("Initializing Glade game engine...");
        var glade = new Game();
        var inputs = SetupInputs();
        glade.Initialize(_display, inputs, 2, displayRotation: RotationType._270Degrees);

        GladePlatformerGame.Run(glade);

        return base.Run();
    }

    private void InitializeBoard()
    {
        LogService.Log.Trace("Creating I2C Bus");
        var i2CBus = Device.CreateI2cBus();

        LogService.Log.Trace("Creating MCP1");
        var mcpReset = Device.CreateDigitalOutputPort(Device.Pins.D11, true);
        var mcp1Interrupt = Device.CreateDigitalInputPort(Device.Pins.D09, InterruptMode.EdgeRising);
        _mcp1 = new Mcp23008(i2CBus, 0x20, mcp1Interrupt, mcpReset);

        LogService.Log.Trace("Creating MCP2");
        var mcp2Interrupt = Device.CreateDigitalInputPort(Device.Pins.D10, InterruptMode.EdgeRising);
        _mcp2 = new Mcp23008(i2CBus, 0x21, mcp2Interrupt);
        
        LogService.Log.Trace("Initializing SPI bus...");
        var config = new SpiClockConfiguration(
            new Frequency(48000, Frequency.UnitType.Kilohertz),
            SpiClockConfiguration.Mode.Mode0);
        
        var spi = Device.CreateSpiBus(Device.Pins.SPI5_SCK, Device.Pins.SPI5_COPI, Device.Pins.SPI5_CIPO, config);

        LogService.Log.Trace("Initializing ILI9341 display...");
        var chipSelectPort = _mcp1.CreateDigitalOutputPort(_mcp1.Pins.GP5);
        var dcPort = _mcp1.CreateDigitalOutputPort(_mcp1.Pins.GP6);
        var resetPort = _mcp1.CreateDigitalOutputPort(_mcp1.Pins.GP7);

        // Turn on the display's backlight
        Device.CreateDigitalOutputPort(Device.Pins.D05, true);
        
        var ili9341 = new Ili9341(
            spi,
            chipSelectPort,
            dcPort,
            resetPort,
            240,
            320,
            ColorMode.Format16bppRgb565);

        ili9341.Rotation = RotationType._90Degrees;

        _display = ili9341;
    }

    private GameInputs SetupInputs()
    {
        var dPadLeftPort = _mcp1.CreateDigitalInputPort(_mcp1.Pins.GP4, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        var dPadRightPort = _mcp1.CreateDigitalInputPort(_mcp1.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        var btnDownPort = _mcp2.CreateDigitalInputPort(_mcp2.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);

        return new GameInputs
        {
            Left = dPadLeftPort,
            Right = dPadRightPort,
            Jump = btnDownPort,
        };
    }
}