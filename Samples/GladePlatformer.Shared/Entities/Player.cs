using Glade2d.Graphics;
using Glade2d.Input;
using Glade2d.Services;

namespace GladePlatformer.Shared.Entities;

public class Player : Sprite
{
    public const int FrameWidth = 10;
    public const int FrameHeight = 11;
    private const double SecondsPerWalkFrame = 0.1f;

    private readonly Frame _standingFrame;
    private readonly Frame[] _walkFrames;
    private int _walkFrameIndex;
    private bool _isWalking;
    private double _secondsSinceLastAnimationChange;

    public Player()
    {
        _standingFrame = new Frame(GameConstants.SpriteSheetName, 18, 84, FrameWidth, FrameHeight);
        _walkFrames = new[]
        {
            _standingFrame,
            new Frame(GameConstants.SpriteSheetName, 2, 84, FrameWidth, FrameHeight),
        };

        _walkFrameIndex = 0;
        CurrentFrame = _standingFrame;
    }

    public override void Activity()
    {
        var inputManager = GameService.Instance.GameInstance.InputManager;
        var wasWalking = _isWalking;
        _isWalking = inputManager.GetButtonState(GameConstants.InputNames.Left) == ButtonState.Down ||
                     inputManager.GetButtonState(GameConstants.InputNames.Right) == ButtonState.Down;

        if (_isWalking)
        {
            _secondsSinceLastAnimationChange += GameService.Instance.Time.FrameDelta;
            if (_secondsSinceLastAnimationChange >= SecondsPerWalkFrame)
            {
                _walkFrameIndex++;
                if (_walkFrameIndex >= _walkFrames.Length)
                {
                    _walkFrameIndex = 0;
                }

                CurrentFrame = _walkFrames[_walkFrameIndex];
                _secondsSinceLastAnimationChange = 0;
            }
        }
        else if (wasWalking)
        {
            _secondsSinceLastAnimationChange = 0;
            CurrentFrame = _standingFrame;
        }
    }
}