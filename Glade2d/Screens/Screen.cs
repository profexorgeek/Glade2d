﻿using Glade2d.Graphics;
using Glade2d.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Glade2d.Screens
{
    public class Screen
    {
        List<Sprite> sprites = new List<Sprite>();
        bool listSortNeeded = false;

        public Screen() { }

        /// <summary>
        /// Updates all children
        /// </summary>
        public void Update()
        {
            if(listSortNeeded)
            {
                LogService.Log.Trace("Resorting sprite list.");
                sprites = sprites.OrderBy(s => s.Layer).ToList();
                listSortNeeded = false;
            }

            for(var i = sprites.Count - 1; i > -1; i--)
            {
                sprites[i].Update();
            }
        }

        /// <summary>
        /// Adds a sprite to the list and marks the list as
        /// needing sorted so layer order is correct.
        /// 
        /// If the sprite is already in the list, this call is
        /// ignored.
        /// </summary>
        /// <param name="sprite">The sprite to add</param>
        public void AddSprite(Sprite sprite)
        {
            LogService.Log.Trace("Adding sprite to scene graph");
            if (!sprites.Contains(sprite))
            {
                sprites.Add(sprite);

                // Can't reorder the list right now because we could
                // be iterating over it. Flag that the list needs sorting
                listSortNeeded = true;
            }
        }

        /// <summary>
        /// Removes a sprite from the list if it exists in the list
        /// </summary>
        /// <param name="sprite"></param>
        public void RemoveSprite(Sprite sprite)
        {
            LogService.Log.Trace("Removing sprite from scene graph");
            if (sprites.Contains(sprite))
            {
                sprites.Remove(sprite);
            }
        }

        /// <summary>
        /// Get the sprite list. This should only be used for rendering
        /// 
        /// TODO: find a better way to protect the list so it is always sorted
        /// while still making it available to the renderer and not duplicating
        /// the collection every frame
        /// </summary>
        /// <returns></returns>
        public List<Sprite> AccessSpritesForRenderingOnly()
        {
            return sprites;
        }
    }
}
