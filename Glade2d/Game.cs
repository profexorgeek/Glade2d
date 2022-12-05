using System;
using System.Diagnostics;
using Glade2d.Graphics;
using Glade2d.Screens;
using Glade2d.Services;
using Meadow.Foundation.Graphics;
using System.Threading;

namespace Glade2d
{
    public class Game
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        
        /// <summary>
        /// The engine operating mode, this can currently only be set
        /// during Initialize
        /// </summary>
        public EngineMode Mode { get; private set; } = EngineMode.GameLoop;

        /// <summary>
        /// How long the engine 
        /// </summary>
        public int SleepMilliseconds { get; set; } = 0;

        /// <summary>
        /// The renderer instance. Used to configure extra rendering parameters,
        /// such as turning on performance information
        /// </summary>
        public Renderer Renderer { get; protected set; }

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
            _stopwatch.Restart();
            Update();
            var updateTime = _stopwatch.ElapsedMilliseconds;
            Draw();
            _stopwatch.Stop();
            var totalTime = _stopwatch.ElapsedMilliseconds;
            var drawTime = totalTime - updateTime;
            
            Console.WriteLine($"Update: {updateTime}ms, Draw: {drawTime}ms");
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
            var accessTime = TimeSpan.Zero;
            var drawTime = TimeSpan.Zero;
            var startTime = _stopwatch.Elapsed;
            Renderer.Reset();
            var resetTime = _stopwatch.Elapsed;
            var screen = GameService.Instance.CurrentScreen;
            if (screen != null)
            {
                // TODO: this is a hack, figure out how to protect list
                // while also making it available to the renderer
                var sprites = screen.AccessSpritesForRenderingOnly();
                accessTime = _stopwatch.Elapsed;
                for (var i = 0; i < sprites.Count; i++)
                {
                    Renderer.DrawSprite(sprites[i]);
                }

                drawTime = _stopwatch.Elapsed;
            }
            
            Renderer.RenderToDisplay();
            var totalTime = _stopwatch.Elapsed;
            
            Console.WriteLine($"Draw timings: " +
                              $"Reset: {(resetTime - startTime).TotalMilliseconds}ms " +
                              $"Access: {(accessTime - resetTime).TotalMilliseconds}ms " +
                              $"Draw: {(drawTime - accessTime).TotalMilliseconds}ms " +
                              $"Render: {(totalTime - drawTime).TotalMilliseconds}ms ");
            
        }
    }
}
