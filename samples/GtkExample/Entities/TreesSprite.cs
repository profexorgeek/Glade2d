using Glade2d.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Glade2dExample.Entities
{
    public class TreesSprite : Sprite
    {
        public readonly static Frame Frame = new Frame()
        {
            TextureName = "spritesheet.bmp",
            X = 96,
            Y = 0,
            Width = 32,
            Height = 16
        };

        public TreesSprite(int x = 0, int y = 0)
        {
            CurrentFrame = Frame;
            Layer = 50;
            X = x;
            Y = y;
        }
    }
}
