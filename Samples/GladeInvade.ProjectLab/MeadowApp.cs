using Glade2d;
using Glade2d.Services;
using GladeInvade.Shared;
using GladeInvade.Shared.Screens;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Units;

namespace GladeInvade.ProjectLab;

public class MeadowApp : App<F7FeatherV2>
{
    private IGraphicsDisplay? _display;
    private GladeInvadeGame? _gladeInvadeGame;
    
    public override Task Initialize()
    {
        InitializeDisplay();
        return base.Initialize();
    }
    
    public override Task Run()
    {
        LogService.Log.Trace("Initializing Glade game engine...");
        var glade = new Game();
        glade.Initialize(_display, 2, EngineMode.GameLoop, RotationType._90Degrees);

        _gladeInvadeGame = new GladeInvadeGame(glade);

        return base.Run();
    }
    
    private void InitializeDisplay()
    {
        LogService.Log.Trace("Initializing SPI bus...");
        var config = new SpiClockConfiguration(
            new Frequency(48000, Frequency.UnitType.Kilohertz),
            SpiClockConfiguration.Mode.Mode3);
        var spi = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.COPI, Device.Pins.CIPO, config);

        LogService.Log.Trace("Initializing MCP...");
        var mcpIn = Device.CreateDigitalInputPort(
            Device.Pins.D09,
            InterruptMode.EdgeRising,
            ResistorMode.InternalPullDown);
        
        var mcpOut = Device.CreateDigitalOutputPort(Device.Pins.D14);
        var mcp = new Mcp23008(Device.CreateI2cBus(), 0x20, mcpIn, mcpOut);

        LogService.Log.Trace("Initializing ST7789 display...");
        var chipSelectPort = mcp.CreateDigitalOutputPort(mcp.Pins.GP5);
        var dcPort = mcp.CreateDigitalOutputPort(mcp.Pins.GP6);
        var resetPort = mcp.CreateDigitalOutputPort(mcp.Pins.GP7);
        
        _display = new St7789(
            spiBus: spi,
            chipSelectPort: chipSelectPort,
            dataCommandPort: dcPort,
            resetPort: resetPort,
            width: 240, height: 240,
            colorMode: ColorType.Format16bppRgb565
        );
    }
}