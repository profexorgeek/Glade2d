using Glade2d.Services;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Glade2d.Graphics
{
    /// <summary>
    /// This class acts as an intermediate buffer between MicroGraphics and
    /// the actual display driver buffer. This exists because MicroGraphics
    /// is hardcoded to draw directly to the device driver buffer but in many
    /// cases the user may want to scale the entire rendering process up to 
    /// render at 200% or more.
    /// 
    /// Using this as the display device for MicroGraphics allows extending 
    /// the MicroGraphics library to support double-buffering and final
    /// transformations on the composite buffer before blitting to hardware.
    /// </summary>
    public class Glade2Buffer : IGraphicsDisplay
    {
        protected BufferBase buffer;
        protected IGraphicsDisplay device;
        protected bool ignoreOutOfBounds;

        public virtual ColorType ColorMode => device.ColorMode;
        public int Width => buffer.Width;
        public int Height => buffer.Height;
        public int Scale { get; protected set; }
        public bool IgnoreOutOfBoundsPixels
        {
            get
            {
                return ignoreOutOfBounds;
            }
            set
            {
                ignoreOutOfBounds = value;
                device.IgnoreOutOfBoundsPixels = ignoreOutOfBounds;
            }
        }
        public int BitsPerPixel
        {
            get
            {
                switch(device.ColorMode)
                {
                    case ColorType.Format1bpp:
                        return 1;
                    case ColorType.Format12bppRgb444:
                        return 12;
                    case ColorType.Format16bppRgb565:
                        return 16;
                    case ColorType.Format24bppRgb888:
                        return 24;
                    default:
                        throw new Exception("Unsupported color mode set!");
                }
            }
        }

        public Glade2Buffer(IGraphicsDisplay device, int scale = 1)
        {
            LogService.Log.Trace($"Creating buffer for display of color mode: {device.ColorMode}");
            this.device = device;Scale = scale;
            IgnoreOutOfBoundsPixels = true;

            var widthScaled = device.Width / scale;
            var heightScaled = device.Height / scale;

            switch(ColorMode)
            {
                case ColorType.Format1bpp:
                    buffer = new Buffer1bpp(widthScaled, heightScaled);
                    break;
                case ColorType.Format12bppRgb444:
                    buffer = new BufferRgb444(widthScaled, heightScaled);
                    break;
                case ColorType.Format16bppRgb565:
                    buffer = new BufferRgb565(widthScaled, heightScaled);
                    break;
                case ColorType.Format24bppRgb888:
                    buffer = new BufferRgb888(widthScaled, heightScaled);
                    break;
                default:
                    throw new Exception($"ColorMode {ColorMode} is unsupported!");
            }

            buffer = new BufferRgb565(device.Width / Scale, device.Height / Scale);
        }

        public void Clear(bool updateDisplay = false)
        {
            buffer.Clear();
            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Cascades call to similar underlying method
        /// </summary>
        /// <param name="x">The origin X</param>
        /// <param name="y">The origin Y</param>
        /// <param name="displayBuffer">The buffer to draw</param>
        public void DrawBuffer(int x, int y, IDisplayBuffer displayBuffer)
        {
            buffer.WriteBuffer(x, y, displayBuffer);
        }

        /// <summary>
        /// Cascades call to similar underlying method
        /// </summary>
        /// <param name="x">The pixel X</param>
        /// <param name="y">The pixel Y</param>
        /// <param name="color">The color to draw</param>
        public void DrawPixel(int x, int y, Color color)
        {
            if (IgnoreOutOfBoundsPixels && CheckOutOfBounds(x, y))
            {
                return;
            }
            buffer.SetPixel(x, y, color);
        }

        /// <summary>
        /// This method is intended for 1bpp displays just draws white
        /// for colored = true and black for colored = false
        /// </summary>
        /// <param name="x">The pixel X</param>
        /// <param name="y">The pixel Y</param>
        /// <param name="colored">True colors the pixel white, False colors it black</param>
        /// <exception cref="NotImplementedException"></exception>
        public void DrawPixel(int x, int y, bool colored)
        {
            if (IgnoreOutOfBoundsPixels && CheckOutOfBounds(x, y))
            {
                return;
            }
            DrawPixel(x, y, colored ? Color.White : Color.Black);
        }

        /// <summary>
        /// Fills the buffer with the specified color.
        /// </summary>
        /// <param name="fillColor">The color to use</param>
        /// <param name="updateDisplay">Whether to send the buffer to the display</param>
        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            buffer.Fill(fillColor);
            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Fills the specified portion of the buffer with the provided color
        /// </summary>
        /// <param name="x">Left point</param>
        /// <param name="y">Top point</param>
        /// <param name="width">Fill width</param>
        /// <param name="height">Fill height</param>
        /// <param name="fillColor">Fill color</param>
        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0) x = 0;
                if (y < 0) y = 0;
                if (x > Width - 1) x = Width - 1;
                if (y > Height - 1) y = Height - 1;
            }

            buffer.Fill(fillColor, x, y, width, height);
        }

        /// <summary>
        /// Inverts the specified pixel
        /// </summary>
        /// <param name="x">Pixel X</param>
        /// <param name="y">Pixel Y</param>
        public void InvertPixel(int x, int y)
        {
            if (IgnoreOutOfBoundsPixels && CheckOutOfBounds(x, y))
            {
                return;
            }
            var color = buffer.GetPixel(x, y);
            var inverse = new Color(byte.MaxValue - color.R, byte.MaxValue - color.G, byte.MaxValue - color.B);
            buffer.SetPixel(x, y, inverse);
        }

        /// <summary>
        /// Draws this buffer to the underlying display device and
        /// cascades the Show call to the device
        /// </summary>
        public void Show()
        {
            DrawBufferToDeviceWithScaling();
            device.Show();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Show(int left, int top, int right, int bottom)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Draws the internal buffer to the device buffer and applies nearest-neighbor
        /// scaling
        /// </summary>
        void DrawBufferToDeviceWithScaling()
        {
            // loop through X & Y, drawing pixels from buffer to device
            for (int x = 0; x < buffer.Width; x++)
            {
                for (int y = 0; y < buffer.Height; y++)
                {
                    // the target X and Y are based on the Scale
                    var tX = x * Scale;
                    var tY = y * Scale;
                    var color = buffer.GetPixel(x, y);

                    if (Scale > 1)
                    {
                        for (var x1 = 0; x1 < Scale; x1++)
                        {
                            for (var y1 = 0; y1 < Scale; y1++)
                            {
                                device.DrawPixel(tX + x1, tY + y1, color);
                            }
                        }
                    }
                    else
                    {
                        device.DrawPixel(x, y, color);
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the provided x and y are within the buffer size range
        /// </summary>
        /// <param name="x">The x index</param>
        /// <param name="y">The y index</param>
        /// <returns>True if X and Y are in range</returns>
        bool CheckOutOfBounds(int x, int y)
        {
            if (x < 0 || x >= buffer.Width || y < 0 || y >= buffer.Height)
            {
                return true;
            }
            return false;
        }

    }
}
