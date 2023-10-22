using Glade2d;
using Glade2d.Graphics;
using Glade2d.Graphics.Layers;
using Glade2d.Profiling;
using GladeInvade.Shared;
using MeadowMgTestEnvironment;
using Microsoft.Xna.Framework.Input;

var environment = new TestEnvironment(240, 240);
var engine = new Game();

var inputs = new GameInputs
{
    LeftButton = environment.CreatePortForKey(Keys.Left),
    RightButton = environment.CreatePortForKey(Keys.Right),
    ActionButton = environment.CreatePortForKey(Keys.Up),
};

var textureManager = new TextureManager(Environment.CurrentDirectory);
var layerManager = new LayerManager();
var profiler = new Profiler();
var renderer = new Renderer(environment.Display, textureManager, layerManager, profiler, 2);

engine.Initialize(renderer, textureManager, layerManager, profiler, inputs); 
GladeInvadeGame.Run(engine);
