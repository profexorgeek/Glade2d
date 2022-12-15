using Glade2d.Graphics;

namespace GladeInvade.Shared.Sprites;

public class Explosion : Sprite
{
    public DateTime CreatedAt { get; }
    public Explosion(bool isBlue)
    {
        CreatedAt = DateTime.Now;
        CurrentFrame = isBlue
            ? new Frame(GameConstants.SpriteSheetName, 50, 33, 12, 12)
            : new Frame(GameConstants.SpriteSheetName, 35, 34, 12, 12);
    }
}