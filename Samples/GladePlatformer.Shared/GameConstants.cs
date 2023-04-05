namespace GladePlatformer.Shared;

public class GameConstants
{
    public const string SpriteSheetName = "spritesheet.bmp";
    
    /// <summary>
    /// Names for different inputs that can be used
    /// </summary>
    public static class InputNames
    {
        /// <summary>
        /// Name of the input to perform an action, such as shooting
        /// </summary>
        public const string Jump = "action";
        
        /// <summary>
        /// Name of the button to move left
        /// </summary>
        public const string Left = "left";
        
        /// <summary>
        /// Name of the input to move right
        /// </summary>
        public const string Right = "right";
    }
}