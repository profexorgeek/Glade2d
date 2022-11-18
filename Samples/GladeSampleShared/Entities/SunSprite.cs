using Glade2d.Graphics;

namespace GladeSampleShared.Entities
{
    public class SunSprite : Sprite
    {
        public readonly static Frame Frame = new Frame()
        {
            TextureName = "spritesheet.bmp",
            X = 48,
            Y = 16,
            Width = 16,
            Height = 16
        };

        public SunSprite(int x = 0, int y = 0)
        {
            CurrentFrame = Frame;
            Layer = 0;
            X = x;
            Y = y;

        }
    }
}
