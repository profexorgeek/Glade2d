using Glade2d.Graphics;

namespace GladePlatformer.Shared.Entities
{
    public class GroundChunk : Sprite
    {
        public const int ChunkWidth = 32;
        public const int ChunkHeight = 15;

        public GroundChunk(float x = 0, float y = 0)
        {
            CurrentFrame = new Frame("spritesheet.bmp", 32, 17, ChunkWidth, ChunkHeight);
            this.X = x;
            this.Y = y;
        }

    }
}
