using Glade2d.Graphics;

namespace GladeSampleShared.Entities
{
    public class FarMountainsSprite : Sprite
    {
        public readonly static Frame Frame = new Frame()
        {
            TextureName = "spritesheet.bmp",
            X = 0,
            Y = 0,
            Width = 32,
            Height = 16
        };

        public FarMountainsSprite(int x = 0, int y = 0)
        {
            CurrentFrame = Frame;
            Layer = 20;
            X = x;
            Y = y;
        }
    }
}
