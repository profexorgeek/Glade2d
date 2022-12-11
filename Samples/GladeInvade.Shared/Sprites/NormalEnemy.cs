using Glade2d.Graphics;

namespace GladeInvade.Shared.Sprites;

public class NormalEnemy : Sprite
{
    private readonly Frame[] _frames = new Frame[2];
    private int _frameIndex;

    public NormalEnemy(bool isBlue, bool startOutward)
    {
        const int width = 12;
        const int height = 11;
        var startX = new[]
        {
            isBlue ? 32 : 34,
            50
        };

        var startY = isBlue ? 2 : 19;

        _frames[0] = new Frame(GameConstants.SpriteSheetName, startX[0], startY, width, height);
        _frames[1] = new Frame(GameConstants.SpriteSheetName, startX[1], startY, width, height);

        _frameIndex = startOutward ? 0 : 1;
        CurrentFrame = _frames[_frameIndex];
    }

    public void NextFrame()
    {
        _frameIndex++;
        if (_frameIndex >= _frames.Length)
        {
            _frameIndex = 0;
        }

        CurrentFrame = _frames[_frameIndex];
    }
}