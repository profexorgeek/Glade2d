using Glade2d.Graphics;
using Glade2d.Screens;
using Glade2d.Services;
using Meadow.Foundation.Graphics;
using System.Threading;

namespace Glade2d
{
    public class Game
    {
        // The engine operating mode, this can only be set
        // during Initialize
        public EngineMode Mode { get; private set; } = EngineMode.GameLoop;

        // How long the engine should sleep between ticks
        // Default is 0, which runs as fast as possible
        public int SleepMilliseconds { get; set; } = 0;

        public Renderer Renderer { get; protected set; }

        public Game() { }

        public virtual void Initialize(IGraphicsDisplay display, int displayScale = 1, EngineMode mode = EngineMode.GameLoop)
        {
            LogService.Log.Trace("Initializing Renderer...");

            // register ourselves with the game service
            GameService.Instance.GameInstance = this;

            // init renderer
            Renderer = new Renderer(display, displayScale);

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
            Update();
            Draw();
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
                var sprites = screen.AccessSpritesForRenderingOnly();
                for (var i = 0; i < sprites.Count; i++)
                {
                    Renderer.DrawSprite(sprites[i]);
                }
            }
            Renderer.RenderToDisplay();
        }
    }
}
