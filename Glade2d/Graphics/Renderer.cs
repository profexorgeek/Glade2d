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
        Dictionary<string, IDisplayBuffer> textures = new Dictionary<string, IDisplayBuffer>();

        IGraphicsDisplay Device { get; set; }
        IGraphicsDisplay Buffer { get; set; }
        public Color BackgroundColor { get; set; } = Color.Black;
        public Color TransparentColor { get; set; } = Color.Magenta;
        public bool ShowFPS { get; set; } = false;
        public int Scale { get; private set; }
        public int Width => Buffer.Width;
        public int Height => Buffer.Height;


        private Renderer(GraphicsDisplayBufferRgb888 buffer, int scale = 1)
            : base(buffer)
        {
            textures = new Dictionary<string, IDisplayBuffer>();
        }

        public static Renderer GetRenderer(IGraphicsDisplay device, int scale = 1)
        {
            var buffer = new GraphicsDisplayBufferRgb888(device, scale);
            var renderer = new Renderer(buffer, scale);
            renderer.Device = device;
            renderer.Buffer = buffer;
            return renderer;
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
            catch (Exception ex)
            {
                LogService.Log.Error($"Failed to load {filePath}: The file should be a 24bit bmp, in the root directory, BuildAction = Content, and Copy if Newer!");
                throw ex;
            }

        }
    }
}
