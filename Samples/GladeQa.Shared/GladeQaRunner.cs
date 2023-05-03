using Glade2d;
using Glade2d.Services;
using GladeQa.Shared.Screens;

namespace GladeQa.Shared;

public static class GladeQaRunner
{
    public static void Run(Game engine)
    {
        LogService.Log.Trace("Running game...");
        engine.Profiler.IsActive = false;
        engine.Start(() => new LayerRenderTestScreen());
    }
}