using Glade2d.Graphics;
using Glade2d.Screens;
using Glade2d.Services;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        float screenMaxDimension;

        public Cloud(int x = 0, int y = 0, float screenMaxDimension = 256)
        {
            const float MaxVelocity = 5f;

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

            var velocity = (float)(GameService.Instance.Random.NextDouble() * MaxVelocity);

            LogService.Log.Trace($"Setting new cloud velocity to: {velocity}");
            VelocityX = velocity;
            this.screenMaxDimension = screenMaxDimension;
        }

        public override void Activity()
        {
            if(this.X - (this.CurrentFrame.Width / 2f) > MountainSceneScreen.ScreenDimensions)
            {
                Die();
            }
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
    

    public class MountainSceneScreen : Screen
    {
        public const int ScreenDimensions = 60;
        public const int NumberOfClouds = 4;

        List<Cloud> clouds = new List<Cloud>();

        public MountainSceneScreen()
        {
            int startY = ScreenDimensions - TreeLayer.Frame.Height;
            

            for (var x = 0; x < ScreenDimensions; x += TreeLayer.Frame.Width)
            {
                AddSprite(new TreeLayer(x, startY));
                AddSprite(new Foothills(x, startY - 4));
                AddSprite(new NearMountains(x, startY - 8));
                AddSprite(new FarMountains(x, startY - 16));
            }

            // position sun  in top 1/4 of the screen
            var sunPosition = (int)(0.15f * ScreenDimensions);
            AddSprite(new Sun(sunPosition, sunPosition));
        }

        public override void Activity()
        {
            base.Activity();

            if(clouds.Count < NumberOfClouds)
            {
                var cloud = new Cloud(0, 0, ScreenDimensions);
                cloud.X = -cloud.CurrentFrame.Width / 2f;
                cloud.Y = (float)(GameService.Instance.Random.NextDouble() * (0.3f * ScreenDimensions));

                // add to both our scene graph and local list
                AddSprite(cloud);
                clouds.Add(cloud);
            }

            for(var i = clouds.Count - 1; i > -1; i--)
            {
                if (clouds[i].Destroyed)
                {
                    clouds.RemoveAt(i);
                }
            }
        }
    }
}
