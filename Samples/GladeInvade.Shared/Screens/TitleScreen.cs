using Glade2d;
using Glade2d.Input;
using Glade2d.Screens;
using Glade2d.Services;
using GladeInvade.Shared.Services;
using GladeInvade.Shared.Sprites;
using Meadow.Foundation;

namespace GladeInvade.Shared.Screens;

public class TitleScreen : Screen
{
    private readonly int _screenHeight, _screenWidth;
    private readonly Game _engine = GameService.Instance.GameInstance;
    private readonly GameTitleDisplay _gameTitle;
    
    public TitleScreen()
    {
        _screenHeight = GameService.Instance.GameInstance.Renderer.Height;
        _screenWidth = GameService.Instance.GameInstance.Renderer.Width;

        _gameTitle = new GameTitleDisplay
        {
            X = 5,
            Y = 5,
            VelocityX = 12,
            VelocityY = 10,
        };
            
        AddSprite(_gameTitle);

        LogService.Log.Info("Started title screen.");
    }

    public override void Activity()
    {
        if (_engine.InputManager.GetButtonState(nameof(GameInputs.ActionButton)) == ButtonState.Pressed)
        {
            // restart progression at level 1
            ProgressionService.Instance.Restart();

            // launch the game screen
            GameService.Instance.CurrentScreen = new GameScreen();
        }

        if (_gameTitle.X < 5 || _gameTitle.X + _gameTitle.CurrentFrame.Width > _screenWidth - 5)
        {
            _gameTitle.VelocityX *= -1;
        }

        if (_gameTitle.Y < 5 || _gameTitle.Y + _gameTitle.CurrentFrame.Height > _screenHeight - 5)
        {
            _gameTitle.VelocityY *= -1;
        }
        
        base.Activity();
    }
}