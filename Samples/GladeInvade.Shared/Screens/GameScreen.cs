using Glade2d;
using Glade2d.Graphics;
using Glade2d.Input;
using Glade2d.Screens;
using Glade2d.Services;
using GladeInvade.Shared.Services;
using GladeInvade.Shared.Sprites;
using Meadow.Foundation.Graphics;

namespace GladeInvade.Shared.Screens;

public class GameScreen : Screen
{
    enum GameScreenState
    {
        Intro,
        Gameplay,
        Outro,
    };

    enum EndgameResult
    {
        None,
        LivesEmpty,
        EnemyEscaped,
        CompletedLevel,
    }


    private const int GapBetweenHearts = 5;
    private const int EnemyColumns = 6;
    private const int EnemyRows = 3;
    private const int PlayerShotVelocity = 35;
    private const float ToastDisplaySeconds = 3f;
    
    private readonly int _screenHeight, _screenWidth;
    private readonly Game _game;
    private readonly List<Heart> _lives = new();
    private readonly Player _player = new();
    private readonly List<NormalEnemy> _enemies = new();
    private readonly List<PlayerShot> _playerShots = new();
    private readonly List<Explosion> _explosions = new();
    private readonly TimeSpan _timePerEnemyAnimationFrame = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _explosionLifetime = TimeSpan.FromSeconds(0.5);
    private DateTime _lastEnemyAnimationAt;
    private float _normalEnemyHorizontalVelocity;
    private bool _lastHitLeftBorder = true;
    private bool _enemyEscaped;
    private ILayer _textLayer = null!, _toastLayer = null!;
    private GameScreenState _screenState;
    private IFont _toastFont;
    private double _timeToNextStateChange;

    public GameScreen()
    {
        _game = GameService.Instance.GameInstance;
        _screenHeight = _game.Renderer.Height;
        _screenWidth = _game.Renderer.Width;
        _normalEnemyHorizontalVelocity = ProgressionService.Instance.CurrentEnemySpeed;
        _toastFont = new Font8x12();

        CreateTextLayers();
        CreateLivesIndicator();
        CreatePlayer();
        CreateEnemies();

        StartIntro();

    }

    /// <summary>
    /// Performs all frame-based activity based on the
    /// current game state
    /// </summary>
    public override void Activity()
    {
        switch(_screenState)
        {
            case GameScreenState.Intro:
                DoIntroState();
                break;
            case GameScreenState.Gameplay:
                DoGameplayState();
                break;
            case GameScreenState.Outro:
                DoOutroState();
                break;
        }
    }

    /// <summary>
    /// Starts the Intro state, update the toast text
    /// and moving the screen into the Intro state
    /// </summary>
    void StartIntro()
    {
        _timeToNextStateChange = ToastDisplaySeconds;
        UpdateToastText($"Level {ProgressionService.Instance.CurrentLevel}");
        IsPaused = true;
        _screenState = GameScreenState.Intro;
    }
    /// <summary>
    /// Does intro state activities, counting down time to
    /// game start
    /// </summary>
    void DoIntroState()
    {
        _timeToNextStateChange -= GameService.Instance.Time.FrameDelta;
        if(_timeToNextStateChange <= 0)
        {
            StartGameplay();
        }
    }
    /// <summary>
    /// Starts the gameplay state, hiding toast text
    /// and unpausing the game
    /// </summary>
    void StartGameplay()
    {
        HideToastText();
        IsPaused = false;
        _screenState = GameScreenState.Gameplay;
    }
    /// <summary>
    /// Does the gameplay state activities, managing movement,
    /// collision, and checking for an endgame condition
    /// </summary>
    void DoGameplayState()
    {
        DoPlayerInput();
        DoEnemyMovement();
        DoExplosions();
        DoEnemyCollisions();
        
        // react to endgame condition
        switch (CheckEndgame())
        {
            case EndgameResult.None:
                // noop, game continues
                break;
            case EndgameResult.EnemyEscaped:
                StartOutro("Enemy Escaped!");
                break;
            case EndgameResult.LivesEmpty:
                StartOutro("No More Lives!");
                break;
            case EndgameResult.CompletedLevel:
                StartOutro($"Cleared {ProgressionService.Instance.CurrentLevel}");
                break;
        }
    }
    /// <summary>
    /// Starts the outro state, showing the provided
    /// toast message and pausing the game
    /// </summary>
    /// <param name="message">The toast message to show, should explain
    /// why the current screen is ending</param>
    void StartOutro(string message)
    {
        _timeToNextStateChange = ToastDisplaySeconds;
        UpdateToastText(message);
        IsPaused = true;
        _screenState = GameScreenState.Outro;
    }
    /// <summary>
    /// Performs the outro activity, counting down time until
    /// the screen should transition and figuring out where the
    /// screen should transition to
    /// </summary>
    void DoOutroState()
    {
        _timeToNextStateChange -= GameService.Instance.Time.FrameDelta;
        if(_timeToNextStateChange <= 0)
        {
            var endgameType = CheckEndgame();

            if(endgameType == EndgameResult.CompletedLevel)
            {
                ProgressionService.Instance.IncreaseDifficultyLevel();
                _game.TransitionToScreen(() => new GameScreen());
            }
            else
            {
                _game.TransitionToScreen(() => new EndgameScreen());
            }
        }
    }


