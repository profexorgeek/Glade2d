
using Glade2d.Services;
using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Glade2d.Graphics
{
    public class Renderer : MicroGraphics
    {
        readonly Dictionary<string, IPixelBuffer> textures = new Dictionary<string, IPixelBuffer>();

        IGraphicsDriver Device { get; set; }
        Glade2Buffer Buffer { get; set; }
        public Color BackgroundColor { get; set; } = Color.Black;
        public Color TransparentColor { get; set; } = Color.Magenta;
        public bool ShowFPS { get; set; } = false;
        public int Scale { get; private set; }


        private Renderer(Glade2Buffer buffer, int scale = 1)
            : base(buffer)
        {
            textures = new Dictionary<string, IPixelBuffer>();
            CurrentFont = new Font4x6();
        }

        /// <summary>
        /// Factory method to produce a renderer in a valid state. This is required
        /// because MeadowGraphics is not extensible enough to extend in the way
        /// required by this project
        /// </summary>
        /// <param name="device">The display device that will be used for rendering</param>
        /// <param name="scale">The scale multiplier to render at</param>
        /// <returns></returns>
        public static Renderer GetRendererForDevice(IGraphicsDriver device, int scale = 1)
        {
            var buffer = new Glade2Buffer(device, scale);
            var renderer = new Renderer(buffer, scale);
            renderer.Device = device;
            renderer.Buffer = buffer;
            return renderer;
        }
        
        public void Reset()
        {
            Clear();
            Buffer.Fill(BackgroundColor);
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
                    var tX = originX + x - frame.X;
                    var tY = originY + y - frame.Y;

                    // only draw if not transparent and within buffer
                    if (!pixel.Equals(TransparentColor) &&
                        tX > 0 &&
                        tY > 0 &&
                        tX < buffer.Width &&
                        tY < buffer.Width)
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

                LogService.Log.Trace($"Got image at {img.BitsPerPixel} and buffer is {Buffer.BitsPerPixel}");

                if (img.BitsPerPixel == Buffer.BitsPerPixel)
                {
                    imgBuffer = img.DisplayBuffer as BufferRgb565;
                }
                else
                {
                    LogService.Log.Info($"Image {name} is wrong bit depth ({img.BitsPerPixel}bpp), matching buffer depth of {Buffer.BitsPerPixel}.");

                    switch(Buffer.BitsPerPixel)
                    {
                        case 1:
                            imgBuffer = new Buffer1bpp(img.Width, img.Height);
                            break;
                        case 12:
                            imgBuffer = new BufferRgb444(img.Width, img.Height);
                            break;
                        case 16:
                            imgBuffer = new BufferRgb565(img.Width, img.Height);
                            break;
                        case 24:
                            imgBuffer = new BufferRgb888(img.Width, img.Height);
                            break;
                        default:
                            throw new Exception("Unsupported buffer type detected!");
                    }

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
                DrawText(0, 0, $"{GameService.Instance.Time.FPS}fps", Color.White);
            }

            // send the driver buffer to device
            Show();
        }
    }
}
