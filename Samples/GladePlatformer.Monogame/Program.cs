﻿using Glade2d;
using GladePlatformer.Shared;
using Meadow.Foundation.Graphics;
using MeadowMgTestEnvironment;
using Microsoft.Xna.Framework.Input;

var environment = new TestEnvironment(240, 320);
var engine = new Game();
engine.Initialize(
    environment.Display, 
    2, 
    contentRoot: Environment.CurrentDirectory,
    displayRotation: RotationType.Default);

var inputManager = engine.InputManager;
environment.BindKey(Keys.Right, 
    () => inputManager.ButtonPushed(GameConstants.InputNames.Right),
    () => inputManager.ButtonReleased(GameConstants.InputNames.Right));

environment.BindKey(Keys.Left,
    () => inputManager.ButtonPushed(GameConstants.InputNames.Left),
    () => inputManager.ButtonReleased(GameConstants.InputNames.Left));

environment.BindKey(Keys.Up,
    () => inputManager.ButtonPushed(GameConstants.InputNames.Up),
    () => inputManager.ButtonReleased(GameConstants.InputNames.Up));

GladePlatformerGame.Run(engine);
