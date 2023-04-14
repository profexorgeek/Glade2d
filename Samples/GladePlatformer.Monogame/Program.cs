using Glade2d;
using GladePlatformer.Shared;
using Meadow.Foundation.Graphics;
using MeadowMgTestEnvironment;
using Microsoft.Xna.Framework.Input;

var environment = new TestEnvironment(240, 320);
var engine = new Game();

var input = new GameInputs
{
    Left = environment.CreatePortForKey(Keys.Left),
    Right = environment.CreatePortForKey(Keys.Right),
    Jump = environment.CreatePortForKey(Keys.Up),
};

engine.Initialize(
    environment.Display, 
    input,
    2, 
    contentRoot: Environment.CurrentDirectory,
    displayRotation: RotationType.Default);

GladePlatformerGame.Run(engine);
