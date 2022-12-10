using Glade2d.Graphics;

namespace GladeInvade.Shared.Sprites;

public class GameTitleDisplay : Sprite
{
    public GameTitleDisplay()
    {
        CurrentFrame = new Frame(GameConstants.SpriteSheetName, 4, 0, 25, 16);
    }
}