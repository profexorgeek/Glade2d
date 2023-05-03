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
        DoPlayerInput();
        DoEnemyMovement();
        DoExplosions();
        DoEnemyCollisions();

        CheckLevelComplete();
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
            var color = row % 2 == 0 ? EntityColor.Blue : EntityColor.Pink;
            var offset = col % 2 == 0 ? true : false;
            var enemy = new NormalEnemy(color, offset);

            enemy.X = col * (2 + enemy.CurrentFrame.Width) + 5;
            enemy.Y = row * (5 + enemy.CurrentFrame.Height);
            enemy.VelocityX = _normalEnemyHorizontalVelocity;
            
            AddSprite(enemy);
            _enemies.Add(enemy);
        }
    }

    

    /// <summary>
    /// Frame Time: Performs all player movement logic in reaction to player input,
    /// additionally it ensures all shots that have left the screen are destroyed
    /// </summary>
    private void DoPlayerInput()
    {
        // do movement input
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

        // do shot input
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

        // Destroy shots that are out of screen
        for (var shotIndex = _playerShots.Count - 1; shotIndex >= 0; shotIndex--)
        {
            var shot = _playerShots[shotIndex];
            if (shot.Bottom <= 0)
            {
                // Shot is now off screen
                RemoveSprite(_playerShots[shotIndex]);
                _playerShots.RemoveAt(shotIndex);

                continue;
            }

        }

    }

    /// <summary>
    /// Frame Time: Processes enemy movement from side to side and advancing down the screen
    /// </summary>
    private void DoEnemyMovement()
    {
        if (DateTime.Now - _lastEnemyAnimationAt >= _timePerEnemyAnimationFrame)
        {
            foreach (var enemy in _enemies)
            {
                enemy.FrameIndex++;
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
                enemy.Y += ProgressionService.EnemyVerticalMovementAmount;
            }
        }
    }

    /// <summary>
    /// Processes any active explosions and removes them when their
    /// lifespan has expired
    /// </summary>
    private void DoExplosions()
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
    /// <summary>
    /// Collides enemies with shots, the player, and checks if they have escaped the screen,
    /// causing a game-over event
    /// </summary>
    private void DoEnemyCollisions()
    {
        bool didPlayerLoseThisFrame = false;

        for (var i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];

            // did enemy collide with shot?
            bool enemyWasDestroyedByShot = false;
            for (var j = _playerShots.Count - 1; j >= 0; j--)
            {
                var shot = _playerShots[j];
                if(shot.IsOverlapping(enemy))
                {
                    DestroyShot(shot);
                    DestroyEnemy(enemy);
                    ProgressionService.Instance.AwardEnemyKill();
                    enemyWasDestroyedByShot = true;
                    break;
                }
            }
            if(enemyWasDestroyedByShot)
            {
                continue;
            }

            // did enemy collide with player?
            if (_player.IsOverlapping(enemy))
            {
                var heart = _lives[0];
                _lives.RemoveAt(0);
                RemoveSprite(heart);

                // create an explosion to draw attention to the life loss
                CreateExplosionAtPoint(heart.X, heart.Y, EntityColor.Red);

                DestroyEnemy(enemy);

                if(_lives.Count == 0)
                {
                    LogService.Log.Info("Player lost because they ran out of lives");
                    didPlayerLoseThisFrame = true;
                    break;
                }
            }

            // did enemy leave screen
            if (enemy.Bottom >= _screenHeight)
            {
                LogService.Log.Info("Player lost because an enemy escaped.");
                didPlayerLoseThisFrame = true;
                break;
            }
        }

        if(didPlayerLoseThisFrame)
        {
            // TODO: this should go to the endgame screen when that
            // exists!
            GameService.Instance.CurrentScreen = new TitleScreen();
        }
    }



    private void DestroyShot(PlayerShot shot)
    {
        var index = _playerShots.IndexOf(shot);

        if (index < 0)
        {
            LogService.Log.Error("Tried to remove shot that did not exist in collection!");
        }
        else
        {
            _enemies.RemoveAt(index);
            RemoveSprite(shot);
        }
    }
    private void DestroyEnemy(NormalEnemy enemy, bool createExplosion = true)
    {
        var index = _enemies.IndexOf(enemy);

        if(index < 0)
        {
            LogService.Log.Error("Tried to remove enemy that did not exist in collection!");
        }
        else
        {
            _enemies.RemoveAt(index);
            RemoveSprite(enemy);

            if(createExplosion)
            {
                CreateExplosionAtPoint(enemy.X, enemy.Y, enemy.Color);
            }
        }
    }
    private void CreateExplosionAtPoint(float x, float y, EntityColor color = EntityColor.Blue)
    {
        var explosion = new Explosion(color)
        {
            X = x,
            Y = y,
        };
        _explosions.Add(explosion);
        AddSprite(explosion);
    }
    private void CheckLevelComplete()
    {
        // TODO: show congratulations message?

        if(_enemies.Count == 0)
        {
            ProgressionService.Instance.IncreaseDifficultyLevel();
            GameService.Instance.CurrentScreen = new GameScreen();
        }
    }
}