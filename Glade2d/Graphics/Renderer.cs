using Glade2d.Services;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;

namespace Glade2d.Graphics
{
    public class Renderer : MicroGraphics
    {
        private const int BytesPerPixel = 2;
        private readonly TextureManager _textureManager;
        private readonly Layer _spriteLayer;
        
        public Color BackgroundColor
        {
            get => _spriteLayer.BackgroundColor;
            set => _spriteLayer.BackgroundColor = value;
        }

        public Color TransparentColor
        {
            get => _spriteLayer.TransparentColor;
            set => _spriteLayer.TransparentColor = value;
        }
        
        public bool ShowPerf { get; set; }
        public int Scale { get; private set; }
        public bool RenderInSafeMode { get; set; } = false;

        public Renderer(IGraphicsDisplay display, 
            TextureManager textureManager,
            int scale = 1) : base(display)
        {
            if (display.PixelBuffer.ColorMode != ColorType.Format16bppRgb565)
            {
                var message = $"Only color mode rgb565 is supported, but {base.display.PixelBuffer.ColorMode} " +
                              "was given.";

                throw new InvalidOperationException(message);
            }
            
            _textureManager = textureManager;
            Scale = scale;
            Rotation = rotation;

            // If we are rendering at a different resolution than our
            // device, we need to create a new buffer as our primary drawing buffer
            // so we draw at the scaled resolution
            if (scale > 1)
            {
                var scaledWidth = display.Width / scale;
                var scaledHeight = display.Height / scale;
                
                pixelBuffer = new BufferRgb565(scaledWidth, scaledHeight);
                LogService.Log.Trace($"Initialized renderer with custom buffer: {scaledWidth}x{scaledHeight}");
            }
            else
            {
                // do nothing, the default behavior is to draw
                // directly into the graphics buffer
                LogService.Log.Trace($"Initialized renderer using default display driver buffer: {display.Width}x{display.Height}");
            }

            CurrentFont = new Font4x6();
            
            _spriteLayer = Layer.FromExistingBuffer((BufferRgb565)pixelBuffer, Rotation);
        }

        public void Reset()
        {
            pixelBuffer.Fill(BackgroundColor);
            _spriteLayer.Clear();
            
            // Should we be clearing the display buffer too???
        }

        /// <summary>
        /// Draws a sprite's CurrentFrame into the graphics buffer
        /// </summary>
        /// <param name="sprite">The sprite to draw</param>
        public void DrawSprite(Sprite sprite)
        {
            if (sprite.CurrentFrame != null)
            {
                var spriteOrigin = new Point((int)sprite.X, (int)sprite.Y);
                var textureOrigin = new Point(sprite.CurrentFrame.X, sprite.CurrentFrame.Y);
                var dimensions = new Dimensions(sprite.CurrentFrame.Width, sprite.CurrentFrame.Height);

                var texture = _textureManager.GetTexture(sprite.CurrentFrame.TextureName);
                _spriteLayer.DrawTexture(texture, textureOrigin, spriteOrigin, dimensions);
            }
        }

        /// <summary>
        /// Renders the contents of the internal buffer to the driver buffer and
        /// then blits the driver buffer to the device
        /// </summary>
        public void RenderToDisplay()
        {
            // draw the FPS counter
            if (ShowPerf)
            {
                DrawRectangle(0, 0, Width, CurrentFont.Height, Color.Black, true);
                DrawText(0, 0, $"{GameService.Instance.Time.FPS:n1}fps", Color.White);
            }

            // send the driver buffer to device
            Show();
        }

        public override void Show()
        {
            GameService.Instance.GameInstance.Profiler.StartTiming("Renderer.Show");
            // If we are doing a scaled draw, we must perform special copy logic
            if (Scale > 1)
            {
                if (RenderInSafeMode)
                {
                    ShowSafeMode();
                }
                else
                {
                    ShowFastMode();
                }
            }
            // if we're not doing a scaled draw, our buffers should match
            // draw the pixel buffer to the display
            else if (pixelBuffer != display.PixelBuffer)
            {
                display.PixelBuffer.WriteBuffer(0, 0, pixelBuffer);
            }

            GameService.Instance.GameInstance.Profiler.StartTiming("Micrographics.Show");
            base.Show();
            GameService.Instance.GameInstance.Profiler.StopTiming("Micrographics.Show");
            GameService.Instance.GameInstance.Profiler.StopTiming("Renderer.Show");
        }

        void ShowSafeMode()
        {
            // loop through X & Y, drawing pixels from buffer to device
            for (int x = 0; x < pixelBuffer.Width; x++)
            {
                for (int y = 0; y < pixelBuffer.Height; y++)
                {
                    // the target X and Y are based on the Scale
                    var tX = x * Scale;
                    var tY = y * Scale;
                    var color = pixelBuffer.GetPixel(x, y);

                    // draw the pixel multiple times to scale
                    for (var x1 = 0; x1 < Scale; x1++)
                    {
                        for (var y1 = 0; y1 < Scale; y1++)
                        {
                            display.DrawPixel(tX + x1, tY + y1, color);
                        }
                    }
                }
            }
        }

        private void ShowFastMode()
        {
            var displayBuffer = display.PixelBuffer.Buffer;
            var displayBufferWidth = display.PixelBuffer.Width;
            var sourceBuffer = pixelBuffer.Buffer;
            var sourceBufferWidth = pixelBuffer.Width;
            var sourceBufferHeight = pixelBuffer.Height;
            var displayBytesPerRow = display.PixelBuffer.Width * BytesPerPixel;

            for (var y = 0; y < sourceBufferHeight; y++)
            {
                var yScaled = y * Scale;

                // First draw all of the pixels in a row into the
                // destination buffer
                for (var x = 0; x < sourceBufferWidth; x++)
                {
                    var frameIndex = (y * sourceBufferWidth + x) * BytesPerPixel;
                    var colorByte1 = sourceBuffer[frameIndex];
                    var colorByte2 = sourceBuffer[frameIndex + 1];
                    var xScaled = x * Scale;
                    for (var i = 0; i < Scale; i++)
                    {
                        var index = (yScaled * displayBufferWidth + xScaled + i) * BytesPerPixel;
                        displayBuffer[index] = colorByte1;
                        displayBuffer[index + 1] = colorByte2;
                    }
                }

                // now that we have drawn a row, blit the entire row
                // this is 1-indexed because we've already drawn the first row
                // [Scale] more times on the Y axis - this is faster than
                // drawing pixel-by-pixel!
                for (var i = 1; i < Scale; i++)
                {
                    var copyFromStartIndex = yScaled * displayBufferWidth * BytesPerPixel;
                    var copyToStartIndex = (yScaled + i) * displayBufferWidth * BytesPerPixel;
                    
                    Array.Copy(displayBuffer, copyFromStartIndex, displayBuffer, copyToStartIndex, displayBytesPerRow);
                }
            } 
        }
    }
}