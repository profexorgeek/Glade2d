using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Units;
using SimpleJpegDecoder;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Glade2d
{
    // Change F7MicroV2 to F7Micro for V1.x boards
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        readonly Color transparent = new Color(255, 0, 255);
        readonly Color background = Color.CornflowerBlue;
        RgbPwmLed onboardLed;
        MicroGraphics graphics;
        int displayWidth;
        int displayHeight;


        public MeadowApp()
        {
            Initialize();
            DrawShapes();
        }

        void Initialize()
        {
            Console.WriteLine("Initializing hardware...");

            onboardLed = new RgbPwmLed(
                device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue);
            onboardLed.SetColor(Color.Red);

            var config = new SpiClockConfiguration(
                speed: new Frequency(48000, Frequency.UnitType.Kilohertz),
                mode: SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(
                clock: Device.Pins.SCK,
                copi: Device.Pins.MOSI,
                cipo: Device.Pins.MISO,
                config: config);
            var st7789 = new St7789(
                device: Device,
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240,
                height: 240);

            displayWidth = Convert.ToInt32(st7789.Width);
            displayHeight = Convert.ToInt32(st7789.Height);

            graphics = new MicroGraphics(st7789);
            graphics.Rotation = RotationType._180Degrees;

            onboardLed.SetColor(Color.Green);
        }

        void DrawShapes()
        {
            Random rand = new Random();
            var stopwatch = new Stopwatch();

            var img = Image.LoadFromResource("Glade2d.Resources.logo.bmp");
            Console.WriteLine($"Loaded bitmap {img.Width}x{img.Height} with depth {img.BitsPerPixel} and buffer type {img.DisplayBuffer.GetType()}");
            IDisplayBuffer buffer = img.DisplayBuffer;

            //var jpgData = LoadResource("logo.jpg");
            //var decoder = new JpegDecoder();
            //var jpg = decoder.DecodeJpeg(jpgData);
            //var jpgBuffer = new BufferRgb888(decoder.Width, decoder.Height, jpg);

            var imgX = displayWidth / 2 - (buffer.Width / 2);
            var imgY = displayHeight / 2 - (buffer.Height / 2);

            graphics.CurrentFont = new Font4x6();
            long elapsed;
            float fps;

            stopwatch.Start();
            while(true)
            {
                stopwatch.Stop();
                elapsed = stopwatch.ElapsedMilliseconds;
                fps = 1000f / elapsed;
                stopwatch.Restart();
                graphics.Clear();
                graphics.DrawRectangle(0, 0, 240, 240, Color.CornflowerBlue, true);
                graphics.DrawText(5, 5, fps.ToString() + "fps", Color.Black);
                graphics.DrawBuffer(imgX, imgY, img.DisplayBuffer);
                //RenderBufferWithTransparency(imgX, imgY, img.DisplayBuffer);
                graphics.Show();
            }
        }

        void RenderBufferWithTransparency(int originX, int originY, IDisplayBuffer buffer)
        {
            for(var x = 0; x < buffer.Width; x++)
            {
                for(var y = 0; y < buffer.Height; y++)
                {
                    var pixel = buffer.GetPixel(x, y);
                    if(!pixel.Equals(transparent))
                    {
                        graphics.DrawPixel(originX + x, originY + y, pixel);
                    }
                }
            }
        }

        byte[] LoadResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = $"Glade2d.Resources.{filename}";
            var resources = string.Join(",", assembly.GetManifestResourceNames());
            Console.WriteLine($"Attempting to load {resourcePath} from set [{resources}]");
            
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

    }
}
