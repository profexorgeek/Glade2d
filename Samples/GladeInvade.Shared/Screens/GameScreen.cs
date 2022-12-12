using Glade2d;
using Glade2d.Input;
using Glade2d.Screens;
using Glade2d.Services;
using GladeInvade.Shared.Sprites;

namespace GladeInvade.Shared.Screens;

public class GameScreen : Screen
{
    private const int StartingLives = 3;
    private const int GapBetweenHearts = 5;
    private const int EnemyColumns = 6;
    private const int EnemyRows = 3;
    
    private readonly int _screenHeight, _screenWidth;
    private readonly Game _engine;
    private readonly List<Heart> _lives = new();
    private readonly Player _player = new();
    private readonly List<NormalEnemy> _enemies = new();
    private readonly TimeSpan _timePerEnemyAnimationFrame = TimeSpan.FromSeconds(1);
    private DateTime _lastEnemyAnimationAt;
    private float _normalEnemyHorizontalVelocity = 15;
    private bool _lastHitLeftBorder = true;

    public GameScreen()
    {
        _engine = GameService.Instance.GameInstance;
        _screenHeight = GameService.Instance.GameInstance.Renderer.Height;
        _screenWidth = GameService.Instance.GameInstance.Renderer.Width;
        
        CreateLivesIndicator();
        CreatePlayer();
        CreateEnemies();
    }

    private void CreateLivesIndicator()
    {
        for (var x = 0; x < StartingLives; x++)
        {
            var heart = new Heart();
            heart.X = _screenWidth - (heart.CurrentFrame.Width + GapBetweenHearts) * (x + 1);
            heart.Y = 5;

            _lives.Add(heart);
            AddSprite(heart);
        }
    }

    private void CreatePlayer()
    {
        _player.X = _screenWidth / 2f;
        _player.Y = _screenHeight - _player.CurrentFrame.Height;
        AddSprite(_player);
    }

    private void CreateEnemies()
    {
        _lastEnemyAnimationAt = new DateTime();
        for (var col = 0; col < EnemyColumns; col++)
        for (var row = 0; row < EnemyRows; row++)
        {
            var enemy = new NormalEnemy(row % 2 == 0, col % 2 == 0);
            enemy.X = col * (2 + enemy.CurrentFrame.Width) + 5;
            enemy.Y = row * (5 + enemy.CurrentFrame.Height);
            enemy.VelocityX = _normalEnemyHorizontalVelocity;
            
            AddSprite(enemy);
            _enemies.Add(enemy);
        }
    }

    public override void Activity()
    {
        ProcessPlayerMovement();
        ProcessEnemyMovement();
    }

    private void ProcessPlayerMovement()
    {
        if (_engine.InputManager.GetButtonState(GameConstants.InputNames.Left) == ButtonState.Down)
        {
            _player.MoveLeft();
        }
        else if (_engine.InputManager.GetButtonState(GameConstants.InputNames.Right) == ButtonState.Down)
        {
            _player.MoveRight();
        }
        else
        {
            _player.StopMoving();
        }

        // Constrain the player to the screen
        if (_player.X < 0)
        {
            _player.X = 0;
        }
        else if (_player.X > _screenWidth - _player.CurrentFrame.Width)
        {
            _player.X = _screenWidth - _player.CurrentFrame.Width;
        }
    }

    private void ProcessEnemyMovement()
    {
        if (DateTime.Now - _lastEnemyAnimationAt >= _timePerEnemyAnimationFrame)
        {
            foreach (var enemy in _enemies)
            {
                enemy.NextFrame();
            }

            _lastEnemyAnimationAt = DateTime.Now;
        }
        
        // If any enemy has hit the border, move them all the opposite way. Only care about the border that's opposite
        // from what they last hit
        var borderHit = false;
        switch (_lastHitLeftBorder)
        {
            case true when _enemies.Any(x => x.X + x.CurrentFrame.Width >= _screenWidth):
                borderHit = true;
                _lastHitLeftBorder = false;
                break;
            
            case false when _enemies.Any(x => x.X <= 0):
                borderHit = true;
                _lastHitLeftBorder = true;
                break;
        }

        if (borderHit)
        {
            _normalEnemyHorizontalVelocity += 5;
            _normalEnemyHorizontalVelocity *= -1;
            foreach (var enemy in _enemies)
            {
                enemy.VelocityX = _normalEnemyHorizontalVelocity;
                enemy.Y += enemy.CurrentFrame.Height;
            }
        }
    }
}