using System;
using Glade2d.Graphics;
using Glade2d.Screens;
using Glade2d.Services;
using Meadow.Foundation.Graphics;
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
        public Renderer Renderer { get; private set; }

        /// <summary>
        /// The single instance for the input manager. 
        /// </summary>
        public InputManager InputManager { get; } = new();

        /// <summary>
        /// The profiler instance to track performance metrics
        /// </summary>
        public Profiler Profiler { get; } = new();

        public Game() { }

        public virtual void Initialize(IGraphicsDisplay display, int displayScale = 1, EngineMode mode = EngineMode.GameLoop, RotationType displayRotation = RotationType.Default)
        {
            LogService.Log.Trace("Initializing Renderer...");

            // register ourselves with the game service
            GameService.Instance.GameInstance = this;

            // init renderer
            Renderer = new Renderer(display, displayScale, displayRotation);

            Mode = mode;

            LogService.Log.Trace("Renderer Initialized.");
        }

        public void Start(Screen startupScreen = null)
        {
            Profiler.Reset();
            var screen = startupScreen ?? new Screen();

            GameService.Instance.Initialize();
            GameService.Instance.CurrentScreen = screen;

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
            Renderer.Reset();
            var screen = GameService.Instance.CurrentScreen;
            if (screen != null)
            {
                // TODO: this is a hack, figure out how to protect list
                // while also making it available to the renderer
                
                Profiler.StartTiming("Renderer.DrawSprites");
                var sprites = screen.AccessSpritesForRenderingOnly();
                for (var i = 0; i < sprites.Count; i++)
                {
                    Renderer.DrawSprite(sprites[i]);
                }
                Profiler.StopTiming("Renderer.DrawSprites");
            }
           
            Profiler.StartTiming("Renderer.RenderToDisplay");
            Renderer.RenderToDisplay();
            Profiler.StopTiming("Renderer.RenderToDisplay");
        }
    }
}