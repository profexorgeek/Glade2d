using Meadow.Foundation;

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
    /// The back buffer color and layer color
    /// </summary>
    public static readonly Color BackgroundColor = new Meadow.Foundation.Color(48, 44, 46);

    /// <summary>
    /// The specific color to use for "red" text so it matches game palette
    /// </summary>
    public static readonly Color RedTextColor = new Meadow.Foundation.Color(230, 72, 46);

    /// <summary>
    /// The specific color to use for the "white" text (which is not pure white)
    /// </summary>
    public static readonly Color WhiteTextColor = new Meadow.Foundation.Color(223, 246, 245);
}