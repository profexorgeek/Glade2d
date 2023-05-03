using Glade2d.Graphics;

namespace GladeInvade.Shared.Sprites;



public class Explosion : Sprite
{
    private static readonly Dictionary<EntityColor, Frame> frames = new Dictionary<EntityColor, Frame>
    {
        {EntityColor.Blue, new Frame(GameConstants.SpriteSheetName, 48, 32, 16, 16) },
        {EntityColor.Pink, new Frame(GameConstants.SpriteSheetName, 32, 32, 16, 16) },
        {EntityColor.Red, new Frame(GameConstants.SpriteSheetName, 32, 48, 16, 16) }
    };

    private EntityColor _color = EntityColor.Blue;

    public DateTime CreatedAt { get; }

    public EntityColor Color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            CurrentFrame = frames[_color];
        }
    }

    public Explosion(EntityColor color = EntityColor.Blue)
    {
        CreatedAt = DateTime.Now;
        Color = color;
    }
}