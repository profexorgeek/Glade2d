using Glade2d.Graphics;

namespace GladeSampleShared.Entities
{
    public class GroundChunk : Sprite
    {
        public const int ChunkWidth = 32;
        public const int ChunkHeight = 16;

        public GroundChunk(float x = 0, float y = 0)
        {
            CurrentFrame = new Frame("spritesheet.bmp", 32, 16, ChunkWidth, ChunkHeight);
            this.X = x;
            this.Y = y;
        }

    }
}
