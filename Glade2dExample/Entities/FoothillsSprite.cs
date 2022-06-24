using Glade2d.Graphics;

namespace Glade2dExample.Entities
{
    public class FoothillsSprite : Sprite
    {
        public readonly static Frame Frame = new Frame()
        {
            TextureName = "spritesheet.bmp",
            X = 64,
            Y = 0,
            Width = 32,
            Height = 16
        };

        public FoothillsSprite(int x = 0, int y = 0)
        {
            CurrentFrame = Frame;
            Layer = 40;
            X = x;
            Y = y;
        }
    }
}
