using Glade2d.Input;
using Glade2d.Screens;
using Glade2d.Services;
using GladePlatformer.Shared.Entities;
using Meadow.Foundation;

namespace GladePlatformer.Shared.Screens;

public class LevelScreen : Screen, IDisposable
{
    private const float PlayerSpeed = 30;
    private const float Gravity = 10f;
    private const float JumpAcceleration = -50f;
    
    private readonly int _screenWidth;
    private readonly Color _backgroundColor = new(57, 120, 168);
    private readonly Player _player;
    private readonly LevelHandler _levelHandler;
    private float _playerPositionX; 
    private float _playerVelocityX; 

    public LevelScreen()
    {
        // set screen dimensions for easy reference
        _screenWidth = GameService.Instance.GameInstance.Renderer.Width;
        var screenHeight = GameService.Instance.GameInstance.Renderer.Height;
        
        // Set background color
        GameService.Instance.GameInstance.Renderer.BackgroundColor = _backgroundColor;
        
        CreateSun();

        var levelData = new LevelHandler.LevelData(new byte[] { 1, 2, 3, 2, 1, 2, 2, 1, 3, 1, 3 });
        _levelHandler = new LevelHandler(levelData);

        _player = new Player
        {
            X = _screenWidth / 2.0f
        };
        
        _player.Y = screenHeight - new GroundChunk().CurrentFrame.Height - _player.CurrentFrame.Height;
        AddSprite(_player);
    }

    public override void Activity()
    {
        // We don't want to move the player horizontally in screen space, because the
        // player is always the center of the view, therefore we can't use the
        // standard `Sprite.X` and `Sprite.VelocityX` properties. So instead we need
        // to track the position and velocity manually so we can fake movement by
        // moving the layers.
        var frameDelta = (float) GameService.Instance.Time.FrameDelta;
        _playerPositionX += _playerVelocityX * frameDelta;
        _playerVelocityX = 0;

        var playerPositionY = _player.Y;
        var playerVelocityY = _player.VelocityY;
        
        _levelHandler.Update(ref _playerPositionX, ref playerPositionY, ref playerVelocityY);
        _player.Y = playerPositionY;
        _player.VelocityY = playerVelocityY;

        var inputManager = GameService.Instance.GameInstance.InputManager;
        if (inputManager.GetButtonState(GameConstants.InputNames.Right) == ButtonState.Down)
        {
            _playerVelocityX = PlayerSpeed;
        }
        else if (inputManager.GetButtonState(GameConstants.InputNames.Left) == ButtonState.Down)
        {
            _playerVelocityX = -PlayerSpeed;
        }
        
        _player.VelocityY += Gravity;
        if (inputManager.GetButtonState(GameConstants.InputNames.Up) == ButtonState.Pressed)
        {
            _player.VelocityY += JumpAcceleration;
        }
    }

    /// <summary>
    /// Add a sun in the top left of the screen
    /// </summary>
    private void CreateSun()
    {
        var sun = new Sun(_screenWidth - 8 - Sun.ChunkWidth, 8)
        {
            Layer = 0
        };
        
        AddSprite(sun);
    }

    public void Dispose()
    {
        _levelHandler.Dispose();
    }
}