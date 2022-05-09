using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Glade2d.Graphics
{
    public class Renderer
    {
        MicroGraphics graphicsDriver;
        Dictionary<string, IDisplayBuffer> textures = new Dictionary<string, IDisplayBuffer>();

        public Renderer(MicroGraphics graphics)
        {
            graphicsDriver = graphics;
        }

    }
}