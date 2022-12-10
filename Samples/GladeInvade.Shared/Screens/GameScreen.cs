using Glade2d.Screens;
using Glade2d.Services;
using GladeInvade.Shared.Sprites;

namespace GladeInvade.Shared.Screens;

public class GameScreen : Screen
{
    private const int StartingLives = 3;
    private const int GapBetweenHearts = 5;
    
    private readonly int _screenHeight, _screenWidth;
    private readonly List<Heart> _lives = new();

    public GameScreen()
    {
        _screenHeight = GameService.Instance.GameInstance.Renderer.Height;
        _screenWidth = GameService.Instance.GameInstance.Renderer.Width;
        
        CreateLives();
    }

    private void CreateLives()
    {
        for (var x = 0; x < StartingLives; x++)
        {
            var heart = new Heart();
            heart.X = _screenWidth - (heart.CurrentFrame.Width + GapBetweenHearts) * x;
            heart.Y = 5;

            _lives.Add(heart);
            AddSprite(heart);
        }
    }
}