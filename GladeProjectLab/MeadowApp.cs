﻿using Glade2d;
using Glade2d.Services;
using GladeSampleShared.Screens;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Gateways.Bluetooth;
using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace GladeProjectLab
{
    public class MeadowApp : App<F7FeatherV2>
    {
        IGraphicsDisplay display;
        Game glade;

        public override Task Run()
        {
            LogService.Log.Trace("Initializing Glade game engine...");
            glade = new Game();
            glade.Initialize(display, 4);

            LogService.Log.Trace("Running game...");
            glade.Start(new MountainSceneScreen());

            return base.Run();
        }

        public override Task Initialize()
        {
            LogService.Log.Trace("Initializing SPI bus...");
            var config = new SpiClockConfiguration(
                new Frequency(48000, Frequency.UnitType.Kilohertz),
                SpiClockConfiguration.Mode.Mode3);
            var spi = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.COPI, Device.Pins.CIPO, config);

            LogService.Log.Trace("Initializing MCP...");
            var mcp_in = Device.CreateDigitalInputPort(
                Device.Pins.D09,
                InterruptMode.EdgeRising,
                ResistorMode.InternalPullDown);
            var mcp_out = Device.CreateDigitalOutputPort(Device.Pins.D14);
            var mcp = new Mcp23008(Device.CreateI2cBus(), 0x20, mcp_in, mcp_out);

            LogService.Log.Trace("Initializing ST7789 display...");
            var chipSelectPort = mcp.CreateDigitalOutputPort(mcp.Pins.GP5);
            var dcPort = mcp.CreateDigitalOutputPort(mcp.Pins.GP6);
            var resetPort = mcp.CreateDigitalOutputPort(mcp.Pins.GP7);
            display = new St7789(
                spiBus: spi,
                chipSelectPort: chipSelectPort,
                dataCommandPort: dcPort,
                resetPort: resetPort,
                width: 240, height: 240,
                colorMode: ColorType.Format16bppRgb565
                );



            return base.Initialize();
        }
    }
}