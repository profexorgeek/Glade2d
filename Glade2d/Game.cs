using Glade2d.Graphics;
using Glade2d.Screens;
using Glade2d.Services;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace Glade2d
{
    public class Game
    {
        public Renderer Renderer { get; protected set; }

        public Game() { }

        public virtual void Initialize(IGraphicsDisplay display, int displayScale = 1)
        {
            LogService.Log.Trace("Initializing Renderer...");

            // register ourselves with the game service
            GameService.Instance.GameInstance = this;

            // init renderer
            Renderer = new Renderer(display, displayScale);

            LogService.Log.Trace("Renderer Initialized.");
        }

        public void Start(Screen startupScreen = null)
        {
            var screen = startupScreen ?? new Screen();

            GameService.Instance.Initialize();
            GameService.Instance.CurrentScreen = screen;

            LogService.Log.Trace("Starting Glade2d Game loop.");
            while(true)
            {
                Update();
                Draw();
            }
        }

        void Update()
        {
            GameService.Instance.Time?.Update();
            GameService.Instance.CurrentScreen?.Update();
        }

        void Draw()
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
