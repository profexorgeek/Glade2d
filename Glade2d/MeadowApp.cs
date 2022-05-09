using Glade2d.Services;
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
using Image = Glade2d.Graphics.Image;
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
            LogService.Log.Level = Logging.LogLevel.Trace;

            Initialize();
            DrawTest();
        }

        void Initialize()
        {
            LogService.Log.Trace("Initializing hardware...");

            onboardLed = new RgbPwmLed(
                device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue);
            onboardLed.SetColor(Color.Red);
            LogService.Log.Trace("Onboard LED initialized.");

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
            LogService.Log.Trace("St7789 Graphics Device initialized.");

            displayWidth = Convert.ToInt32(st7789.Width);
            displayHeight = Convert.ToInt32(st7789.Height);

            graphics = new MicroGraphics(st7789);
            graphics.Rotation = RotationType._180Degrees;
            graphics.Clear(true);
            LogService.Log.Trace("Graphics buffer initialized.");

            onboardLed.SetColor(Color.Green);

            LogService.Log.Trace("Initialization complete.");
        }

        

        void DrawTest()
        {

            var stopwatch = new Stopwatch();
            graphics.CurrentFont = new Font4x6();
            long elapsed;
            float fps;

            var img47 = LoadBitmapFile("47x47.bmp");
            var img64 = LoadBitmapFile("64x64.bmp");
            var img48 = LoadBitmapFile("48x48.bmp");
            var img120 = LoadBitmapFile("120x120.bmp");



            //var imgX = (displayWidth / 2) - (test.Width / 2);
            //var imgY = (displayHeight / 2) - (test.Height / 2);

            stopwatch.Start();
            while (true)
            {
                stopwatch.Stop();
                elapsed = stopwatch.ElapsedMilliseconds;
                fps = 1000f / elapsed;
                stopwatch.Restart();
                graphics.Clear();
                graphics.DrawRectangle(0, 0, 240, 240, Color.CornflowerBlue, true);
                graphics.DrawText(5, 5, fps.ToString() + "fps", Color.Black);

                graphics.DrawBuffer(0, 0, img47);
                graphics.DrawBuffer(100, 0, img64);
                graphics.DrawBuffer(0, 100, img48);
                graphics.DrawBuffer(100, 100, img120);

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

        IDisplayBuffer LoadBitmapResource(string name)
        {
            LogService.Log.Trace($"Attempting to LoadBitmapResource: {name}");
            var resourcePath = $"Glade2d.Resources.{name}";
            var img = Image.LoadFromResource(resourcePath);
            return img.DisplayBuffer;
        }

        IDisplayBuffer LoadBitmapFile(string name)
        {
            LogService.Log.Trace($"Attempting to LoadBitmapFile: {name}");
            var filePath = Path.Combine(MeadowOS.FileSystem.UserFileSystemRoot, name);
            var img = Image.LoadFromFile(filePath);
            return img.DisplayBuffer;
        }

        IDisplayBuffer LoadJpgResource(string name)
        {
            LogService.Log.Trace($"Attempting to LoadJpgResource: {name}");
            var bytes = FileService.Instance.LoadResource(name);
            var decoder = new JpegDecoder();
            var jpg = decoder.DecodeJpeg(bytes);
            var buffer = new BufferRgb888(decoder.Width, decoder.Height, jpg);
            return buffer;
        }
    }
}
