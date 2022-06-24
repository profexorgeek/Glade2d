using Glade2d.Screens;
using Glade2d.Services;
using Glade2dExample.Entities;
using Meadow.Foundation;
using System.Collections.Generic;

namespace Glade2dExample.Screens
{
    public class MountainSceneScreen : Screen
    {
        public const int ScreenDimensions = 60;
        public const int NumberOfClouds = 4;

        List<CloudSprite> clouds = new List<CloudSprite>();

        public MountainSceneScreen()
        {
            int startY = ScreenDimensions - TreesSprite.Frame.Height;

            for (var x = 0; x < ScreenDimensions; x += TreesSprite.Frame.Width)
            {
                AddSprite(new TreesSprite(x, startY));
                AddSprite(new FoothillsSprite(x, startY - 4));
                AddSprite(new NearMountainsSprite(x, startY - 8));
                AddSprite(new FarMountainsSprite(x, startY - 16));
            }

            // set background color and FPS on the renderer
            var renderer = GameService.Instance.GameInstance.Renderer;
            renderer.ShowFPS = true;
            renderer.BackgroundColor = Color.Cyan;


            // position sun  in top 1/4 of the screen
            var sunPosition = (int)(0.15f * ScreenDimensions);
            AddSprite(new SunSprite(sunPosition, sunPosition));
        }

        public override void Activity()
        {
            base.Activity();

            if (clouds.Count < NumberOfClouds)
            {
                var cloud = new CloudSprite(0, 0, ScreenDimensions);
                cloud.X = -cloud.CurrentFrame.Width / 2f;
                cloud.Y = (float)(GameService.Instance.Random.NextDouble() * (0.3f * ScreenDimensions));

                // add to both our scene graph and local list
                AddSprite(cloud);
                clouds.Add(cloud);
            }

            for (var i = clouds.Count - 1; i > -1; i--)
            {
                if (clouds[i].Destroyed)
                {
                    clouds.RemoveAt(i);
                }
            }
        }
    }
}
