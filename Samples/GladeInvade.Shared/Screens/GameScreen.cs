using Glade2d;
using Glade2d.Input;
using Glade2d.Screens;
using Glade2d.Services;
using GladeInvade.Shared.Services;
using GladeInvade.Shared.Sprites;

namespace GladeInvade.Shared.Screens;

public class GameScreen : Screen
{
    private const int StartingLives = 3;
    private const int GapBetweenHearts = 5;
    private const int EnemyColumns = 6;
    private const int EnemyRows = 3;
    private const int PlayerShotVelocity = 35;
    
    private readonly int _screenHeight, _screenWidth;
    private readonly Game _engine;
    private readonly List<Heart> _lives = new();
    private readonly Player _player = new();
    private readonly List<NormalEnemy> _enemies = new();
    private readonly List<PlayerShot> _playerShots = new();
    private readonly List<Explosion> _explosions = new();
    private readonly TimeSpan _timePerEnemyAnimationFrame = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _explosionLifetime = TimeSpan.FromSeconds(0.5);
    private DateTime _lastEnemyAnimationAt;
    private float _normalEnemyHorizontalVelocity = 0;
    private bool _lastHitLeftBorder = true;

    public GameScreen()
    {
        _engine = GameService.Instance.GameInstance;
        _screenHeight = GameService.Instance.GameInstance.Renderer.Height;
        _screenWidth = GameService.Instance.GameInstance.Renderer.Width;
        _normalEnemyHorizontalVelocity = ProgressionService.Instance.CurrentEnemySpeed;
        
        CreateLivesIndicator();
        CreatePlayer();
        CreateEnemies();
    }

    /// <summary>
    /// Performs all frame-based activity
    /// </summary>
    public override void Activity()
    {
        ProcessPlayerMovement();
        ProcessEnemyMovement();
        ProcessPlayerShots();
        ProcessExplosions();
        ProcessEndgameState();
    }


    /// <summary>
    /// Sets up the display of hearts that shows how many lives players have
    /// </summary>
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

    /// <summary>
    /// Creates the player object
    /// </summary>
    private void CreatePlayer()
    {
        _player.X = _screenWidth / 2f;
        _player.Y = _screenHeight - _player.CurrentFrame.Height;
        AddSprite(_player);
    }

    /// <summary>
    /// Creates the rows of enemies at the beginning of the game.
    /// </summary>
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

    
    /// <summary>
    /// Frame Time: Performs all player movement logic in reaction to player input
    /// </summary>
    private void ProcessPlayerMovement()
    {
        if (_engine.InputManager.GetButtonState(nameof(GameInputs.LeftButton)) == ButtonState.Down)
        {
            _player.MoveLeft();
        }
        else if (_engine.InputManager.GetButtonState(nameof(GameInputs.RightButton)) == ButtonState.Down)
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

    /// <summary>
    /// Frame Time: Processes enemy movement from side to side and advancing down the screen
    /// </summary>
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

    /// <summary>
    /// Frame time: checks shots versus enemies and does enemy destruction logic
    /// </summary>
    private void ProcessPlayerShots()
    {
        if (_engine.InputManager.GetButtonState(nameof(GameInputs.ActionButton)) == ButtonState.Pressed)
        {
            var shot = new PlayerShot
            {
                X = _player.X + _player.CurrentFrame.Width / 2f,
                Y = _screenHeight - _player.CurrentFrame.Height,
                VelocityY = -PlayerShotVelocity,
            };

            _playerShots.Add(shot);
            AddSprite(shot);
        }

        // Check if any shot has either left the screen, or collided with an enemy
        for (var shotIndex = _playerShots.Count - 1; shotIndex >= 0; shotIndex--)
        {
            var shotLeft = _playerShots[shotIndex].X;
            var shotRight = _playerShots[shotIndex].X + _playerShots[shotIndex].CurrentFrame.Width;
            var shotTop = _playerShots[shotIndex].Y;
            var shotBottom = _playerShots[shotIndex].Y + _playerShots[shotIndex].CurrentFrame.Height;
            
            if (shotBottom <= 0)
            {
                // Shot is now off screen
                RemoveSprite(_playerShots[shotIndex]);
                _playerShots.RemoveAt(shotIndex);
                
                continue;
            }

            // Shot is on screen. Has it hit any enemies?
            for (var enemyIndex = _enemies.Count - 1; enemyIndex >= 0; enemyIndex--)
            {
                var enemy = _enemies[enemyIndex];
                var enemyLeft = enemy.X;
                var enemyRight = enemy.X + enemy.CurrentFrame.Width;
                var enemyTop = enemy.Y;
                var enemyBottom = enemy.Y + enemy.CurrentFrame.Height;

                var collisionOccurred = shotRight >= enemyLeft &&
                                        shotLeft <= enemyRight &&
                                        shotTop <= enemyBottom &&
                                        shotBottom >= enemyTop;

                if (collisionOccurred)
                {
                    // remove shots and enemy
                    RemoveSprite(enemy);
                    RemoveSprite(_playerShots[shotIndex]);
                    _enemies.RemoveAt(enemyIndex);
                    _playerShots.RemoveAt(shotIndex);

                    // create explosion effect
                    var explosion = new Explosion(enemy.IsBlue)
                    {
                        X = enemy.X,
                        Y = enemy.Y,
                    };
                    _explosions.Add(explosion);
                    AddSprite(explosion);

                    // award points
                    ProgressionService.Instance.AwardEnemyKill();

                    // we have destroyed the shot and enemy and removed
                    // them from the collection so we need to break out
                    // of this loop
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Processes any active explosions and removes them when their
    /// lifespan has expired
    /// </summary>
    private void ProcessExplosions()
    {
        for (var index = _explosions.Count - 1; index >= 0; index--)
        {
            if (DateTime.Now - _explosions[index].CreatedAt >= _explosionLifetime)
            {
                RemoveSprite(_explosions[index]);
                _explosions.RemoveAt(index);
            }
        }
    }

    private void ProcessEndgameState()
    {
        // TODO: show congratulations message?

        if(_enemies.Count == 0)
        {
            ProgressionService.Instance.IncreaseDifficultyLevel();
            GameService.Instance.CurrentScreen = new GameScreen();
        }
    }
}