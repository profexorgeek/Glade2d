using Glade2d.Services;
using Glade2d.Logging;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowApp
{
    /// <summary>
    /// This is a test app that's useful for testing core MicroGraphics
    /// capabilities. You can set the type of app initialized by Program.cs
    /// to this app to test MicroGraphics without any special Glade2d features
    /// </summary>
    public class MicroGraphicsTest : App<F7FeatherV2>, IApp
    {
        IPixelBuffer bitmapBuffer;
        Image bitmapImage;
        Random rand = new Random();
        ISpiBus bus;


        async Task IApp.Initialize()
        {
            LogService.Log.Level = LogLevel.Trace;
            LogService.Log.Trace("Initializing MeadowGraphicsTest app");

            LogService.Log.Trace("Loading bitmap buffers...");

            try
            {
                bitmapImage = LoadBitmapToImage("testImage.bmp");
                bitmapBuffer = LoadBitmapToBuffer("testImage.bmp");
            }
            catch(Exception e)
            {
                LogService.Log.Error($"Error initializing app: {e.Message}");
            }
            

            LogService.Log.Trace("Init complete");
        }

        async Task IApp.Run()
        {
            PerformTestInMode(ColorType.Format16bppRgb565);

            //while (true)
            //{
            //    PerformTests();
            //    Thread.Sleep(10000);
            //}
        }

        void PerformTests()
        {
            LogService.Log.Trace("Starting test performance");

            var modes = Enum.GetValues(typeof(ColorType));
            Dictionary<ColorType, long> results = new Dictionary<ColorType, long>();
            foreach(var mode in modes)
            {
                var modeEnum = (ColorType)mode;
                long time = 0;
                try
                {
                    time = PerformTestInMode(modeEnum);
                }
                catch(Exception e)
                {
                    LogService.Log.Error($"Got exception: {e.Message}");
                    time = -1;
                }
                results.Add(modeEnum, time);
            }

            LogService.Log.Trace("==================================");
            LogService.Log.Trace("Render test results:");
            LogService.Log.Trace("-1 means unsupported mode");
            LogService.Log.Trace("==================================");
            foreach (var key in results.Keys)
            {
                LogService.Log.Trace($"{key}: {results[key]}ms");
            }
            LogService.Log.Trace("==================================");

            LogService.Log.Trace("MeadowGraphics test complete.");
        }

        long PerformTestInMode(ColorType mode)
        {
            LogService.Log.Trace($"Performing test in mode {mode}.");

            var renderer = GetRendererInMode(mode);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // clear and fill buffer
            renderer.Clear();
            renderer.DrawRectangle(0, 0, renderer.Width, renderer.Height, new Color(48, 44, 46), true);

            LogService.Log.Trace($"Calling shape methods...");
            for (var y = 40; y <= 120; y += 10)
            {
                var color = GetRandomColor();

                renderer.DrawCircle(20, y, 20, color);

                renderer.Stroke = 2;
                renderer.PenColor = color;
                renderer.DrawLine(48, y, 64, y, color);
                renderer.DrawRectangle(80, y, 40, 40, color);
            }

            // NOTE: DrawBitmap seems to be for 1-bit bitmaps and not actual bitmap images
            //LogService.Log.Trace($"Calling DrawBitmap {bitmapBuffer.Width}x{bitmapBuffer.Height}...");
            //renderer.DrawBitmap(120, 0, 32, 32, bitmapBuffer.Buffer, BitmapMode.Copy);

            LogService.Log.Trace($"Calling DrawBuffer {bitmapBuffer.Width}x{bitmapBuffer.Height}...");
            renderer.DrawBuffer(180, 80, bitmapBuffer);

            LogService.Log.Trace($"Calling DrawImage {bitmapImage.Width}x{bitmapImage.Height}...");
            renderer.DrawImage(140, 80, bitmapImage);

            renderer.DrawText(120, 12, $"MG Test Mode:{mode}", Color.GreenYellow, alignment: TextAlignment.Center);
            
            stopwatch.Stop();

            var ms = stopwatch.ElapsedMilliseconds;
            var elapsed = $"Done in {ms}ms";
            renderer.DrawText(120, 200, elapsed, Color.Cyan, alignment: TextAlignment.Center);

            renderer.Show();

            return ms;
        }

        IGraphicsDisplay GetDeviceInMode(ColorType mode)
        {
            LogService.Log.Trace($"Initializing St7789 Graphics Display in mode {mode}");
            var config = new SpiClockConfiguration(
                speed: new Frequency(48000, Frequency.UnitType.Kilohertz),
                mode: SpiClockConfiguration.Mode.Mode3);
            LogService.Log.Trace($"Got config {config.ToString()}");
            bus = Device.CreateSpiBus(
                clock: Device.Pins.SCK,
                copi: Device.Pins.MOSI,
                cipo: Device.Pins.MISO,
                config: config);
            LogService.Log.Trace($"Got bus {bus.ToString()}");
            var device = new St7789(
                device: Device,
                spiBus: bus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                displayColorMode: mode,
                width: 240,
                height: 240);
            LogService.Log.Trace($"Got device {device.ToString()} in mode {mode}");

            LogService.Log.Trace($"St7789 Graphics Display initialized in mode {mode}");
            return device;
        }

        MicroGraphics GetRendererInMode(ColorType mode)
        {
            var device = GetDeviceInMode(mode);
            var gfx = new MicroGraphics(device);
            gfx.CurrentFont = new Font8x12();
            return gfx;
        }

        Image LoadBitmapToImage(string name)
        {
            LogService.Log.Trace($"Attempting to LoadBitmapFile: {name}");
            var filePath = Path.Combine(MeadowOS.FileSystem.UserFileSystemRoot, name);
            return Image.LoadFromFile(filePath);
        }

        IPixelBuffer LoadBitmapToBuffer(string name)
        {
            var img = LoadBitmapToImage(name);
            return img.DisplayBuffer;
        }

        Color GetRandomColor()
        {
            return new Color(rand.Next(100, 255), rand.Next(100, 255), rand.Next(100, 255));
        }
    }
}
