using Glade2d.Screens;
using Glade2d.Services;
using Glade2d.Utility;
using GladeSampleShared.Entities;
using System;
using System.Collections.Generic;

namespace GladeSampleShared.Screens
{
    public class GladeDemoScreen : Screen
    {
        const int NumberOfClouds = 3;

        int screenWidth;
        int screenHeight;
        List<MountainChunk> mountains = new List<MountainChunk>();
        List<GroundChunk> grounds = new List<GroundChunk>();
        List<Tree> trees = new List<Tree>();
        List<Cloud> clouds = new List<Cloud>();

        public GladeDemoScreen()
        {
            // set screen dimensions for easy reference
            screenWidth = GameService.Instance.GameInstance.Renderer.Width;
            screenHeight = GameService.Instance.GameInstance.Renderer.Height;
            
            // Set background color
            GameService.Instance.GameInstance.Renderer.BackgroundColor = new Meadow.Foundation.Color(57, 120, 168, 255);

            // create background sky chunks, which do not move
            CreateSkyChunks();

            // create sun
            CreateSun();
        }

        public override void Activity()
        {
            DoMountainChunks();
            DoGroundChunks();
            DoTrees();
            DoClouds();

            base.Activity();
        }


        /// <summary>
        /// Fill the screen horizontally with sky sprites
        /// </summary>
        public void CreateSkyChunks()
        {
            for(var x = 0; x < screenWidth; x += SkyChunk.ChunkWidth)
            {
                var sky = new SkyChunk(x, 0);
                sky.Layer = -1;
                AddSprite(sky);
            }
        }

        /// <summary>
        /// Add a sun in the top left of the screen
        /// </summary>
        public void CreateSun()
        {
            var sun = new Sun(screenWidth - 8 - Sun.ChunkWidth, 8);
            sun.Layer = 0;
            AddSprite(sun);
        }

        /// <summary>
        /// Dynamically create and destroy mountain chunks as they
        /// scroll to the left
        /// </summary>
        public void DoMountainChunks()
        {
            float rightEdge = 0;
            float yOffset = screenHeight - 16 - MountainChunk.ChunkHeight;

            for(var i = mountains.Count - 1; i > - 1; i--)
            {
                var mtn = mountains[i];
                var mtnRightEdge = mtn.X + MountainChunk.ChunkWidth;

                if(mtnRightEdge < 0)
                {
                    mtn.Die();
                    mountains.Remove(mtn);
                }
                else
                {
                    rightEdge = Math.Max(mtnRightEdge, rightEdge);
                }
            }

            while(rightEdge < screenWidth)
            {
                var m = new MountainChunk(rightEdge, yOffset);
                m.VelocityX = -2f;
                m.Layer = 5;
                rightEdge += MountainChunk.ChunkWidth;

                mountains.Add(m);
                AddSprite(m);
            }
        }

        /// <summary>
        /// Dynamically create and destroy ground chunks as they
        /// scroll to the left
        /// </summary>
        public void DoGroundChunks()
        {
            float rightEdge = 0;
            float yOffset = screenHeight - GroundChunk.ChunkHeight;

            for (var i = grounds.Count - 1; i > -1; i--)
            {
                var grnd = grounds[i];
                var grndRightEdge = grnd.X + GroundChunk.ChunkWidth;

                if (grndRightEdge < 0)
                {
                    grnd.Die();
                    grounds.Remove(grnd);
                }
                else
                {
                    rightEdge = Math.Max(grndRightEdge, rightEdge);
                }
            }

            while (rightEdge < screenWidth)
            {
                var g = new GroundChunk(rightEdge, yOffset);
                g.VelocityX = -10f;
                g.Layer = 10;
                rightEdge += GroundChunk.ChunkWidth;

                grounds.Add(g);
                AddSprite(g);
            }
        }

        /// <summary>
        /// Dynamically create and destroy trees as they scroll to the left
        /// </summary>
        public void DoTrees()
        {
            float rightEdge = 0;
            float yOffset = screenHeight - GroundChunk.ChunkHeight - Tree.GroundOffset;
            float halfTreeWidth = Tree.ChunkWidth / 2f;
            var rand = GameService.Instance.Random;

            for (var i = trees.Count - 1; i > -1; i--)
            {
                var tree = trees[i];
                var grndRightEdge = tree.X + Tree.ChunkWidth;

                if (grndRightEdge < 0)
                {
                    tree.Die();
                    trees.Remove(tree);
                }
                else
                {
                    rightEdge = Math.Max(grndRightEdge, rightEdge);
                }
            }

            while (rightEdge < screenWidth + halfTreeWidth)
            {
                var xPos = rightEdge - halfTreeWidth;
                var yPos = yOffset + rand.Next(-1, 3);
                var t = new Tree(xPos, yPos);
                t.VelocityX = -5f;
                t.Layer = rand.Next(6, 10);
                rightEdge += halfTreeWidth;
                trees.Add(t);
                AddSprite(t);
            }
        }

        /// <summary>
        /// Dynamically create and destroy clouds as they drift to the left
        /// </summary>
        public void DoClouds()
        {
            var rand = GameService.Instance.Random;
            int yOffsetMin = screenHeight - 16 - MountainChunk.ChunkHeight;

            for (var i = clouds.Count - 1; i > -1; i--)
            {
                var cloud = clouds[i];
                var cloudRightEdge = cloud.X + cloud.CurrentFrame.Width;

                if(cloudRightEdge < 0)
                {
                    cloud.Die();
                    clouds.Remove(cloud);
                }
            }

            // This is intentionally only done once per frame instead of
            // a while loop like the other methods
            if(clouds.Count < NumberOfClouds)
            {
                var xPos = rand.Between(screenWidth, screenWidth + 8);
                var yPos = rand.Next(0, yOffsetMin);
                var c = new Cloud(xPos, yPos);
                c.VelocityX = rand.Between(-2f, -4f);
                c.Layer = 6;
                clouds.Add(c);
                AddSprite(c);
            }
        }

    }
}
