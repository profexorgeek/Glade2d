using Glade2d;
using Glade2d.Input;
using Glade2d.Screens;
using Glade2d.Services;
using GladeInvade.Shared.Sprites;
using Meadow.Foundation;

namespace GladeInvade.Shared.Screens;

public class TitleScreen : Screen
{
    private readonly Game _engine = GameService.Instance.GameInstance;
    
    public TitleScreen()
    {
        _engine.Renderer.BackgroundColor = new Color(0, 0, 0);

        var gameTitle = new GameTitleDisplay
        {
            X = 5,
            Y = 5,
        };
            
        AddSprite(gameTitle);
    }

    public override void Activity()
    {
        if (_engine.InputManager.GetButtonState(GameConstants.InputNames.Action) == ButtonState.Pressed)
        {
            GameService.Instance.CurrentScreen = new GameScreen();
        }
        
        base.Activity();
    }
}