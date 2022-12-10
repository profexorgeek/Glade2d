using Glade2d;
using Glade2d.Services;
using GladeInvade.Shared.Screens;

namespace GladeInvade.Shared;

public class GladeInvadeGame
{
    private readonly Game _engine;

    public GladeInvadeGame(Game engine)
    {
        _engine = engine;
        // _engine.Renderer.ShowPerf = true;

        LogService.Log.Trace("Running game...");
        _engine.Start(new TitleScreen());
    }
}