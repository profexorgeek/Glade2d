using Glade2d.Services;
using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Glade2d.Graphics
{
    public class Renderer : MicroGraphics
    {
        private const int BytesPerPixel = 2;
        private readonly int _width, _height;
        
        readonly Dictionary<string, IPixelBuffer> textures = new Dictionary<string, IPixelBuffer>();
        public Color BackgroundColor { get; set; } = Color.Black;
        public Color TransparentColor { get; set; } = Color.Magenta;
        public bool ShowPerf { get; set; } = false;
        public int Scale { get; private set; }
        public bool UseTransparency { get; set; } = true;
        public bool RenderInSafeMode { get; set; } = false;

        public Renderer(IGraphicsDisplay display, int scale = 1, RotationType rotation = RotationType.Default)
            : base(display)
        {
            this.Scale = scale;
            this.Rotation = rotation;

            // If we are rendering at a different resolution than our
            // device, we need to create a new buffer as our primary drawing buffer
            // so we draw at the scaled resolution
            if (scale > 1)
            {
                var scaledWidth = display.Width / scale;
                var scaledHeight = display.Height / scale;
                this.pixelBuffer = GetBufferForColorMode(display.ColorMode, scaledWidth, scaledHeight);
                LogService.Log.Trace($"Initialized renderer with custom buffer: {scaledWidth}x{scaledHeight}");
            }
            else
            {
                // do nothing, the default behavior is to draw
                // directly into the graphics buffer
                LogService.Log.Trace($"Initialized renderer using default display driver buffer: {display.Width}x{display.Height}");
            }

            textures = new Dictionary<string, IPixelBuffer>();
            CurrentFont = new Font4x6();
            
            _width = Width;
            _height = Height;

            if (display.PixelBuffer.BitDepth != 16)
            {
                var message = $"Only 16bpp is supported but {display.PixelBuffer.BitDepth} was set";
                throw new InvalidOperationException(message);
            }
        }

        public void Reset()
        {
            pixelBuffer.Fill(BackgroundColor);
        }

        /// <summary>
        /// Draws a frame into the graphics buffer
        /// </summary>
        /// <param name="originX">The frame's X origin point</param>
        /// <param name="originY">The frame's Y origin point</param>
        /// <param name="frame">The frame to be drawn</param>
        public void DrawFrame(int originX, int originY, Frame frame)
        {
            if (!textures.ContainsKey(frame.TextureName))
            {
                LoadTexture(frame.TextureName);
            }

            var transparentByte1 = (byte)(TransparentColor.Color16bppRgb565 >> 8);
            var transparentByte2 = (byte)(TransparentColor.Color16bppRgb565 & 0x00FF);

            var imgBuffer = textures[frame.TextureName];

            var imgBufferWidth = imgBuffer.Width;
            var pixelBufferWidth = pixelBuffer.Width;
            var pixelBufferHeight = pixelBuffer.Height;
            var frameHeight = frame.Height;
            var frameWidth = frame.Width;
            var frameX = frame.X;
            var frameY = frame.Y;
            var innerPixelBuffer = pixelBuffer.Buffer;
            var innerImgBuffer = imgBuffer.Buffer;
            
            for (var x = frameX; x < frameX + frameWidth; x++)
            {
                for (var y = frameY; y < frameY + frameHeight; y++)
                {
                    var tX = originX + x - frameX;
                    var tY = originY + y - frameY;
                    
                    RotateCoordinates(ref tX, ref tY, pixelBufferWidth, pixelBufferHeight, Rotation);

                    // only draw if not transparent and within buffer
                    if (tX >= 0 && tY >= 0 && tX < _width && tY < _height)
                    {
                        // temporarily assuming rgb565
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
        /// Draws a sprite's CurrentFrame into the
        /// graphics buffer
        /// </summary>
        /// <param name="sprite">The sprite to draw</param>
        public void DrawSprite(Sprite sprite)
        {
            if (sprite.CurrentFrame != null)
            {
                DrawFrame((int)sprite.X, (int)sprite.Y, sprite.CurrentFrame);
            }
        }

        /// <summary>
        /// Loads a bitmap texture into memory
        /// </summary>
        /// <param name="name">The texture name, such as myimage.bmp</param>
        public void LoadTexture(string name)
        {
            var buffer = LoadBitmapFile(name);
            textures.Add(name, buffer);
        }

        /// <summary>
        /// Unloads a bitmap texture from memory
        /// </summary>
        /// <param name="name">The </param>
        public void UnloadTexture(string name)
        {
            textures.Remove(name);
        }

        /// <summary>
        /// Internal method that loads a bitmap file from disk and
        /// creates an IDisplayBuffer
        /// </summary>
        /// <param name="name">The bitmap file path</param>
        /// <returns>An IDisplayBuffer containing bitmap data</returns>
        IPixelBuffer LoadBitmapFile(string name)
        {
            LogService.Log.Trace($"Attempting to LoadBitmapFile: {name}");
            var filePath = Path.Combine(MeadowOS.FileSystem.UserFileSystemRoot, name);
            IPixelBuffer imgBuffer;

            try
            {
                var img = Image.LoadFromFile(filePath);

                LogService.Log.Trace($"Got image at {img.BitsPerPixel} and buffer is {pixelBuffer.BitDepth}");

                // Always make sure that the texture is formatted in the same color mode as the display
                LogService.Log.Info($"Image {name} is wrong bit depth ({img.BitsPerPixel}bpp), matching buffer depth of {pixelBuffer.BitDepth}.");
                imgBuffer = GetBufferForColorMode(pixelBuffer.ColorMode, img.Width, img.Height);
                imgBuffer.WriteBuffer(0, 0, img.DisplayBuffer);

                LogService.Log.Trace($"{name} loaded to buffer of type {imgBuffer.GetType()}");
                return imgBuffer;
            }
            catch (Exception ex)
            {
                LogService.Log.Error($"Failed to load {filePath}: The file should be a 24bit bmp, in the root directory with BuildAction = Content, and Copy if Newer!");
                throw ex;
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

            base.Show();
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

        public static IPixelBuffer GetBufferForColorMode(ColorType mode, int width, int height)
        {
            IPixelBuffer buffer;
            switch (mode)
            {
                case ColorType.Format12bppRgb444:
                    buffer = new BufferRgb444(width, height);
                    break;
                case ColorType.Format16bppRgb565:
                    buffer = new BufferRgb565(width, height);
                    break;
                case ColorType.Format24bppRgb888:
                    buffer = new BufferRgb888(width, height);
                    break;
                case ColorType.Format32bppRgba8888:
                    buffer = new BufferRgb8888(width, height);
                    break;
                default:
                    throw new NotImplementedException($"Color mode {mode} has not been implemented by this renderer yet!");
            }

            return buffer;
        }

        /// <summary>
        /// Takes target coordinates and adjusts them for a rotated display
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RotateCoordinates(ref int x, ref int y, int width, int height, RotationType rotationType)
        {
            switch (rotationType)
            {
                case RotationType._90Degrees:
                {
                    var temp = y;
                    y = x;
                    x = width - temp;
                    break;
                }

                case RotationType._180Degrees:
                {
                    x = width - x;
                    y = height - y;
                    break;
                }

                case RotationType._270Degrees:
                {
                    var temp = y;
                    y = width - x;
                    x = temp;
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the index for a specific x and y coordinate in a pixel buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBufferIndex(int x, int y, int width, int bytesPerPixel)
        {
            return (y * width + x) * bytesPerPixel;
        }
    }
}
