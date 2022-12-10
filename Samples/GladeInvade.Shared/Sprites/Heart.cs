using Glade2d.Graphics;

namespace GladeInvade.Shared.Sprites;

public class Heart : Sprite
{
    public Heart()
    {
        CurrentFrame = new Frame(GameConstants.SpriteSheetName, 0, 18, 8, 8);
    }
}