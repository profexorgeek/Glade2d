using Glade2d.Graphics;

namespace GladeInvade.Shared.Sprites;

public class Heart : Sprite
{
    public Heart()
    {
        CurrentFrame = new Frame(GameConstants.SpriteSheetName, 0, 17, 8, 8);
    }
}