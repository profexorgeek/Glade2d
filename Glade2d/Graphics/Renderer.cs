
using Glade2d.Services;
using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Glade2d.Graphics
{
    public class Renderer : MicroGraphics
    {
        readonly Dictionary<string, IPixelBuffer> textures = new Dictionary<string, IPixelBuffer>();
        public Color BackgroundColor { get; set; } = Color.Black;
        public Color TransparentColor { get; set; } = Color.Magenta;
        public bool ShowFPS { get; set; } = false;
        public int Scale { get; private set; }


        public Renderer(IGraphicsDisplay display, int scale = 1)
            : base(display)
        {
            this.Scale = scale;

            // If we are rendering at a different resolution than our
            // device, we need to create a new buffer as our primary drawing buffer
            // so we draw at the scaled resolution
            if(scale > 1)
            {
                var scaledWidth = display.Width / scale;
                var scaledHeight = display.Height / scale;
                this.pixelBuffer = GetBufferForColorMode(display.ColorMode, scaledWidth, scaledHeight);
            }
            else
            {
                // do nothing, the default behavior is to draw
                // directly into the graphics buffer
            }

            textures = new Dictionary<string, IPixelBuffer>();
            CurrentFont = new Font4x6();
        }
        
        public void Reset()
        {
            Clear();
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

            var imgBuffer = textures[frame.TextureName];

            for (var x = frame.X; x < frame.X + frame.Width; x++)
            {
                for (var y = frame.Y; y < frame.Y + frame.Height; y++)
                {
                    var pixel = imgBuffer.GetPixel(x, y);
                    var tX = originX + x - frame.X;
                    var tY = originY + y - frame.Y;

                    // only draw if not transparent and within buffer
                    if (!pixel.Equals(TransparentColor) &&
                        tX > 0 &&
                        tY > 0 &&
                        tX < pixelBuffer.Width &&
                        tY < pixelBuffer.Height)
                    {
                        DrawPixel(tX, tY, pixel);
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

                // if our loaded image buffer is the same color depth, we can just
                // directly use the image's display buffer
                // NOTE: it would be better to check actual ColorType but the image
                // buffer doesn't have a ColorType property, only BitsPerPixel
                if (img.BitsPerPixel == pixelBuffer.BitDepth)
                {
                    imgBuffer = img.DisplayBuffer;
                }

                // if our loaded image has a different color depth we do a one-time,
                // pixel-by-pixel slow copy into a matching buffer to make future
                // buffer blitting much faster!
                else
                {
                    LogService.Log.Info($"Image {name} is wrong bit depth ({img.BitsPerPixel}bpp), matching buffer depth of {pixelBuffer.BitDepth}.");
                    imgBuffer = GetBufferForColorMode(pixelBuffer.ColorMode, img.Width, img.Height);
                    imgBuffer.WriteBuffer(0, 0, img.DisplayBuffer);
                }

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
            if (ShowFPS)
            {
                DrawRectangle(0, 0, Width, CurrentFont.Height, Color.Black, true);
                DrawText(0, 0, $"{GameService.Instance.Time.FPS:n3}fps", Color.White);
            }

            // send the driver buffer to device
            Show();
        }

        public override void Show()
        {
            if(Scale > 1)
            {
                // TODO: this can be much faster if we draw a line and then array copy
                // the whole line * scale
                // loop through X & Y, drawing pixels from buffer to device
                var displayBuffer = display.PixelBuffer.Buffer;
                var displayBytesPerPixel = (int)Math.Round(display.PixelBuffer.BitDepth / 8f);
                var displayBytesPerRow = display.PixelBuffer.Width * displayBytesPerPixel;

                for (int y = 0; y < pixelBuffer.Height; y++)
                {
                    var yScaled = y * Scale;

                    // First draw all of the pixels in a row into the
                    // destination buffer
                    for (int x = 0; x < pixelBuffer.Width; x++)
                    {
                        var color = pixelBuffer.GetPixel(x, y);
                        var xScaled = x * Scale;
                        for (var i = 0; i < Scale; i++)
                        {
                            display.DrawPixel(xScaled + i, yScaled, color);
                        }
                    }

                    // now that we have drawn a row, blit the entire row
                    // this is 1-indexed because we've already drawn the first row
                    // [Scale] more times on the Y axis - this is faster than
                    // drawing pixel-by-pixel!
                    var startByteOffset = (yScaled * displayBytesPerRow);
                    for (var i = 1; i < Scale; i++)
                    {
                        var rowByteOffset = startByteOffset + (i * displayBytesPerRow);
                        //try
                        //{
                            Array.Copy(displayBuffer, startByteOffset, displayBuffer, rowByteOffset, displayBytesPerRow);
                        //}
                        //catch(Exception ex)
                        //{
                        //    var sb = new StringBuilder();
                        //    sb.Append($"Exception thrown when y = {y}\n");
                        //    sb.Append($"startByteOffset = {startByteOffset}");
                        //    sb.Append($"startCopyPoint = {rowByteOffset}");
                        //    sb.Append($"row byte length = {displayBytesPerRow}");
                        //    sb.Append($"total display buffer length = {displayBuffer.Length}");
                        //    sb.Append($"final byte should be {rowByteOffset + displayBytesPerRow}");
                        //    LogService.Log.Error($"Exception details: {sb.ToString()}");
                        //    throw ex;
                        //}
                        
                    }
                }
            }
            else
            {
                base.Show();
            }
        }

        public static IPixelBuffer GetBufferForColorMode(ColorType mode, int width, int height)
        {
            IPixelBuffer buffer;
            switch(mode)
            {
                case ColorType.Format12bppRgb444:
                    buffer = new BufferRgb444(width, height);
                    break;
                case ColorType.Format16bppRgb565:
                    buffer = new BufferRgb565(width, height);
                    break;
                default:
                    throw new NotImplementedException($"Color mode {mode} has not been implemented by this renderer yet!");
            }

            return buffer;
        }
    }
}
