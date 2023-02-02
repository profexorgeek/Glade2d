using Glade2d.Graphics;

namespace GladePlatformer.Shared.Entities
{
    public class MountainChunk : Sprite
    {
        public const int ChunkWidth = 32;
        public const int ChunkHeight = 48;
        
        public MountainChunk(float x = 0, float y = 0)
        {
            CurrentFrame = new Frame("spritesheet.bmp", 0, 0, ChunkWidth, ChunkHeight);
            this.X = x;
            this.Y = y;
        }
    }
}
