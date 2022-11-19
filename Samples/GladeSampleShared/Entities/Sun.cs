using Glade2d.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GladeSampleShared.Entities
{
    public class Sun : Sprite
    {
        public const int ChunkWidth = 16;
        public const int ChunkHeight = 16;

        public Sun(float x = 0, float y = 0)
        {
            CurrentFrame = new Frame("spritesheet.bmp", 0, 48, ChunkWidth, ChunkHeight);
            this.X = x;
            this.Y = y;
        }

    }
}
