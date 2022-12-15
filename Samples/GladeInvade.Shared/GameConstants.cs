namespace GladeInvade.Shared;

/// <summary>
/// Constants used for the Glade Invade game
/// </summary>
public static class GameConstants
{
    /// <summary>
    /// The name of the global sprite sheet to load textures from
    /// </summary>
    public const string SpriteSheetName = "glade-invade.bmp";

    /// <summary>
    /// Names for different inputs that can be used
    /// </summary>
    public static class InputNames
    {
        /// <summary>
        /// Name of the input to perform an action, such as shooting
        /// </summary>
        public const string Action = "action";
        
        /// <summary>
        /// Name of the button to move left
        /// </summary>
        public const string Left = "left";
        
        /// <summary>
        /// Name of the input to move right
        /// </summary>
        public const string Right = "Right";
    }
}