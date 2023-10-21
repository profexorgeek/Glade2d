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
    private const float TreeSpeedMultiplier = 0.5f;
    private const float MountainSpeedMultiplier = 0.2f;
    
    public record LevelData(IReadOnlyList<byte> SectionHeights);
    private readonly record struct GroundSection(int Index, int LayerStartX);

    private readonly ILayer _groundLayer;
    private readonly LevelData _levelData;
    private readonly GroundChunk _ground;
    private readonly BufferRgb565 _sectionClearTexture;
    private readonly int _screenHeight;
    private readonly ILayer _skyLayer;
    private readonly ILayer _treeLayer;
    private readonly ILayer _mountainLayer;
    private float _lastPlayerPositionX;
    private float _movementSinceLastDraw;

    public LevelHandler(LevelData levelData)
    {
        _screenHeight = GameService.Instance.GameInstance.Renderer.Height;
        
        _levelData = levelData;
        _ground = new GroundChunk();
        _groundLayer = CreateGroundLayer(_ground);
        _skyLayer = CreateSkyLayer();
        _treeLayer = CreateTreeLayer();
        _mountainLayer = CreateMountainLayer();
        _sectionClearTexture = CreateSectionClearTexture();
        
        Console.WriteLine($"Layer: {_groundLayer.Width}x{_groundLayer.Height}");
        
        // Draw the initial level sections that are "visible" on the layer
        var (left, _) = GetSectionUnderPlayer(0);
        
        // Draw right sections
        var section = left;
        while (section.LayerStartX <= _groundLayer.Width)
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
        _groundLayer.Shift(new Vector2(-movedBy, 0));
        _treeLayer.Shift(new Vector2(-movedBy * TreeSpeedMultiplier, 0));
        _mountainLayer.Shift(new Vector2(-movedBy * MountainSpeedMultiplier, 0));
        _lastPlayerPositionX = playerPositionX;

        _movementSinceLastDraw += movedBy;
        if (Math.Abs(_movementSinceLastDraw) >= GroundChunk.ChunkWidth)
        {
            // We've moved at least a full tile width, so redraw the 2nd to last tile at edge.
            // We want to draw the 2nd to last tile instead of the last, because the last one
            // is most likely obscured by the edge of the layer. This guarantees we draw a 
            // complete ground tile.
            var (leftTile, _) = GetSectionUnderPlayer(playerPositionX);
            var totalTilesOnLayer = _groundLayer.Width / GroundChunk.ChunkWidth;

            GroundSection sectionToDraw;
            if (_movementSinceLastDraw < 0)
            {
                var layerStartX = leftTile.LayerStartX;
                var tileIndex = leftTile.Index;
                for (var x = 0; x < (totalTilesOnLayer / 2) - 1; x++)
                {
                    layerStartX -= GroundChunk.ChunkWidth;
                    tileIndex--;
                }

                sectionToDraw = new GroundSection(tileIndex, layerStartX);
            }
            else
            {
                var layerStartX = leftTile.LayerStartX;
                var tileIndex = leftTile.Index;
                for (var x = 0; x < (totalTilesOnLayer / 2) - 1; x++)
                {
                    layerStartX += GroundChunk.ChunkWidth;
                    tileIndex++;
                }

                sectionToDraw = new GroundSection(tileIndex, layerStartX);
            }
            
            DrawGround(sectionToDraw);
            _movementSinceLastDraw = 0;
        }
    }

    public void Dispose()
    {
        GameService.Instance.GameInstance.LayerManager.RemoveLayer(_groundLayer);
        GameService.Instance.GameInstance.LayerManager.RemoveLayer(_treeLayer);
        GameService.Instance.GameInstance.LayerManager.RemoveLayer(_mountainLayer);
        GameService.Instance.GameInstance.LayerManager.RemoveLayer(_skyLayer);
    }

    private static ILayer CreateGroundLayer(GroundChunk ground)
    {
        var renderer = GameService.Instance.GameInstance.Renderer;
        
        // Layer should be the same width as the screen, with enough area on
        // both sides for two and a half chunks as a buffer.
        var layerWidth = renderer.Width + (GroundChunk.ChunkWidth * 5);
        var layerHeight = ground.CurrentFrame.Height * MaxGroundStack;
        var layer = GameService.Instance.GameInstance.Renderer.CreateLayer(new Dimensions(layerWidth, layerHeight));
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

    private static ILayer CreateSkyLayer()
    {
        var screenWidth = GameService.Instance.GameInstance.Renderer.Width;
        var skyChunk = new SkyChunk();
        
        // Sky doesn't move, so it can be the same width of the screen
        var layer = GameService.Instance.GameInstance.Renderer.CreateLayer(new Dimensions(screenWidth, skyChunk.CurrentFrame.Height));
        layer.CameraOffset = new Point(0);
        
        GameService.Instance.GameInstance.LayerManager.AddLayer(layer, -1);
        for (var x = 0; x < screenWidth; x += skyChunk.CurrentFrame.Width)
        {
            layer.DrawTexture(skyChunk.CurrentFrame, new Point(x));
        }

        return layer;
    }

    private static ILayer CreateTreeLayer()
    {
        var screenWidth = GameService.Instance.GameInstance.Renderer.Width;
        var screenHeight = GameService.Instance.GameInstance.Renderer.Height;
        var tree = new Tree();
        var ground = new GroundChunk();
        
        // We want to make sure the layer is at least as wide as the screen, 
        // but also wide enough that it can tile with itself, so no seams show
        // when it scrolls. The trees will be staggered in two "depths", with
        // every other in front of the two surrounding to it.
        var layerWidth = screenWidth +
                         (screenWidth % (tree.CurrentFrame.Width / 2));
        
        var layer = GameService.Instance.GameInstance.Renderer.CreateLayer(new Dimensions(layerWidth, tree.CurrentFrame.Height));
        layer.CameraOffset = new Point( 0, screenHeight - tree.CurrentFrame.Height - ground.CurrentFrame.Height);
        layer.BackgroundColor = new Color(79, 84, 107); // Mountain color
        layer.Clear();
        
        GameService.Instance.GameInstance.LayerManager.AddLayer(layer, -2);
        
        // Draw background trees first
        for (var x = 0; x < layerWidth; x += tree.CurrentFrame.Width)
        {
            layer.DrawTexture(tree.CurrentFrame, new Point(x));
        }
        
        // Now draw the foreground trees
        for (var x = tree.CurrentFrame.Width / 2; x < layerWidth; x += tree.CurrentFrame.Width)
        {
            layer.DrawTexture(tree.CurrentFrame, new Point(x));
        }

        return layer;
    }

    private static ILayer CreateMountainLayer()
    {
        var screenWidth = GameService.Instance.GameInstance.Renderer.Width;
        var screenHeight = GameService.Instance.GameInstance.Renderer.Height;
        var mountain = new MountainChunk();
        
        // We want to make sure the layer is at least as wide as the screen, 
        // but also wide enough that it can tile with itself, so no seams show
        // when it scrolls. 
        var layerWidth = screenWidth + (screenWidth % (mountain.CurrentFrame.Width));
        
        var layer = GameService.Instance.GameInstance.Renderer.CreateLayer(new Dimensions(layerWidth, mountain.CurrentFrame.Height));
        layer.BackgroundColor = new Color(57, 120, 168);
        layer.CameraOffset = new Point( 0, screenHeight - 16 - mountain.CurrentFrame.Height);
        layer.Clear();
        
        GameService.Instance.GameInstance.LayerManager.AddLayer(layer, -3);

        for (var x = 0; x < layerWidth; x += mountain.CurrentFrame.Width)
        {
            layer.DrawTexture(mountain.CurrentFrame, new Point(x));
        }

        return layer;
    }

    private BufferRgb565 CreateSectionClearTexture()
    {
        var width = _ground.CurrentFrame.Width;
        var height = _ground.CurrentFrame.Height * MaxGroundStack;
        var buffer = new BufferRgb565(width, height);
        buffer.Fill(_groundLayer.TransparentColor);
        
        return buffer;
    }

    private (GroundSection left, GroundSection right) GetSectionUnderPlayer(float horizontalOffset)
    {
        var groundWidth = _ground.CurrentFrame.Width;
        var tileOffset = horizontalOffset / groundWidth;
        var leftTileIndex = (int)tileOffset;
        var pixelsFromLeftTileStart = (int)(horizontalOffset % groundWidth);
        var leftTileStart = _groundLayer.Width / 2 - pixelsFromLeftTileStart;
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
        var heightToClear = _groundLayer.Height - _ground.CurrentFrame.Height * MaxGroundStack;
        _groundLayer.DrawTexture(
            _sectionClearTexture,
            new Point(0),
            new Point(sectionToDraw.LayerStartX, heightToClear),
            new Dimensions(_sectionClearTexture.Width, _sectionClearTexture.Height),
            true); // ignore transparency for this draw call

        var height = GetSectionHeight(sectionToDraw.Index);
        for (var x = 0; x < height; x++)
        {
            var startX = sectionToDraw.LayerStartX;
            var startY = _groundLayer.Height - _ground.CurrentFrame.Height * (x + 1);
            _groundLayer.DrawTexture(_ground.CurrentFrame, new Point(startX, startY));
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