namespace Glade2d.Graphics
{
    public class Frame
    {
        public string TextureName { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Frame() { }

        public Frame(string textureName, int x, int y, int width, int height)
        {
            TextureName = textureName;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
