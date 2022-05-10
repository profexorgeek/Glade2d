using Glade2d.Graphics;
using Glade2d.Screens;
using Glade2d.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Glade2d.Examples
{
    public class Sun : Sprite
    {
        public readonly static Frame Frame = new Frame()
        {
            TextureName = "spritesheet.bmp",
            X = 48,
            Y = 16,
            Width = 16,
            Height = 16
        };

        public Sun(int x = 0, int y = 0)
        {
            CurrentFrame = Frame;
            Layer = 0;
            X = x;
            Y = y;
        }
    }

    public class Cloud : Sprite
    {
        public Cloud(int x = 0, int y = 0)
        {
            if (GameService.Instance.Random.NextDouble() < 0.5f)
            {
                CurrentFrame = new Frame()
                {
                    TextureName = "spritesheet.bmp",
                    X = 0,
                    Y = 16,
                    Width = 32,
                    Height = 16
                };
            }
            else
            {
                CurrentFrame = new Frame()
                {
                    TextureName = "spritesheet.bmp",
                    X = 32,
                    Y = 16,
                    Width = 16,
                    Height = 16
                };
            }

            Layer = 10;
            X = x;
            Y = y;
        }
    }

    public class FarMountains : Sprite
    {
        public readonly static Frame Frame = new Frame()
        {
            TextureName = "spritesheet.bmp",
            X = 0,
            Y = 0,
            Width = 32,
            Height = 16
        };

        public FarMountains(int x = 0, int y = 0)
        {
            CurrentFrame = Frame;
            Layer = 20;
            X = x;
            Y = y;
        }
    }

    public class NearMountains : Sprite
    {
        public readonly static Frame Frame = new Frame()
        {
            TextureName = "spritesheet.bmp",
            X = 32,
            Y = 0,
            Width = 32,
            Height = 16
        };

        public NearMountains(int x = 0, int y = 0)
        {
            CurrentFrame = Frame;
            Layer = 30;
            X = x;
            Y = y;
        }
    }

    public class Foothills : Sprite
    {
        public readonly static Frame Frame = new Frame()
        {
            TextureName = "spritesheet.bmp",
            X = 64,
            Y = 0,
            Width = 32,
            Height = 16
        };

        public Foothills(int x = 0, int y = 0)
        {
            CurrentFrame = Frame;
            Layer = 40;
            X = x;
            Y = y;
        }
    }

    public class TreeLayer : Sprite
    {
        public readonly static Frame Frame = new Frame()
        {
            TextureName = "spritesheet.bmp",
            X = 96,
            Y = 0,
            Width = 32,
            Height = 16
        };

        public TreeLayer(int x = 0, int y = 0)
        {
            CurrentFrame = Frame;
            Layer = 50;
            X = x;
            Y = y;
        }
    }
    

    public class Glade2dScreen : Screen
    {
        const int screenDimensions = 120;

        public Glade2dScreen()
        {
            int startY = screenDimensions - TreeLayer.Frame.Height;

            for (var x = 0; x < screenDimensions; x += TreeLayer.Frame.Width)
            {
                AddSprite(new TreeLayer(x, startY));
                AddSprite(new Foothills(x, startY - 4));
                AddSprite(new NearMountains(x, startY - 8));
                AddSprite(new FarMountains(x, startY - 16));
            }

            AddSprite(new Sun(8, 8));
            AddSprite(new Cloud(12, 12));
            AddSprite(new Cloud(44, 32));
        }
    }
}
