using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Foundation;

namespace Glade2d.Graphics;

/// <summary>
/// The renderer is the system which takes scene data, manages layers, draw them to a framebuffer,
/// and sends that frame buffer to the display.
/// </summary>
public interface IRenderer
{
    /// <summary>
    /// If true, shows FPS counter with the scene
    /// </summary>
    public bool ShowPerf { get; set; }
   
    /// <summary>
    /// Number of pixels horizontally in our render area
    /// </summary>
    public int Width { get; }
   
    /// <summary>
    /// Number of pixels vertically in our render area
    /// </summary>
    public int Height { get; }
    
    /// <summary>
    /// The color of the background when the scene is cleared between
    /// each frame.
    /// </summary>
    public Color BackgroundColor { get; set; }
    
    /// <summary>
    /// Renders the scene to the display.
    /// </summary>
    ValueTask RenderAsync(List<Sprite> sprites);
   
    /// <summary>
    /// Creates a new layer with the specified dimensions
    /// </summary>
    public ILayer CreateLayer(Dimensions dimensions);
}