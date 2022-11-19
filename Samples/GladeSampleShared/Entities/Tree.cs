using Glade2d.Graphics;

namespace GladeSampleShared.Entities
{
    public class Tree : Sprite
    {
        public const int ChunkWidth = 32;
        public const int ChunkHeight = 32;
        public const int GroundOffset = 26;

        public Tree(float x = 0, float y = 0)
        {
            CurrentFrame = new Frame("spritesheet.bmp", 64, 0, ChunkWidth, ChunkHeight);
            this.X = x;
            this.Y = y;
        }
    }
}
