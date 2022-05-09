using Glade2d.Graphics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Glade2d.Screens
{
    public class Screen
    {
        public List<Sprite> Sprites { get; set; } = new List<Sprite>();

        public Screen() { }

        public void Update()
        {
            for(var i = Sprites.Count - 1; i > -1; i--)
            {
                Sprites[i].Update();
            }
        }
    }
}
