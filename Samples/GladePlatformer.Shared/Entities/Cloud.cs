using Glade2d.Graphics;
using Glade2d.Services;

namespace GladePlatformer.Shared.Entities
{
    public class Cloud : Sprite
    {
        public Cloud()
        {
        }

        public Cloud(float x = 0, float y = 0)
        {
            CurrentFrame = new Frame("spritesheet.bmp", 0, 64, 32, 16);

            if(GameService.Instance.Random.NextDouble() < 0.5)
            {
                CurrentFrame = new Frame("spritesheet.bmp", 32, 64, 16, 16);
            }

            this.X = x;
            this.Y = y;
        }
    }
}
