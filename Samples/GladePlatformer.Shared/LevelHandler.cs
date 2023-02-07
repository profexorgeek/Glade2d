using Glade2d;
using Glade2d.Graphics.Layers;
using Glade2d.Services;
using GladePlatformer.Shared.Entities;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;

namespace GladePlatformer.Shared;

public class LevelHandler : IDisposable
{
    private const byte MaxGroundStack = 3;
    public record LevelData(IReadOnlyList<byte> SectionHeights);
    private readonly record struct GroundSection(int Index, int LayerStartX);

    private readonly Layer _layer;
    private readonly LevelData _levelData;
    private readonly GroundChunk _ground;
    private readonly BufferRgb565 _sectionClearTexture;
    private readonly int _screenHeight;
    private float _lastPlayerPositionX;
    private GroundSection _lastLeft, _lastRight;
    private byte _previousHighestHeight;

    public LevelHandler(LevelData levelData)
    {
        _screenHeight = GameService.Instance.GameInstance.Renderer.Height;
        
        _levelData = levelData;
        _ground = new GroundChunk();
        _layer = CreateLevelLayer(_ground);
        _sectionClearTexture = CreateSectionClearTexture();
        
        // Draw the initial level sections that are "visible" on the layer
        var (left, right) = GetSectionUnderPlayer(0);
        
        // Draw right sections
        var section = left;
        while (section.LayerStartX <= _layer.Width)
        {
            DrawGround(section);
            section = new GroundSection(
                section.Index + 1,
                section.LayerStartX + _ground.CurrentFrame.Width);
        }

        // Draw left sections
        section = new GroundSection(
            left.Index - 1,
            left.LayerStartX - _ground.CurrentFrame.Width);

        while (section.LayerStartX + _ground.CurrentFrame.Width >= 0)
        {
            DrawGround(section);
            section = new GroundSection(
                section.Index - 1,
                section.LayerStartX - _ground.CurrentFrame.Width);
        }

        _lastPlayerPositionX = 0;
        _lastLeft = left;
        _lastRight = right;
        _previousHighestHeight = Math.Max(GetSectionHeight(left.Index), GetSectionHeight(right.Index));
    }

    public void Update(ref float playerPositionX,
        ref float playerPositionY,
        ref float playerVelocityY)
    {
        var (newLeft, newRight) = GetSectionUnderPlayer(playerPositionX);
        var highestHeight = Math.Max(GetSectionHeight(newLeft.Index), GetSectionHeight(newRight.Index));
        var highestHeightY = _screenHeight - _ground.CurrentFrame.Height * highestHeight;
        
        // Check for collision into the ground
        if (playerPositionY > highestHeightY)
        {
            var previousHeightY = _screenHeight - _ground.CurrentFrame.Height * _previousHighestHeight;
            if (playerPositionY < previousHeightY)
            {
                // Player is above the new section but not of the old. This means the player
                // was trying to move into a taller section. So we move him back to the edge
                // of the section he was trying to move into.
                if (playerPositionX < _lastPlayerPositionX)
                {
                    playerPositionX = _lastLeft.Index * _ground.CurrentFrame.Width;
                }
                else
                {
                    playerPositionX = _lastRight.Index * _ground.CurrentFrame.Width - Player.FrameWidth;
                }
                
                newLeft = _lastLeft;
                newRight = _lastRight;
                highestHeight = _previousHighestHeight;
            }
            else
            {
                // Player is colliding with the ground directly below them
                playerPositionY = highestHeightY;
                playerVelocityY = 0;
            }
            
        }

        _lastLeft = newLeft;
        _lastRight = newRight;
        _previousHighestHeight = highestHeight;
        _lastPlayerPositionX = playerPositionX;
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
        
        GameService.Instance.GameInstance.LayerManager.AddLayer(layer, -1);

        return layer;
    }

    private BufferRgb565 CreateSectionClearTexture()
    {
        var width = _ground.CurrentFrame.Width;
        var height = _ground.CurrentFrame.Height * MaxGroundStack;
        var buffer = new BufferRgb565(width, height);
        buffer.Fill(_layer.TransparentColor);
        
        return buffer;
    }

    private (GroundSection left, GroundSection right) GetSectionUnderPlayer(float horizontalOffset)
    {
        var groundWidth = _ground.CurrentFrame.Width;
        var tileOffset = horizontalOffset / groundWidth;
        var leftTileIndex = (int)tileOffset;
        var pixelsFromLeftTileStart = (int)(horizontalOffset % groundWidth);
        var leftTileStart = _layer.Width / 2 - pixelsFromLeftTileStart;
        var leftTile = new GroundSection(leftTileIndex, leftTileStart);
        
        // Is the player overlapping a different tile on its right? We are assuming
        // the player can only span at most two tiles.
        var rightTile =
            pixelsFromLeftTileStart + Player.FrameWidth > groundWidth 
                ? new GroundSection(leftTileIndex + 1, leftTileStart + groundWidth) // multiple tiles
                : leftTile; // single tile

        return (leftTile, rightTile);
    }

    private void DrawGround(GroundSection sectionToDraw)
    {
        // Clear this ground section
        var heightToClear = _layer.Height - _ground.CurrentFrame.Height * MaxGroundStack;
        _layer.DrawTexture(
            _sectionClearTexture,
            new Point(0, 0),
            new Point(sectionToDraw.LayerStartX, heightToClear),
            new Dimensions(_sectionClearTexture.Width, _sectionClearTexture.Height),
            true); // ignore transparency for this draw call

        var height = GetSectionHeight(sectionToDraw.Index);
        for (var x = 0; x < height; x++)
        {
            var startX = sectionToDraw.LayerStartX;
            var startY = _layer.Height - _ground.CurrentFrame.Height * (x + 1);
            _layer.DrawTexture(_ground.CurrentFrame, new Point(startX, startY));
        }
    }

    private byte GetSectionHeight(int sectionIndex)
    {
        if (sectionIndex < 0 || sectionIndex >= _levelData.SectionHeights.Count())
        {
            // Out of defined level bounds, so no ground.
            return 0;
        }

        return Math.Min(_levelData.SectionHeights[sectionIndex], MaxGroundStack);
    }
}