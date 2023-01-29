using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Glade2d.Services;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;

namespace Glade2d.Graphics.Layers;

/// <summary>
/// A layer represents an isolated pixel buffer that can be drawn to, cached, and manipulated individually. 
/// </summary>
public class Layer
{
    private readonly record struct DrawQuadrant(int StartX, int StartY, int Width, int Height);
    private const int BytesPerPixel = 2; 

    private readonly BufferRgb565 _layerBuffer;
    private readonly Dimensions _dimensions;
    private readonly TextureManager _textureManager;
    private float _horizontalShift, _verticalShift;
    private Vector2 _internalOrigin;
    
    /// <summary>
    /// How far the layer's origin (0,0) is offset from the camera's origin. 
    /// </summary>
    public Point CameraOffset { get; set; }
   
    /// <summary>
    /// The default background for this layer
    /// </summary>
    public Color BackgroundColor { get; set; } = Color.Black;
    
    /// <summary>
    /// What RGB value we consider to be transparent. When textures are drawn to the buffer, pixels of this color
    /// will be skipped.
    /// </summary>
    public Color TransparentColor { get; set; } = Color.Magenta;

    private Layer(BufferRgb565 layerBuffer, TextureManager textureManager)
    {
        _layerBuffer = layerBuffer;
        _textureManager = textureManager;
        _dimensions = new Dimensions
        {
            Width = layerBuffer.Width,
            Height = layerBuffer.Height,
        };
    }

    /// <summary>
    /// Creates a new Layer with the specified dimensions
    /// </summary>
    public static Layer Create(Dimensions dimensions)
    {
        var pixelBuffer = new BufferRgb565(dimensions.Width, dimensions.Height);
        return new Layer(pixelBuffer, GameService.Instance.GameInstance.TextureManager);
    }
   
    /// <summary>
    /// Creates a new Layer from an existing pixel buffer. Only really used for the internal sprite layer, so that
    /// we can immediately draw sprite textures to the MicroGraphics display buffer instead of adding the overhead of
    /// an intermediary buffer. Having sprites rendered via a layer allows consolidation of drawing code.
    /// </summary>
    internal static Layer FromExistingBuffer(BufferRgb565 pixelBuffer)
    {
        return new Layer(pixelBuffer, GameService.Instance.GameInstance.TextureManager);
    }

    /// <summary>
    /// Fills the whole pixel buffer with the specified color
    /// </summary>
    public void Clear()
    {
        _layerBuffer.Clear(BackgroundColor.Color16bppRgb565);
    }

