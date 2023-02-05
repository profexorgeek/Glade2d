using Glade2d;
using Glade2d.Graphics.Layers;
using Glade2d.Screens;
using Glade2d.Services;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace GladeQa.Shared.Screens;

public class LayerRenderTestScreen : Screen, IDisposable
{
    private readonly Layer _testLayer;

    public LayerRenderTestScreen()
    {
        _testLayer = CreateTestLayer();
    }

    private static Layer CreateTestLayer()
    {
        var textureManager = GameService.Instance.GameInstance.TextureManager;

        var texture = textureManager.GetTexture("layertest.bmp");

        var layer = Layer.Create(new Dimensions(texture.Width, texture.Height));
        layer.BackgroundColor = Color.Black;
        layer.TransparentColor = Color.Black;
        layer.Clear();
        
        layer.DrawTexture(texture, 
            new Point(), 
            new Point(), 
            new Dimensions(texture.Width, texture.Height));
        
        GameService.Instance.GameInstance.LayerManager.AddLayer(layer, -1);

        return layer;
    }

    public void Dispose()
    {
        GameService.Instance.GameInstance.LayerManager.RemoveLayer(_testLayer);
    }
}