using Glade2d.Screens;
using Glade2d.Services;
using Meadow.Foundation;

namespace Glade2dExample.Screens
{
    public class EmptyScreen : Screen
    {
        public EmptyScreen()
        {
            // set background color and FPS on the renderer
            var renderer = GameService.Instance.GameInstance.Renderer;
            renderer.ShowPerf = true;
            renderer.BackgroundColor = Color.Cyan;
        }
    }
}
