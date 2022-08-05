using Glade2d.Graphics;
using Glade2d.Services;

namespace Glade2dExample.Entities
{
    public class CloudSprite : Sprite
    {
        float screenMaxDimension;

        public CloudSprite(int x = 0, int y = 0, float screenMaxDimension = 256)
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
            if (this.X - (this.CurrentFrame.Width / 2f) > screenMaxDimension)
            {
                Die();
            }
        }
    }
}
