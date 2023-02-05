using Glade2d;
using Glade2d.Graphics.Layers;
using Glade2d.Services;
using GladePlatformer.Shared.Entities;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace GladePlatformer.Shared;

public class LevelHandler : IDisposable
{
    private const int MaxGroundStack = 3;
    public record LevelData(IReadOnlyList<byte> TileHeights);

    private readonly Layer _layer;
    private readonly LevelData _levelData;
    private readonly GroundChunk _ground;
    private float _horizontalOffset;

    public LevelHandler(LevelData levelData)
    {
        _levelData = levelData;
        _ground = new GroundChunk();
        _layer = CreateLevelLayer(_ground);
    }

    public void Dispose()
    {
        GameService.Instance.GameInstance.LayerManager.RemoveLayer(_layer);
    }

    private static Layer CreateLevelLayer(GroundChunk ground)
    {
        var renderer = GameService.Instance.GameInstance.Renderer;
        
        // Layer should be double the width of the screen, and tall enough to
        // show the max number of stacked ground chunks
        var layerWidth = renderer.Width * 2;
        var layerHeight = ground.CurrentFrame.Height * MaxGroundStack;
        var layer = Layer.Create(new Dimensions(layerWidth, layerHeight));
        layer.TransparentColor = Color.Magenta;
        layer.BackgroundColor = Color.Magenta;
        layer.DrawLayerWithTransparency = true; // We want trees visible on lower ground stacks
        layer.Clear();
        
        // Layer should be horizontally centered to the camera, so we have an
        // equal buffer space on the left and right side
        var horizontalOffset = -((layerWidth - renderer.Width) / 2);
        var verticalOffset = renderer.Height - layerHeight;
        layer.CameraOffset = new Point(horizontalOffset, verticalOffset);

        return layer;
    }

    private void DrawLevelColumn(int column)
    {
        // Calculate location on the layer that this column should appear
        var columnsOffset = _horizontalOffset / _ground.CurrentFrame.Width;
        var layerStart = columnsOffset
    }
}