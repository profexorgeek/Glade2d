using Glade2d;
using Glade2d.Input;
using Glade2d.Services;
using GladePlatformer.Shared;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;


namespace GladePlatformer.ProjectLab;

public class MeadowApp : App<F7FeatherV2>
{
    private IGraphicsDisplay _display = default!;
    private Meadow.Devices.ProjectLab _projectLab = default!;
    
    public override Task Initialize()
    {
        _projectLab = new Meadow.Devices.ProjectLab();
        _display = _projectLab.Display!;
        
        return base.Initialize();
    }
    
    public override Task Run()
    {
        LogService.Log.Trace("Initializing Glade game engine...");
        var glade = new Game();
        glade.Initialize(_display, 2);
        InitializeInput(glade.InputManager);

        GladePlatformerGame.Run(glade);

        return base.Run();
    }

    private void InitializeInput(InputManager inputManager)
    {
        // inputManager.RegisterPushButton(_projectLab.UpButton!, GameConstants.InputNames.Action);
        // inputManager.RegisterPushButton(_projectLab.LeftButton!, GameConstants.InputNames.Left);
        // inputManager.RegisterPushButton(_projectLab.RightButton!, GameConstants.InputNames.Right);
    }
}
