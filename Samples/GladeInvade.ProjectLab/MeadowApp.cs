using Glade2d;
using Glade2d.Services;
using GladeInvade.Shared;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Units;

namespace GladeInvade.ProjectLab;

public class MeadowApp : App<F7FeatherV2>
{
    private IGraphicsDisplay _display = default!;
    private GladeInvadeGame _gladeInvadeGame = default!;
    private Mcp23008 _mcp = default!;
    
    public override Task Initialize()
    {
        InitializeDisplay();
        InitializeInput();
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
        _mcp = new Mcp23008(Device.CreateI2cBus(), 0x20, mcpIn, mcpOut);

        LogService.Log.Trace("Initializing ST7789 display...");
        var chipSelectPort = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP5);
        var dcPort = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP6);
        var resetPort = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP7);
        
        _display = new St7789(
            spiBus: spi,
            chipSelectPort: chipSelectPort,
            dataCommandPort: dcPort,
            resetPort: resetPort,
            width: 240, height: 240,
            colorMode: ColorType.Format16bppRgb565
        );
    }

    private void InitializeInput()
    {
        var upButtonPort = _mcp.CreateDigitalInputPort(_mcp.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        var upButton = new PushButton(upButtonPort);
        upButton.PressStarted += (_, _) =>
            GameService.Instance.GameInstance.InputManager.ButtonPushed(GameConstants.InputNames.Action);
        upButton.PressEnded += (_, _) =>
            GameService.Instance.GameInstance.InputManager.ButtonReleased(GameConstants.InputNames.Action);
        
        var leftButtonPort = _mcp.CreateDigitalInputPort(_mcp.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        var leftButton = new PushButton(leftButtonPort);
        leftButton.PressStarted += (_, _) =>
            GameService.Instance.GameInstance.InputManager.ButtonPushed(GameConstants.InputNames.Left);
        leftButton.PressEnded += (_, _) =>
            GameService.Instance.GameInstance.InputManager.ButtonReleased(GameConstants.InputNames.Left);
        
        var rightButtonPort = _mcp.CreateDigitalInputPort(_mcp.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
        var rightButton = new PushButton(rightButtonPort);
        rightButton.PressStarted += (_, _) =>
            GameService.Instance.GameInstance.InputManager.ButtonPushed(GameConstants.InputNames.Right);
        rightButton.PressEnded += (_, _) =>
            GameService.Instance.GameInstance.InputManager.ButtonReleased(GameConstants.InputNames.Right);
    }
}