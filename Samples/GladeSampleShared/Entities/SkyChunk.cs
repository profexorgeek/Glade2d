using Glade2d.Graphics;

namespace GladeSampleShared.Entities
{
    public class SkyChunk : Sprite
    {
        public const int ChunkWidth = 32;
        public const int ChunkHeight = 48;

        public SkyChunk(float x = 0, float y = 0)
        {
            CurrentFrame = new Frame("spritesheet.bmp", 96, 0, ChunkWidth, ChunkHeight);
            this.X = x;
            this.Y = y;
        }
    }
}
