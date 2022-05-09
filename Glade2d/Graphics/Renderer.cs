using Glade2d.Services;
using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Glade2d.Graphics
{
    /// <summary>
    /// This Renderer class uses a double-buffer pattern. Clear and Draw calls draw to a temporary
    /// buffer that is scaled by the scale parameter provided to the constructor. When Render is called,
    /// the contents of the graphicsBuffer are written to the display.
    /// 
    /// graphicsBuffer - the internal buffer where draw calls are composited
    /// graphicsDevice - the actual hardware graphics device, we avoid drawing to this more than required
    /// 
    /// NOTES:
    /// This used to use the MicroGraphics framework but it was too inflexible to support things like
    /// scaled rendering:
    /// https://github.com/WildernessLabs/Meadow.Foundation/blob/main/Source/Meadow.Foundation.Libraries_and_Frameworks/Graphics/MicroGraphics/Driver/Graphics.MicroGraphics/MicroGraphics.cs
    /// 
    /// This directly uses the SPI display drivers and there is probably tons of room for optimization:
    /// https://github.com/WildernessLabs/Meadow.Foundation/blob/main/Source/Meadow.Foundation.Peripherals/Displays.TftSpi/Driver/Displays.TftSpi/TftSpiBase.cs#L271
    /// https://github.com/WildernessLabs/Meadow.Foundation/blob/main/Source/Meadow.Foundation.Peripherals/Displays.TftSpi/Driver/Displays.TftSpi/Drivers/ST7789.cs#L16
    /// 
    /// </summary>
    public class Renderer
    {
        IGraphicsDisplay driver;
        Dictionary<string, IDisplayBuffer> textures = new Dictionary<string, IDisplayBuffer>();
        IDisplayBuffer graphicsBuffer;

        public Color BackgroundColor { get; set; } = Color.Black;
        public Color TransparentColor { get; set; } = Color.Magenta;
        public bool ShowFPS { get; set; } = true;
        public int Scale { get; private set; }
        public int Width => driver.Width / Scale;
        public int Height => driver.Height / Scale;

        public Renderer(IGraphicsDisplay display, int scale = 1)
        {
            LogService.Log.Trace("Initializing Renderer");

            Scale = scale;

            // clear and configure the display device
            driver = display;
            driver.Fill(BackgroundColor, true);
            //driver.IgnoreOutOfBoundsPixels = true;

            LogService.Log.Trace($"Got display driver of size: {driver.Width}x{driver.Height}");

            // set up a buffer whose size is determined by the draw Scale
            graphicsBuffer = new BufferRgb888(driver.Width / Scale, driver.Height / Scale);

            LogService.Log.Trace($"Created an internal buffer of size {graphicsBuffer.Width}x{graphicsBuffer.Height}");
        }

        /// <summary>
        /// Clears the local graphics buffer and fills it with the background color
        /// </summary>
        /// <param name="sendToDevice"></param>
        public void Clear(bool sendToDevice = false)
        {
            graphicsBuffer.Clear();
            graphicsBuffer.Fill(BackgroundColor);
            driver.Clear(sendToDevice);
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

            var buffer = textures[frame.TextureName];

            for (var x = frame.X; x < frame.X + frame.Width; x++)
            {
                for (var y = frame.Y; y < frame.Y + frame.Height; y++)
                {
                    var pixel = buffer.GetPixel(x, y);
                    if (!pixel.Equals(TransparentColor))
                    {
                        graphicsBuffer.SetPixel(originX + x - frame.X, originY + y - frame.Y, pixel);
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
            if(sprite.CurrentFrame != null)
            {
                DrawFrame((int)sprite.X, (int)sprite.Y, sprite.CurrentFrame);
            }
        }

        /// <summary>
        /// Renders the contents of the internal buffer to the driver buffer and
        /// then blits the driver buffer to the device
        /// </summary>
        public void Render()
        {
            // clear the graphics device
            driver.Clear();

            // draw the internal buffer to the driver buffer with scaling
            DrawBufferToDeviceWithScaling();

            // draw the FPS counter
            if (ShowFPS)
            {
                // TODO: port font rendering from MicroGraphics?
            }
                       
            // send the driver buffer to device
            driver.Show();
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
        IDisplayBuffer LoadBitmapFile(string name)
        {
            LogService.Log.Trace($"Attempting to LoadBitmapFile: {name}");
            var filePath = Path.Combine(MeadowOS.FileSystem.UserFileSystemRoot, name);

            try
            {
                var img = Image.LoadFromFile(filePath);
                return img.DisplayBuffer;
            }
            catch(Exception ex)
            {
                LogService.Log.Error($"Failed to load {filePath}: The file should be a 24bit bmp, in the root directory, BuildAction = Content, and Copy if Newer!");
                throw ex;
            }
            
        }

        /// <summary>
        /// Draws the internal buffer to the device buffer and applies nearest-neighbor
        /// scaling
        /// </summary>
        void DrawBufferToDeviceWithScaling()
        {
            // loop through X & Y, drawing pixels from buffer to device
            for(int x = 0; x < graphicsBuffer.Width; x++)
            {
                for(int  y = 0; y < graphicsBuffer.Height; y++)
                {
                    // the target X and Y are based on the Scale
                    var tX = x * Scale;
                    var tY = y * Scale;
                    var color = graphicsBuffer.GetPixel(x, y);

                    if(Scale > 1)
                    {
                        for(var x1 = 0; x1 < Scale; x1++)
                        {
                            for(var y1 = 0; y1 < Scale; y1++)
                            {
                                driver.DrawPixel(tX + x1, tY + y1, color);
                            }
                        }
                    }
                    else
                    {
                        driver.DrawPixel(x, y, color);
                    }
                    
                }
            }
        }
    }
}