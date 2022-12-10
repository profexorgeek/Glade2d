using Glade2d.Screens;
using Glade2d.Services;
using GladeInvade.Shared.Entities;
using Meadow.Foundation;

namespace GladeInvade.Shared.Screens;

public class TitleScreen : Screen
{
    private readonly int _screenHeight, _screenWidth;
    private readonly GameTitleDisplay _gameTitle;

    public TitleScreen()
    {
        _screenHeight = GameService.Instance.GameInstance.Renderer.Height;
        _screenWidth = GameService.Instance.GameInstance.Renderer.Width;

        GameService.Instance.GameInstance.Renderer.BackgroundColor = new Color(0, 0, 0);

        _gameTitle = new GameTitleDisplay();
        AddSprite(_gameTitle);
    }
}