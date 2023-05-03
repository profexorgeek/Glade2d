using System;
using Meadow.Foundation.Graphics.Buffers;

namespace Glade2d.Graphics.Layers;

/// <summary>
/// Makes it easy for classes to create, order, and remove layers
/// </summary>
public class LayerManager
{
    private readonly TrackedLayers _trackedLayers = new();

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
    /// Removes all layers from the layer manager
    /// </summary>
    public void RemoveAllLayers()
    {
        _trackedLayers.RemoveAllLayers();
    }

    /// <summary>
    /// Renders all background layers to the specified buffer
    /// </summary>
    internal void RenderBackgroundLayers(BufferRgb565 buffer)
    {
        // Use the enumerator directly, to avoid garbage from IEnumerator
        var enumerator = _trackedLayers.BackgroundLayerEnumerator();
        while (enumerator.MoveNext())
        {
            enumerator.Current.Layer.RenderToBuffer(buffer);
        }
    }

    /// <summary>
    /// Renders all foreground layers to the specified buffer
    /// </summary>
    internal void RenderForegroundLayers(BufferRgb565 buffer)
    {
        // Use the enumerator directly, to avoid garbage from IEnumerator
        var enumerator = _trackedLayers.ForegroundLayerEnumerator();
        while (enumerator.MoveNext())
        {
            enumerator.Current.Layer.RenderToBuffer(buffer);
        }
    }
}