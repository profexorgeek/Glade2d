using System.Numerics;
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

    public LevelHandler(LevelData levelData)
    {
        _screenHeight = GameService.Instance.GameInstance.Renderer.Height;
        
        _levelData = levelData;
        _ground = new GroundChunk();
        _layer = CreateLevelLayer(_ground);
        _sectionClearTexture = CreateSectionClearTexture();
        
        Console.WriteLine($"Layer: {_layer.Width}x{_layer.Height}");
        
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
    }

    public void Update(ref float playerPositionX,
        ref float playerPositionY,
        ref float playerVelocityY)
    {
        var (newLeft, newRight) = GetSectionUnderPlayer(playerPositionX);
        var leftHeight = GetSectionHeight(newLeft.Index);
        var rightHeight = GetSectionHeight(newRight.Index);
        var highestHeight = Math.Max(leftHeight, rightHeight);
        var highestHeightY = _screenHeight - _ground.CurrentFrame.Height * highestHeight;
        var collisionPositionY = playerPositionY + Player.FrameHeight;
        
        // Check for collision into the ground
        if (collisionPositionY > highestHeightY)
        {
            Vector2 collisionAdjustment;
            var topOverlapAmount = collisionPositionY - highestHeightY;
            if (newLeft == newRight || leftHeight == rightHeight)
            {
                // Left and right ends are on the same section or same height. This is a 
                // pretty likely it's a collision from the top
                collisionAdjustment = new Vector2(0, -topOverlapAmount);
            }
            else if (leftHeight < rightHeight)
            {
                // We collided against the right, so check if the right side collided
                // further than the top
                var rightOverlapAmount = (Player.FrameWidth + playerPositionX) % GroundChunk.ChunkWidth;
                collisionAdjustment = topOverlapAmount < rightOverlapAmount
                    ? new Vector2(0, -topOverlapAmount) // move up
                    : new Vector2(-rightOverlapAmount, 0); // move left
            }
            else
            {
                // Left side collided
                var leftOverlapAmount = GroundChunk.ChunkWidth - (playerPositionX % GroundChunk.ChunkWidth);
                collisionAdjustment = topOverlapAmount < leftOverlapAmount
                    ? new Vector2(0, -topOverlapAmount) // move up
                    : new Vector2(leftOverlapAmount, 0); // move right
            }

            playerPositionY += collisionAdjustment.Y;
            playerPositionX += collisionAdjustment.X;

            if (collisionAdjustment.Y < 0)
            {
                // Since the player collided from above, we need to zero out their velocity
                playerVelocityY = 0;
            }
        }

        var movedBy = playerPositionX - _lastPlayerPositionX;
        _layer.Shift(new Vector2(-movedBy, 0));

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
        Console.WriteLine($"Drawing section {sectionToDraw.Index} height = {height}");
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