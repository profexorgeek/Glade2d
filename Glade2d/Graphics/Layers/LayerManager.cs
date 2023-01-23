using System;
using System.Collections.Generic;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;

namespace Glade2d.Graphics.Layers;

/// <summary>
/// Makes it easy for classes to create, order, and remove layers
/// </summary>
public class LayerManager
{
    private readonly RotationType _rotation;
    private readonly TrackedLayers _trackedLayers = new();

    public LayerManager(RotationType rotation)
    {
        _rotation = rotation;
    }

    /// <summary>
    /// Creates a new layer and optionally adds it as an active layer
    /// </summary>
    /// <param name="dimensions">The size of the drawable area for the layer</param>
    /// <param name="zIndex">
    /// A priority value for when this layer should be rendered compared to others. All layers will be
    /// rendered in ascending order based on z index values specified. Layers with a z index
    /// less than 0 will be rendered before sprites, and layers with a z index greater than
    /// zero will be rendered after sprites. Z index values of 0 are not allowed, as that is reserved
    /// for sprites.
    ///
    /// A z index of `null` indicates that the layer should not be added as an actively rendered
    /// layer.
    /// </param>
    /// <returns>The created layer</returns>
    public Layer Create(Dimensions dimensions, int? zIndex)
    {
        var layer = Layer.Create(dimensions, _rotation);
        if (zIndex != null)
        {
            AddLayer(layer, zIndex.Value);
        }

        return layer;
    }

    /// <summary>
    /// Adds a layer to be actively rendered. If the layer is already active, then it
    /// changes the z index for that specific layer.
    /// </summary>
    /// <param name="layer">The layer to add to the layering manager</param>
    /// <param name="zIndex">
    /// A priority value for when this layer should be rendered compared to others. All layers will be
    /// rendered in ascending order based on z index values specified. Layers with a z index
    /// less than 0 will be rendered before sprites, and layers with a z index greater than
    /// zero will be rendered after sprites. Z index values of 0 are not allowed, as that is reserved
    /// for sprites.
    /// </param>
    public void AddLayer(Layer layer, int zIndex)
    {
        if (zIndex == 0)
        {
            const string message = "Attempted to add a layer to z index of 0, " +
                                   "but that is reserved for sprite layers only.";

            throw new InvalidOperationException(message);
        }
        
        _trackedLayers.AddLayer(layer, zIndex);
    }

    /// <summary>
    /// Removes a layer from being actively rendered.
    /// </summary>
    public void RemoveLayer(Layer layer)
    {
        _trackedLayers.RemoveLayer(layer);
    }

    /// <summary>
    /// Renders all background layers to the specified buffer
    /// </summary>
    internal void RenderBackgroundLayers(BufferRgb565 buffer)
    {
        foreach (var layer in _trackedLayers.BackgroundLayers())
        {
            layer.RenderToBuffer(buffer);
        }
    }

    /// <summary>
    /// Renders all foreground layers to the specified buffer
    /// </summary>
    internal void RenderForegroundLayers(BufferRgb565 buffer)
    {
        foreach (var layer in _trackedLayers.ForegroundLayers())
        {
            layer.RenderToBuffer(buffer);
        }
    }
}