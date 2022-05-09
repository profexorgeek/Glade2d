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
    public class Renderer
    {
        MicroGraphics graphicsDriver;
        Dictionary<string, IDisplayBuffer> textures = new Dictionary<string, IDisplayBuffer>();

        Color BackgroundColor { get; set; } = Color.Black;
        Color TransparentColor { get; set; } = Color.Magenta;
        bool ShowFPS { get; set; } = true;

        public Renderer(MicroGraphics graphics)
        {
            LogService.Log.Trace("Initializing Renderer");
            graphicsDriver = graphics;
            graphicsDriver.Clear(true);
            graphicsDriver.CurrentFont = new Font12x16();
        }

        public void Clear(bool sendToDevice = false)
        {
            graphicsDriver.Clear(sendToDevice);
            graphicsDriver.DrawRectangle(0, 0, 240, 240, BackgroundColor, true);
        }

        public void RenderFrame(int originX, int originY, Frame frame)
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
                        graphicsDriver.DrawPixel(originX + x, originY + y, pixel);
                    }
                }
            }
        }

        public void DrawBuffer()
        {
            if(ShowFPS)
            {
                graphicsDriver.DrawRectangle(0, 0, graphicsDriver.Width, 16, Color.Black, true);
                graphicsDriver.DrawText(0, 0, $"{GameService.Instance.Time.FPS}fps", Color.White);
            }

            graphicsDriver.Show();
        }

        public void LoadTexture(string name)
        {
            var buffer = LoadBitmapFile(name);
            textures.Add(name, buffer);
        }

        public void UnloadTexture(string name)
        {
            textures.Remove(name);
        }

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

    }
}