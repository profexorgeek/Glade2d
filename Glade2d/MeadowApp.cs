﻿using Glade2d.Graphics;
using Glade2d.Services;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Units;

namespace Glade2d
{
    // Change F7MicroV2 to F7Micro for V1.x boards
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        RgbPwmLed onboardLed;
        MicroGraphics graphics;
        OldRenderer renderer;

        public MeadowApp()
        {
            LogService.Log.Level = Logging.LogLevel.Trace;

            Initialize();
            Start();
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
            var display = new St7789(
                device: Device,
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240,
                height: 240);
            LogService.Log.Trace("St7789 Graphics Device initialized.");

            var interimBuffer = new GraphicsDisplayBufferRgb888(display, 2);
            graphics = new MicroGraphics(interimBuffer);


            //renderer = new Renderer(display, 2);

            LogService.Log.Trace("Graphics buffer initialized.");

            onboardLed.SetColor(Color.Green);
            LogService.Log.Trace("Initialization complete.");
        }

        void Start()
        {
            GameService.Instance.Initialize();

            var sprite1 = new Sprite(
                new Frame()
                {
                    TextureName = "spritesheet.bmp",
                    X = 0,
                    Y = 0,
                    Width = 16,
                    Height = 16,
                });
            var sprite2 = new Sprite(
                new Frame()
                {
                    TextureName = "spritesheet.bmp",
                    X = 16,
                    Y = 0,
                    Width = 16,
                    Height = 16,
                });
            sprite2.X = 16;

            GameService.Instance.CurrentScreen.AddSprite(sprite1);
            GameService.Instance.CurrentScreen.AddSprite(sprite2);

            LogService.Log.Trace("Starting game loop.");
            while(true)
            {
                Update();
                Draw();
            }
        }

        void Update()
        {
            GameService.Instance.Time?.Update();
            GameService.Instance.CurrentScreen?.Update();
        }

        void Draw()
        {
            graphics.Clear();
            graphics.DrawCircle(60, 60, 60, Color.Pink);
            graphics.Show();

            //renderer.Clear();
            //var screen = GameService.Instance.CurrentScreen;
            //if(screen != null)
            //{
            //    var sprites = screen.AccessSpritesForRenderingOnly();
            //    for (var i = 0; i < sprites.Count; i++)
            //    {
            //        renderer.DrawSprite(sprites[i]);
            //    }
            //}
            //renderer.Render();
        }
    }
}
