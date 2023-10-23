using System.Numerics;
using Glade2d;
using Glade2d.Graphics;
using Glade2d.Input;
using Glade2d.Screens;
using Glade2d.Services;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace GladeQa.Shared.Screens;

public class LayerRenderTestScreen : Screen, IDisposable
{
    private const float ShiftSpeed = 50f;
    private readonly ILayer _testLayer = CreateTestLayer();

    public override void Activity()
    {
        var frameDelta = (float)GameService.Instance.Time.FrameDelta;
        var inputManager = GameService.Instance.GameInstance.InputManager;
        var shiftAmount = new Vector2();
        
        // Pressing the up, left, or right buttons shifts the layer in those respective 
        // directions. Used to validate layer shifting works properly
        if (inputManager.GetButtonState(InputConstants.Up) == ButtonState.Down)
        {
            shiftAmount.Y += ShiftSpeed * frameDelta;
        }
        if (inputManager.GetButtonState(InputConstants.Right) == ButtonState.Down)
        {
            shiftAmount.X += ShiftSpeed * frameDelta;
        }
        if (inputManager.GetButtonState(InputConstants.Left) == ButtonState.Down)
        {
            shiftAmount.X -= ShiftSpeed * frameDelta;
        }
        
        // Pressing the down button re-draws the texture on the layer. This is used to
        // validate that drawing textures on a shifted layer works properly. If it works
        // properly then the texture should be re-drawn from the current shift position
        // as the origin. This should make the texture appear starting from 0,0 despite
        // the layer being shifted.
        if (inputManager.GetButtonState(InputConstants.Down) == ButtonState.Pressed)
        {
            Console.WriteLine("Redrawing texture");
            DrawTestImage(_testLayer);
        }
        
        _testLayer.Shift(shiftAmount);
    }

    public void Dispose()
    {
        GameService.Instance.GameInstance.LayerManager.RemoveLayer(_testLayer);
    }
    
    private static ILayer CreateTestLayer()
    {
        var textureManager = GameService.Instance.GameInstance.TextureManager;

        var texture = textureManager.GetTexture("layertest.bmp");

        var layer = GameService.Instance.GameInstance.Renderer.CreateLayer(new Dimensions(texture.Width, texture.Height));
        layer.BackgroundColor = Color.Purple;
        layer.TransparentColor = Color.Purple;
        layer.Clear();

        DrawTestImage(layer);
        
        GameService.Instance.GameInstance.LayerManager.AddLayer(layer, -1);

        return layer;
    }

    private static void DrawTestImage(ILayer layer)
    {
        var textureManager = GameService.Instance.GameInstance.TextureManager;
        var texture = textureManager.GetTexture("layertest.bmp");
        
        layer.DrawTexture(texture, 
            new Point(), 
            new Point(), 
            new Dimensions(texture.Width, texture.Height));
    }
}