using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;

namespace Glade2d.Graphics.Layers;

/// <summary>
/// A layer represents an isolated pixel buffer that can be drawn to, cached, and manipulated individually. 
/// </summary>
public class Layer
{
    private const int BytesPerPixel = 2; 

    private readonly BufferRgb565 _pixelBuffer;
    private readonly Dimensions _dimensions;
    
    /// <summary>
    /// How far the layer's origin (0,0) is offset from the camera's origin. 
    /// </summary>
    public Vector2 CameraOffset { get; set; }
   
    /// <summary>
    /// The default background for this layer
    /// </summary>
    public Color BackgroundColor { get; set; } = Color.Black;
    
    /// <summary>
    /// What RGB value we consider to be transparent. When textures are drawn to the buffer, pixels of this color
    /// will be skipped.
    /// </summary>
    public Color TransparentColor { get; set; } = Color.Magenta;

    private Layer(BufferRgb565 pixelBuffer)
    {
        _pixelBuffer = pixelBuffer;
        _dimensions = new Dimensions
        {
            Width = pixelBuffer.Width,
            Height = pixelBuffer.Height,
        };
    }

    /// <summary>
    /// Creates a new Layer with the specified dimensions
    /// </summary>
    public static Layer Create(Dimensions dimensions)
    {
        var pixelBuffer = new BufferRgb565(dimensions.Width, dimensions.Height);
        return new Layer(pixelBuffer);
    }
   
    /// <summary>
    /// Creates a new Layer from an existing pixel buffer. Only really used for the internal sprite layer, so that
    /// we can immediately draw sprite textures to the MicroGraphics display buffer instead of adding the overhead of
    /// an intermediary buffer. Having sprites rendered via a layer allows consolidation of drawing code.
    /// </summary>
    internal static Layer FromExistingBuffer(BufferRgb565 pixelBuffer)
    {
        return new Layer(pixelBuffer);
    }

    /// <summary>
    /// Fills the whole pixel buffer with the specified color
    /// </summary>
    public void Clear()
    {
        _pixelBuffer.Clear(BackgroundColor.Color16bppRgb565);
    }

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
    public void DrawTexture(BufferRgb565 texture,
        Point topLeftOnTexture,
        Point topLeftOnLayer,
        Dimensions drawSize)
    {
        var transparentByte1 = (byte)(TransparentColor.Color16bppRgb565 >> 8);
        var transparentByte2 = (byte)(TransparentColor.Color16bppRgb565 & 0x00FF);

        var imgBufferWidth = texture.Width;
        var pixelBufferWidth = _pixelBuffer.Width;
        var pixelBufferHeight = _pixelBuffer.Height;
        var frameHeight = drawSize.Height;
        var frameWidth = drawSize.Width;
        var frameX = topLeftOnTexture.X;
        var frameY = topLeftOnTexture.Y;
        var innerPixelBuffer = _pixelBuffer.Buffer;
        var innerImgBuffer = texture.Buffer;
        
        for (var x = frameX; x < frameX + frameWidth; x++)
        {
            for (var y = frameY; y < frameY + frameHeight; y++)
            {
                var tX = topLeftOnLayer.X + x - frameX;
                var tY = topLeftOnLayer.Y + y - frameY;
                
                // only draw if not transparent and within buffer
                if (tX >= 0 && tY >= 0 && tX < _dimensions.Width && tY < _dimensions.Height)
                {
                    var frameIndex = (y * imgBufferWidth + x) * BytesPerPixel;
                    var colorByte1 = innerImgBuffer[frameIndex];
                    var colorByte2 = innerImgBuffer[frameIndex + 1];
                    if (colorByte1 != transparentByte1 || colorByte2 != transparentByte2)
                    {
                        var bufferIndex = GetBufferIndex(tX, tY, pixelBufferWidth, BytesPerPixel);
                        innerPixelBuffer[bufferIndex] = colorByte1;
                        innerPixelBuffer[bufferIndex + 1] = colorByte2;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Renders the layer from its own buffer to the specified buffer.
    /// The passed in buffer is assumed to be the "camera" and thus the
    /// first byte of the target buffer is assumed to be the camera's
    /// 0,0/origin.
    /// </summary>
    internal void RenderToBuffer(BufferRgb565 target)
    {
        // Don't render if our buffer is the same as the target. This is
        // essentially a "don't do anything with the sprite layer" 
        // condition.
        if (target == _pixelBuffer)
        {
            return;
        }
       
        // TODO: Do we need different logic for each rotation? Maybe adjusting offset
        // is enough?
        RenderUnRotated(target);
    }
    
    /// <summary>
    /// Gets the index for a specific x and y coordinate in a pixel buffer
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetBufferIndex(int x, int y, int width, int bytesPerPixel)
    {
        return (y * width + x) * bytesPerPixel;
    }

    private void RenderUnRotated(BufferRgb565 buffer)
    {
        var sourceBuffer = _pixelBuffer.Buffer;
        var targetBuffer = buffer.Buffer;

        var sourceWidth = _dimensions.Width;
        var sourceHeight = _dimensions.Height;
        var targetWidth = buffer.Width;
        var targetHeight = buffer.Height;

        var rowOffset = (int)CameraOffset.Y;

        for (var sourceRow = 0; sourceRow < sourceHeight; sourceRow++)
        {
            var targetRow = sourceRow + rowOffset;
            if (targetRow < 0 || targetRow > targetHeight)
            {
                // Row is off the target buffer's area
                continue;
            }
            
            // Todo: Compute how many bytes on this row to copy. We need to adjust
            // this width for any pixels in the layer that are off the display's buffer. 
            // The width is probably static and we can probably calculate this
            // out of the hot loop
        }
    }
}