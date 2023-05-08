using Glade2d.Graphics;
using Meadow.Foundation;

namespace GladeInvade.Shared.Sprites;

public class NormalEnemy : Sprite
{
    /// <summary>
    /// Multi-dimensional collection of frames for each entity color
    /// </summary>
    private static readonly Dictionary<EntityColor, List<Frame>> frames = new Dictionary<EntityColor, List<Frame>>
    {
        {EntityColor.Blue, new List<Frame>
            {
                new Frame(GameConstants.SpriteSheetName, 32, 0, 16, 16),
                new Frame(GameConstants.SpriteSheetName, 48, 0, 16, 16)
            }
        },
        {EntityColor.Pink, new List<Frame>
            {
                new Frame(GameConstants.SpriteSheetName, 32, 16, 16, 16),
                new Frame(GameConstants.SpriteSheetName, 48, 16, 16, 16)
            }
        },
    };

    private readonly Frame[] _frames = new Frame[2];
    private int _frameIndex;
    EntityColor _color = EntityColor.Blue;

    public int Row { get; set; }
    public int Column { get; set; }



    /// <summary>
    /// Sets the entity color, updating the sprite's CurrentFrame
    /// </summary>
    public EntityColor Color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            CurrentFrame = frames[Color][_frameIndex];
        }
    }

    /// <summary>
    /// Sets the frame index, updating the sprite's CurrentFrame
    /// Automatically wraps index to be within frame count range
    /// </summary>
    public int FrameIndex
    {
        get
        {
            return _frameIndex;
        }
        set
        {
            _frameIndex = value;
            if(value >= frames[Color].Count)
            {
                _frameIndex = 0;
            }
            if(_frameIndex < 0)
            {
                _frameIndex = frames[Color].Count - 1;
            }

            CurrentFrame = frames[Color][_frameIndex];
        }
    }


    public NormalEnemy(EntityColor color = EntityColor.Blue, bool startOutward = false)
    {
        _frameIndex = startOutward ? 0 : 1;
        Color = color;
        FrameIndex = startOutward ? 0 : 1;
    }

    /// <summary>
    /// Resets this enemy's position based on its Row and Column
    /// </summary>
    public void SetToStartingPosition()
    {
        this.X = Column * (2 + this.CurrentFrame.Width) + 5;
        this.Y = Row * (5 + this.CurrentFrame.Height);
    }

    /// <summary>
    /// Sets the starting position of this enemy based on its row and column
    /// Automatically sets the Row and Column properties on the enemy
    /// </summary>
    /// <param name="row">This enemy's row</param>
    /// <param name="column">This enemy's column</param>
    public void SetToStartingPosition(int row, int column)
    {
        Row = row;
        Column = column;
        SetToStartingPosition();
    }
}