namespace Glade2d.Graphics
{
    public class Sprite
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Layer { get; set; }

        public Frame CurrentFrame { get; set; }

        public Sprite() { }

        public Sprite(Frame frame)
        {
            this.CurrentFrame = frame;
        }

        public void Update()
        {
            
        }
    }
}
