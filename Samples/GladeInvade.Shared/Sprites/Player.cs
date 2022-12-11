using Glade2d.Graphics;

namespace GladeInvade.Shared.Sprites;

public class Player : Sprite
{
    private const float HorizontalMovementSpeed = 40;
    
    public Player()
    {
        CurrentFrame = new Frame(GameConstants.SpriteSheetName, 3, 34, 25, 13);
    }

    public void MoveLeft()
    {
        VelocityX = -HorizontalMovementSpeed;
    }

    public void MoveRight()
    {
        VelocityX = HorizontalMovementSpeed;
    }

    public void StopMoving()
    {
        VelocityX = 0;
    }
}