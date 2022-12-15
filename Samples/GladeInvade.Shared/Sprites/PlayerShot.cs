using Glade2d.Graphics;

namespace GladeInvade.Shared.Sprites;

public class PlayerShot : Sprite
{
    public PlayerShot()
    {
        CurrentFrame = new Frame(GameConstants.SpriteSheetName, 11, 18, 2, 11);
    }
}