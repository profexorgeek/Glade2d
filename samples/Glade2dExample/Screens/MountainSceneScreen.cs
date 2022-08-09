using Glade2d.Screens;
using Glade2d.Services;
using Glade2dExample.Entities;
using Meadow.Foundation;
using System;
using System.Collections.Generic;

namespace Glade2dExample.Screens
{
    public class MountainSceneScreen : Screen
    {
        public const int NumberOfClouds = 4;
        List<CloudSprite> clouds = new List<CloudSprite>();

        int screenWidth;
        int screenHeight;

        public MountainSceneScreen()
        {
            screenWidth = GameService.Instance.GameInstance.Renderer.Width;
            screenHeight = GameService.Instance.GameInstance.Renderer.Height;

            LogService.Log.Trace($"Renderer reports screen size of {screenWidth}x{screenHeight}");
            
            int startY =  screenHeight - TreesSprite.Frame.Height;
            for (var x = 0; x < screenWidth; x += TreesSprite.Frame.Width)
            {
                AddSprite(new TreesSprite(x, startY));
                AddSprite(new FoothillsSprite(x, startY - 4));
                AddSprite(new NearMountainsSprite(x, startY - 8));
                AddSprite(new FarMountainsSprite(x, startY - 16));
            }

            // set background color and FPS on the renderer
            var renderer = GameService.Instance.GameInstance.Renderer;
            renderer.ShowPerf = true;
            renderer.BackgroundColor = Color.Cyan;


            // position sun  in top 1/4 of the screen
            var sunPosition = (int)(0.15f * screenWidth);
            AddSprite(new SunSprite(sunPosition, sunPosition));
        }

        public override void Activity()
        {
            base.Activity();
            if (clouds.Count < NumberOfClouds)
            {
                var cloud = new CloudSprite(0, 0, screenWidth);
                cloud.X = -cloud.CurrentFrame.Width / 2f;
                cloud.Y = (float)(GameService.Instance.Random.NextDouble() * (0.3f * screenHeight));
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
