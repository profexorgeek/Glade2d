using Glade2d.Services;
using System;

namespace Glade2d.Graphics
{
    public class Sprite
    {
        /// <summary>
        /// The X position of this object
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// The Y position of this object
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// The width of this sprite's current frame or 0 if there is no current frame.
        /// </summary>
        public float Width => this.CurrentFrame != null ? this.CurrentFrame.Width : 0;

        /// <summary>
        /// The height of this sprite's current frame or 0 if there is no current frame.
        /// </summary>
        public float Height => this.CurrentFrame != null ? this.CurrentFrame.Height : 0;

        /// <summary>
        /// The top edge of this sprite in world coordinates
        /// </summary>
        public float Top => this.Y;

        /// <summary>
        /// The right edge of this sprite in world coordinates
        /// </summary>
        public float Right => this.X + Width;

        /// <summary>
        /// The bottom edge of this sprite in world coordinates
        /// </summary>
        public float Bottom => this.Y + Height;

        /// <summary>
        /// The left edge of this sprite in world coordinates
        /// </summary>
        public float Left => this.X;

        /// <summary>
        /// The X velocity of this object in pixels per second
        /// </summary>
        public float VelocityX { get; set; }

        /// <summary>
        /// The Y velocity of this object in pixels per second
        /// </summary>
        public float VelocityY { get; set; }

        /// <summary>
        /// The layer (rendering order) of this object...
        /// Larger numbers render on top
        /// </summary>
        public float Layer { get; set; }

        /// <summary>
        /// Tracks whether this object has been destroyed and should
        /// be removed from the scene graph
        /// </summary>
        public bool Destroyed { get; set; }

        /// <summary>
        /// The texture portion to use for this sprite
        /// </summary>
        public Frame CurrentFrame { get; set; }


        public Sprite() { }
        public Sprite(Frame frame)
        {
            this.CurrentFrame = frame;
        }


        /// <summary>
        /// Called by the engine to update this object
        /// </summary>
        public void Update()
        {
            var delta = GameService.Instance.Time.FrameDelta;
            this.X += (float)(VelocityX * delta);
            this.Y += (float)(VelocityY * delta);
            Activity();
        }

        /// <summary>
        /// Frame time method - intended to be overridden
        /// by child classes
        /// </summary>
        public virtual void Activity() { }

        /// <summary>
        /// Intended to be overridden by child classes to perform
        /// any destroy effects before this object is marked as
        /// destroyed.
        /// </summary>
        public virtual void Die()
        {
            this.Destroy();
        }

        /// <summary>
        /// Marks this object as destroyed
        /// </summary>
        public void Destroy()
        {
            Destroyed = true;
        }

        /// <summary>
        /// Checks if this sprite is overlapping another sprite by checking
        /// if the distance apart is greater than the sum of half of the
        /// widths on each axis
        /// </summary>
        /// <param name="sprite">The sprite to check</param>
        /// <returns>True if overlapping</returns>
        public bool IsOverlapping(Sprite sprite)
        {
            var xDist = sprite.X - this.X;
            var yDist = sprite.Y - this.Y;
            var xCollisionDistance = (this.Width / 2f) + (sprite.Width / 2f);
            var yCollisionDistance = (this.Height / 2f) + (sprite.Height / 2f);

            var xOverlap = xCollisionDistance - Math.Abs(xDist);
            var yOverlap = yCollisionDistance - Math.Abs(yDist);

            return xOverlap > 0 && yOverlap > 0;

        }
    }
}
