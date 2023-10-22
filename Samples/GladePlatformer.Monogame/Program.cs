using Glade2d;
using Glade2d.Graphics;
using Glade2d.Graphics.Layers;
using Glade2d.Profiling;
using GladePlatformer.Shared;
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

var textureManager = new TextureManager(Environment.CurrentDirectory);
var layerManager = new LayerManager();
var profiler = new Profiler();
var renderer = new Renderer(environment.Display, textureManager, layerManager, profiler, 2);

engine.Initialize(renderer, textureManager, layerManager, profiler, input);

GladePlatformerGame.Run(engine);
