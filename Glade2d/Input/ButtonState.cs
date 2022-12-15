namespace Glade2d.Input
{
    /// <summary>
    /// Represents the different states a button may be in
    /// </summary>
    public enum ButtonState
    {
        /// <summary>
        /// The button is in an unactivated state
        /// </summary>
        Up = 0,
        
        /// <summary>
        /// The button is actively being pushed
        /// </summary>
        Down = 1,
        
        /// <summary>
        /// The button was in a pushed state last frame, and this frame it is now unactivated.
        /// The button will only be in this state for one frame.
        /// </summary>
        Pressed = 2,
    }
}