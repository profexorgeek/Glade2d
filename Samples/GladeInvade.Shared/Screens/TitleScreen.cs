using Glade2d.Screens;
using Glade2d.Services;
using GladeInvade.Shared.Sprites;
using Meadow.Foundation;

namespace GladeInvade.Shared.Screens;

public class TitleScreen : Screen
{
    private readonly DateTime _startedAt;
    private readonly TimeSpan _timeBeforeScreenChange = TimeSpan.FromSeconds(10);
    
    public TitleScreen()
    {
        GameService.Instance.GameInstance.Renderer.BackgroundColor = new Color(0, 0, 0);

        var gameTitle = new GameTitleDisplay
        {
            X = 5,
            Y = 5,
        };
            
        AddSprite(gameTitle);
        
        _startedAt = DateTime.Now;
    }

    public override void Activity()
    {
        if (DateTime.Now - _startedAt > _timeBeforeScreenChange)
        {
            GameService.Instance.CurrentScreen = new GameScreen();
        }
        
        base.Activity();
    }
}