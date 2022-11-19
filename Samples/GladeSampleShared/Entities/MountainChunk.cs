using Glade2d.Graphics;
using Glade2d.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace GladeSampleShared.Entities
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
