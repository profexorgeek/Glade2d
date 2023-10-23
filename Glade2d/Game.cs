using System;
using Glade2d.Graphics;
using Glade2d.Screens;
using Glade2d.Services;
using System.Threading;
using Glade2d.Input;
using Glade2d.Profiling;

namespace Glade2d
{
    public class Game
    {
        /// <summary>
        /// The engine operating mode, this can currently only be set
        /// during Initialize
        /// </summary>
        public EngineMode Mode { get; private set; } = EngineMode.GameLoop;

        /// <summary>
        /// How long the engine sleeps between frames
        /// </summary>
        public int SleepMilliseconds { get; set; } = 0;

        /// <summary>
        /// The renderer instance. Used to configure extra rendering parameters,
        /// such as turning on performance information
        /// </summary>
        public IRenderer Renderer { get; private set; }

        /// <summary>
        /// The single instance for the input manager. 
        /// </summary>
        public InputManager InputManager { get; } = new();

        /// <summary>
        /// The profiler instance to track performance metrics
        /// </summary>
        public Profiler Profiler { get; private set; }

        public LayerManager LayerManager { get; private set; }

        public TextureManager TextureManager { get; private set; }

        public Game() { }

        public virtual void Initialize(
            IRenderer renderer,
            TextureManager textureManager,
            LayerManager layerManager,
            Profiler profiler,
            EngineMode mode = EngineMode.GameLoop)
        {
            Renderer = renderer;
            TextureManager = textureManager;
            Profiler = profiler;
            LayerManager = layerManager;
            
            // register ourselves with the game service
            GameService.Instance.GameInstance = this;

            Mode = mode;
        }

        public virtual void Initialize<TInput>(
            IRenderer renderer,
            TextureManager textureManager,
            LayerManager layerManager,
            Profiler profiler,
            TInput gameInput,
            EngineMode mode = EngineMode.GameLoop) where TInput : GameInputSetBase
        {
            Initialize(renderer, textureManager, layerManager, profiler, mode);
            gameInput.SetupInput(InputManager);
        }

        public void Start(Func<Screen> firstScreenGenerator = null)
        {
            GameService.Instance.Initialize();
            TransitionToScreen(firstScreenGenerator);

            LogService.Log.Trace("Starting Glade2d Game loop.");

            if (Mode == EngineMode.GameLoop)
            {
                while (true)
                {
                    Tick();
                    Thread.Sleep(SleepMilliseconds);
                }
            }
        }

        public void TransitionToScreen(Func<Screen> nextScreenGenerator)
        {
            if (GameService.Instance.CurrentScreen is IDisposable oldScreen)
            {
                oldScreen.Dispose();
            }
            
            GameService.Instance.GameInstance.LayerManager.RemoveAllLayers();
            Profiler.Reset();
            
            // We want to make sure we instantiate the screen *after* we've cleared
            // everything from the prior screen first.
            var screen = nextScreenGenerator?.Invoke() ?? new Screen();
            
            GameService.Instance.CurrentScreen = screen;
        }

        /// <summary>
        /// Performs an Update and then a Draw.
        /// This method can be called manually to tick
        /// the engine in RenderOnDemand mode
        /// </summary>
        public void Tick()
        {
            InputManager.Tick();
            Profiler.StartTiming("Game.Update");
            Update();
            Profiler.StopTiming("Game.Update");
            
            Profiler.StartTiming("Game.Draw");
            Draw();
            Profiler.StopTiming("Game.Draw");
            
            Profiler.WriteReport();
        }

        /// <summary>
        /// Updates all currently-active entities (entities
        /// which are children of the CurrentScreen)
        /// </summary>
        public void Update()
        {
            GameService.Instance.Time?.Update();
            GameService.Instance.CurrentScreen?.Update();
        }

        /// <summary>
        /// Draws all sprites in the CurrentScreen into a
        /// buffer and then blits the buffer to the hardware
        /// display
        /// </summary>
        public void Draw()
        {
            var sprites = GameService.Instance.CurrentScreen?.AccessSpritesForRenderingOnly();
            Renderer.RenderAsync(sprites);
        }
    }
}