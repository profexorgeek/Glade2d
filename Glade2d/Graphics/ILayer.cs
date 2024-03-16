using System.Numerics;
using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;

namespace Glade2d.Graphics;

/// <summary>
/// A layer represents an isolated pixel buffer that can be drawn to, cached, and manipulated individually. 
/// </summary>
public interface ILayer
{
    /// <summary>
    /// How far the layer's origin (0,0) is offset from the camera's origin. 
    /// </summary>
    public Point CameraOffset { get; set; }
   
    /// <summary>
    /// The default background for this layer
    /// </summary>
    public Color BackgroundColor { get; set; }
    
    /// <summary>
    /// What RGB value we consider to be transparent. When textures are drawn to the buffer, pixels of this color
    /// will be skipped.
    /// </summary>
    public Color TransparentColor { get; set; }
   
    /// <summary>
    /// If true, then the layer will be drawn taking transparency into account. Any pixels that match the
    /// set transparent color will not be drawn to the target, and thus layers below will be visible. Enabling
    /// this slows down layer draw times and should only be enabled where a benefit to transparency will
    /// be given.
    /// </summary>
    public bool DrawLayerWithTransparency { get; set; }
    
    /// <summary>
    /// The width of the layer's canvas
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// The height of the layer's canvas
    /// </summary>
    public int Height { get; }
   
    public IFont DefaultFont { get; }

    /// <summary>
    /// Fills the whole pixel buffer with the specified color
    /// </summary>
    public void Clear();

    /// <summary>
    /// Draws the texture defined by the passed in frame
    /// </summary>
    /// <param name="frame">The texture frame to draw</param>
    /// <param name="topLeftOnLayer">
    /// The X and Y coordinates on the layer that we will start drawing onto.
    /// </param>
    public void DrawTexture(Frame frame, Point topLeftOnLayer);

    /// <summary>
    /// Draws the specified texture to the layer.
    /// </summary>
    /// <param name="texture">The full image texture we will draw from</param>
    /// <param name="topLeftOnTexture">
    /// The X and Y coordinates of the top left layer on the texture that we will read pixels from
    /// </param>
    /// <param name="topLeftOnLayer">
    /// The X and Y coordinates on the layer that we will start drawing onto.
    /// </param>
    /// <param name="drawSize">The height and width of the amount of pixel data to draw</param>
    /// <param name="ignoreTransparency">
    /// If true, all pixels from the buffer will be drawn, even if it matches the
    /// transparency color
    /// </param>
    public void DrawTexture(BufferRgb565 texture,
        Point topLeftOnTexture,
        Point topLeftOnLayer,
        Dimensions drawSize,
        bool ignoreTransparency = false);

    /// <summary>
    /// Shifts the pixels in the layer by the specified amount. Pixels that shift off the layer
    /// will be wrapped to the other side.
    /// </summary>
    /// <param name="shiftAmount"></param>
    public void Shift(Vector2 shiftAmount);

    /// <summary>
    /// Draws text to the layer using a custom font
    /// </summary>
    /// <param name="position">The position to draw the text</param>
    /// <param name="text">The text to draw</param>
    /// <param name="font">The font to use for the text</param>
    /// <param name="color">The color to use</param>
    public void DrawText(Point position, string text, IFont font = null, Color? color = null);
}