    /// <summary>
    /// Frame Time: Performs all player movement logic in reaction to player input,
    /// additionally it ensures all shots that have left the screen are destroyed
    /// </summary>
    private void DoPlayerInput()
    {
        // do movement input
        if (_game.InputManager.GetButtonState(nameof(GameInputs.LeftButton)) == ButtonState.Down)
        {
            _player.MoveLeft();
        }
        else if (_game.InputManager.GetButtonState(nameof(GameInputs.RightButton)) == ButtonState.Down)
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
        if (_game.InputManager.GetButtonState(nameof(GameInputs.ActionButton)) == ButtonState.Pressed)
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
        for (var i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];

            if (CollideEnemyVsShots(enemy))
            {
                // enemy hit shot, no more checks for this enemy
                continue;
            }

            if (CollideEnemyVsPlayer(enemy))
            {
                ResetEnemies();
                break;
            }

            if (CheckEnemyEscaped(enemy))
            {
                _enemyEscaped = true;
            }
        }
    }


    /// <summary>
    /// Creates the text layers
    /// </summary>
    private void CreateTextLayers()
    {
        // main text layer with score and level
        _textLayer = GameService.Instance.GameInstance.Renderer.CreateLayer(new Dimensions(_screenWidth, _screenHeight));
        _textLayer.BackgroundColor = GameConstants.BackgroundColor;
        _textLayer.DrawLayerWithTransparency = false;
        UpdateScoreText();
        _game.LayerManager.AddLayer(_textLayer, -1);

        _toastLayer = GameService.Instance.GameInstance.Renderer.CreateLayer(new Dimensions(_screenWidth, _toastFont.Height * 2));
        _toastLayer.BackgroundColor = GameConstants.WhiteTextColor;
        _toastLayer.CameraOffset = new Point(0, (_screenHeight / 2) - (_toastLayer.Height / 2));
        _toastLayer.DrawLayerWithTransparency = false;
        // NOTE: we don't add the layer to managers here because we may
        // not want it to be shown yet
    }
    /// <summary>
    /// Sets up the display of hearts that shows how many lives players have
    /// </summary>
    private void CreateLivesIndicator()
    {
        for (var x = 0; x < ProgressionService.Instance.Lives; x++)
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
            var offset = col % 2 == 0;
            var enemy = new NormalEnemy(color, offset);

            enemy.SetToStartingPosition(row, col);
            enemy.VelocityX = _normalEnemyHorizontalVelocity;
            
            AddSprite(enemy);
            _enemies.Add(enemy);
        }
    }
    /// <summary>
    /// Resets any remaining enemies to their starting position
    /// </summary>
    private void ResetEnemies()
    {
        for(var i = 0; i < _enemies.Count; i++)
        {
            _enemies[i].SetToStartingPosition();
        }
    }


    bool CollideEnemyVsShots(NormalEnemy enemy)
    {
        bool didCollide = false;
        for (var j = _playerShots.Count - 1; j >= 0; j--)
        {
            var shot = _playerShots[j];
            if (shot.IsOverlapping(enemy))
            {
                DestroyShot(shot);
                DestroyEnemy(enemy);
                ProgressionService.Instance.AwardEnemyKill();
                UpdateScoreText();
                didCollide = true;
                break;
            }
        }
        return didCollide;
    }
    bool CollideEnemyVsPlayer(NormalEnemy enemy)
    {
        bool didCollide = false;
        if (_player.IsOverlapping(enemy))
        {
            // remove heart sprite and life counter
            var heart = _lives[0];
            _lives.RemoveAt(0);
            RemoveSprite(heart);
            ProgressionService.Instance.RemoveLife();

            // create an explosion to draw attention to the life loss
            CreateExplosionAtPoint(heart.X, heart.Y, EntityColor.Red);

            DestroyEnemy(enemy);

            didCollide = true;
        }
        return didCollide;
    }
    bool CheckEnemyEscaped(NormalEnemy enemy)
    {
        bool didEscape = false;
        if (enemy.Bottom >= _screenHeight)
        {
            didEscape = true;
        }
        return didEscape;
    }
    void UpdateScoreText()
    {
        _textLayer.Clear();
        _textLayer.DrawText(
            position: new Point(4, 4),
            text: ProgressionService.Instance.Score.ToString("N0"),
            color: GameConstants.WhiteTextColor);

        _textLayer.DrawText(
            position: new Point(_screenWidth / 2, 4),
            text: ProgressionService.Instance.CurrentLevel.ToString(),
            color: GameConstants.WhiteTextColor);
    }
    void UpdateToastText(string text)
    {
        _toastLayer.Clear();
        var padding = _toastFont.Height / 2;
        var position = new Point(
            (_screenWidth / 2) - (text.Length * _toastFont.Width / 2),
            padding);
        _toastLayer.DrawText(
            position: position,
            text: text,
            font: _toastFont,
            color: GameConstants.BackgroundColor);

        if(_game.LayerManager.ContainsLayer(_toastLayer) == false)
        {
            _game.LayerManager.AddLayer(_toastLayer, 5);
        }
        
    }
    void HideToastText()
    {
        if(_game.LayerManager.ContainsLayer(_toastLayer))
        {
            _game.LayerManager.RemoveLayer(_toastLayer);
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
            _playerShots.RemoveAt(index);
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
    private EndgameResult CheckEndgame()
    {
        if (ProgressionService.Instance.Lives == 0)
        {
            LogService.Log.Info("Player lost because they ran out of lives");
            return EndgameResult.LivesEmpty;
            //_game.TransitionToScreen(() => new EndgameScreen());
        }

        if(_enemyEscaped)
        {
            LogService.Log.Info("Player lost because an enemy escaped.");
            return EndgameResult.EnemyEscaped;
            //_game.TransitionToScreen(() => new EndgameScreen());
        }

        if (_enemies.Count == 0)
        {
            LogService.Log.Info($"Player completed level {ProgressionService.Instance.CurrentLevel}.");
            return EndgameResult.CompletedLevel;
            //ProgressionService.Instance.IncreaseDifficultyLevel();
            //_game.TransitionToScreen(() => new GameScreen());
        }

        return EndgameResult.None;
    }
}