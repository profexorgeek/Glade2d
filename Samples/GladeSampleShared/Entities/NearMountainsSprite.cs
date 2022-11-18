using Glade2d.Graphics;

namespace GladeSampleShared.Entities
{
    public class NearMountainsSprite : Sprite
    {
        public readonly static Frame Frame = new Frame()
        {
            TextureName = "spritesheet.bmp",
            X = 32,
            Y = 0,
            Width = 32,
            Height = 16
        };

        public NearMountainsSprite(int x = 0, int y = 0)
        {
            CurrentFrame = Frame;
            Layer = 30;
            X = x;
            Y = y;
        }
    }
}
