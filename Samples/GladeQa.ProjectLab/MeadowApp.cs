using Glade2d;
using Glade2d.Input;
using Glade2d.Services;
using GladeQa.Shared;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;

namespace GladeQa.ProjectLab;

public class MeadowApp : App<F7FeatherV2>
{
    private IGraphicsDisplay _display = default!;
    private IProjectLabHardware _projectLab = default!;
    
    public override Task Initialize()
    {
        _projectLab = Meadow.Devices.ProjectLab.Create();
        _display = _projectLab.Display!;
        
        return base.Initialize();
    }
    
    public override Task Run()
    {
        LogService.Log.Trace("Initializing Glade game engine...");
        var glade = new Game();
        glade.Initialize(_display);
        glade.Profiler.IsActive = true;
        InitializeInput(glade.InputManager);

        GladeQaRunner.Run(glade);

        return base.Run();
    }

    private void InitializeInput(InputManager inputManager)
    {
        inputManager.RegisterPushButton(_projectLab.LeftButton!, InputConstants.Up);
        inputManager.RegisterPushButton(_projectLab.UpButton!, InputConstants.Left);
        inputManager.RegisterPushButton(_projectLab.DownButton!, InputConstants.Right);
        inputManager.RegisterPushButton(_projectLab.RightButton!, InputConstants.Down);
    }
}