    /// <summary>
    /// Draws the texture defined by the passed in frame
    /// </summary>
    /// <param name="frame">The texture frame to draw</param>
    /// <param name="topLeftOnLayer">
    /// The X and Y coordinates on the layer that we will start drawing onto.
    /// </param>
    public void DrawTexture(Frame frame, Point topLeftOnLayer)
    {
        var buffer = _textureManager.GetTexture(frame.TextureName);
        DrawTexture(buffer,
            new Point(frame.X, frame.Y),
            topLeftOnLayer,
            new Dimensions(frame.Width, frame.Height));
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
        var pixelBufferWidth = _layerBuffer.Width;
        var frameHeight = drawSize.Height;
        var frameWidth = drawSize.Width;
        var frameX = topLeftOnTexture.X;
        var frameY = topLeftOnTexture.Y;
        var innerPixelBuffer = _layerBuffer.Buffer;
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
                        var bufferIndex = GetBufferIndex(tX, tY, pixelBufferWidth);
                        innerPixelBuffer[bufferIndex] = colorByte1;
                        innerPixelBuffer[bufferIndex + 1] = colorByte2;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Shifts the pixels in the layer by the specified amount. Pixels that shift off the layer
    /// will be wrapped to the other side.
    /// </summary>
    /// <param name="shiftAmount"></param>
    public void Shift(Vector2 shiftAmount)
    {
        // The naive approach to shifting the layer around would be to rotate the actual bytes
        // in the buffer around. This requires 3 array copy calls per row (1 for the pixels that
        // will move off layer into a temporary buffer, one to move the remaining pixels over,
        // then a third to move the pixels from the temporary buffer to their wrapped around
        // area. This is particularly costly when most layers will only shift one pixel at a
        // time in any direction.
        // 
        // Instead we can fake it by just changing where the origin/top-left of the layer is
        // considered to be. This adds a tiny bit of extra draw calculations but should still
        // be cheaper than constant byte transferring.
        _internalOrigin += shiftAmount;
        
        // Normalize the origin so it's always somewhere within the bounds
        // of the layer.
        while (_internalOrigin.X < 0) _internalOrigin.X += _layerBuffer.Width;
        while (_internalOrigin.X >= _layerBuffer.Width) _internalOrigin.X -= _layerBuffer.Width;
        while (_internalOrigin.Y < 0) _internalOrigin.Y += _layerBuffer.Height;
        while (_internalOrigin.Y >= _layerBuffer.Height) _internalOrigin.Y -= _layerBuffer.Height;
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
        if (target == _layerBuffer)
        {
            return;
        }
        
        // Figure out where the source buffer overlaps the camera. All this code
        // assumes the engine does not support zooming, thus 1 unit is 1 pixel. This 
        // works with game scaling because the target buffer should be the 
        // renderer's pixel buffer, *not* the display buffer in that case.
        if (_layerBuffer.Height + CameraOffset.Y < 0 || // Layer is fully above the camera
            _layerBuffer.Width + CameraOffset.X < 0 || // Layer is fully left of the camera
            CameraOffset.Y >= target.Height || // Layer is fully below the camera
            CameraOffset.X >= target.Width) // Layer is fully right of the camera
        {
            return;
        }
        
        // We have four quadrants to draw from depending on how the internal origin
        // has shifted around. Calculate them out.
        var topLeft = new DrawQuadrant(0, 0, (int)_internalOrigin.X, (int)_internalOrigin.Y);
        var topRight = new DrawQuadrant((int)_internalOrigin.X,
            0,
            _layerBuffer.Width - (int)_internalOrigin.X,
            (int)_internalOrigin.Y);

        var bottomLeft = new DrawQuadrant(0,
            (int)_internalOrigin.Y,
            (int)_internalOrigin.X,
            _layerBuffer.Height - (int)_internalOrigin.Y);

        var bottomRight = new DrawQuadrant((int)_internalOrigin.X,
            (int)_internalOrigin.Y,
            _layerBuffer.Width - (int)_internalOrigin.X,
            _layerBuffer.Height - (int)_internalOrigin.Y);
        
        Draw each of the four quadrants

        // When figuring out the first row and column we pull pixels 
        // from on the layer's buffer, we need to take the offset into account.  
        // If the offset is positive, then we start from 0. If the offset is
        // negative, then we start at `0 - offset`.
        var sourceStartRow = CameraOffset.Y < 0 ? -CameraOffset.Y : 0;
        var sourceStartCol = CameraOffset.X < 0 ? -CameraOffset.X : 0;
        var targetStartCol = CameraOffset.X;

        // How many rows and columns will we be moving? This helps with byte counts
        var sourceRowCount = Math.Clamp(_layerBuffer.Height - sourceStartRow, 0, target.Height);
        var sourceColCount = Math.Clamp(_layerBuffer.Width - sourceStartCol, 0, target.Width);
        
        var sourceBuffer = _layerBuffer.Buffer;
        var targetBuffer = target.Buffer;

        var sourceWidth = _layerBuffer.Width;
        var targetWidth = target.Width;

        for (var sourceRow = sourceStartRow; sourceRow < sourceStartRow + sourceRowCount; sourceRow++)
        {
            var targetRow = sourceRow + CameraOffset.Y;
            var sourceBufferIndex = GetBufferIndex(sourceStartCol, sourceRow, sourceWidth);
            var targetBufferIndex = GetBufferIndex(targetStartCol, targetRow, targetWidth);

            // Copy the whole set of pixels from the source to the target
            Array.Copy(sourceBuffer, 
                sourceBufferIndex, 
                targetBuffer, 
                targetBufferIndex, 
                sourceColCount * BytesPerPixel);
        }
    }

    /// <summary>
    /// Gets the index for a specific x and y coordinate in a pixel buffer
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetBufferIndex(int x, int y, int width)
    {
        return (y * width + x) * BytesPerPixel;
    }
}