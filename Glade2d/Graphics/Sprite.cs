using Glade2d.Services;

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
        /// destoryed.
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
    }
}
