﻿using Glade2d;
using Glade2d.Services;
using GladePlatformer.Shared.Screens;

namespace GladePlatformer.Shared;

public class GladePlatformerGame
{
    public static void Run(Game engine)
    {
        LogService.Log.Trace("Running game...");
        engine.Start(() => new LevelScreen());
    }
}