using Glade2d.Graphics;

namespace GladeSampleShared.Entities
{
    public class Tree : Sprite
    {
        public Tree()
        {
            CurrentFrame = new Frame("spritesheet.bmp", 69, 3, 23, 26);
        }
    }
}
