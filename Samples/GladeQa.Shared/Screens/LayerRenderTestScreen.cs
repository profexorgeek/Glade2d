using System.Numerics;
using Glade2d;
using Glade2d.Graphics.Layers;
using Glade2d.Input;
using Glade2d.Screens;
using Glade2d.Services;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace GladeQa.Shared.Screens;

public class LayerRenderTestScreen : Screen, IDisposable
{
    private const float ShiftSpeed = 50f;
    private readonly Layer _testLayer;

    public LayerRenderTestScreen()
    {
        _testLayer = CreateTestLayer();
    }

    public override void Activity()
    {
        var frameDelta = (float)GameService.Instance.Time.FrameDelta;
        var inputManager = GameService.Instance.GameInstance.InputManager;
        var shiftAmount = new Vector2();
        if (inputManager.GetButtonState(InputConstants.Up) == ButtonState.Down)
        {
            shiftAmount.Y += ShiftSpeed * frameDelta;
        }
        if (inputManager.GetButtonState(InputConstants.Down) == ButtonState.Down)
        {
            shiftAmount.Y -= ShiftSpeed * frameDelta;
        }
        if (inputManager.GetButtonState(InputConstants.Right) == ButtonState.Down)
        {
            shiftAmount.X += ShiftSpeed * frameDelta;
        }
        if (inputManager.GetButtonState(InputConstants.Left) == ButtonState.Down)
        {
            shiftAmount.X -= ShiftSpeed * frameDelta;
        }
        
        _testLayer.Shift(shiftAmount);
    }

    public void Dispose()
    {
        GameService.Instance.GameInstance.LayerManager.RemoveLayer(_testLayer);
    }
    
    private static Layer CreateTestLayer()
    {
        var renderer = GameService.Instance.GameInstance.Renderer;
        var textureManager = GameService.Instance.GameInstance.TextureManager;

        var texture = textureManager.GetTexture("layertest.bmp");

        var layer = Layer.Create(new Dimensions(texture.Width, texture.Height));
        layer.BackgroundColor = Color.Black;
        layer.TransparentColor = Color.Black;
        layer.Clear();

        layer.CameraOffset = new Point(
            -((texture.Width - renderer.Width) / 2),
            -((texture.Height - renderer.Height) / 2));
        
        layer.DrawTexture(texture, 
            new Point(), 
            new Point(), 
            new Dimensions(texture.Width, texture.Height));
        
        GameService.Instance.GameInstance.LayerManager.AddLayer(layer, -1);

        return layer;
    }
}