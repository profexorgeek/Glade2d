using Glade2d;
using Glade2d.Services;
using GladeInvade.Shared;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;

namespace GladeInvade.ProjectLab;

public class MeadowApp : App<F7FeatherV2>
{
    private IGraphicsDisplay _display = default!;
    private Meadow.Devices.ProjectLab _projectLab = default!;
    
    public override Task Initialize()
    {
        _projectLab = new Meadow.Devices.ProjectLab();
        _display = _projectLab.Display!;
        
        InitializeInput();
        return base.Initialize();
    }
    
    public override Task Run()
    {
        LogService.Log.Trace("Initializing Glade game engine...");
        var glade = new Game();
        glade.Initialize(_display, 2, EngineMode.GameLoop, RotationType._90Degrees);
        
        GladeInvadeGame.Run(glade);

        return base.Run();
    }

    private void InitializeInput()
    {
        _projectLab.UpButton!.PressStarted += (_, _) =>
            GameService.Instance.GameInstance.InputManager.ButtonPushed(GameConstants.InputNames.Action);
        _projectLab.UpButton!.PressEnded += (_, _) =>
            GameService.Instance.GameInstance.InputManager.ButtonReleased(GameConstants.InputNames.Action);
        
        _projectLab.RightButton!.PressStarted += (_, _) =>
            GameService.Instance.GameInstance.InputManager.ButtonPushed(GameConstants.InputNames.Right);
        _projectLab.RightButton!.PressEnded += (_, _) =>
            GameService.Instance.GameInstance.InputManager.ButtonReleased(GameConstants.InputNames.Right);
        
        _projectLab.LeftButton!.PressStarted += (_, _) =>
            GameService.Instance.GameInstance.InputManager.ButtonPushed(GameConstants.InputNames.Left);
        _projectLab.LeftButton!.PressEnded += (_, _) =>
            GameService.Instance.GameInstance.InputManager.ButtonReleased(GameConstants.InputNames.Left);
    }
}