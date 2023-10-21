using System;
using System.Collections;
using System.Collections.Generic;
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
    public void AddLayer(ILayer layer, int zIndex)
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
    /// Checks whether the provided layer is already
    /// managed by this manager as part of the scene graph
    /// </summary>
    /// <param name="layer">The layer to check</param>
    /// <returns>True if the LayerManager is already managing this layer</returns>
    public bool ContainsLayer(ILayer layer)
    {
        return _trackedLayers.ContainsLayer(layer);
    }

    /// <summary>
    /// Removes a layer from being actively rendered.
    /// </summary>
    public void RemoveLayer(ILayer layer)
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
    /// Retrieves the background layers in order
    /// </summary>
    internal LayerEnumerator BackgroundLayerEnumerator()
    {
        return new LayerEnumerator(_trackedLayers.BackgroundLayerEnumerator());
    }
   
    /// <summary>
    /// Retrieves the foreground layers in order
    /// </summary>
    /// <returns></returns>
    internal LayerEnumerator ForegroundLayerEnumerator()
    {
        return new LayerEnumerator(_trackedLayers.ForegroundLayerEnumerator());
    }

    internal struct LayerEnumerator : IEnumerator<ILayer>
    {
        private SortedSet<TrackedLayers.TrackedLayer>.Enumerator _inner;
        public ILayer Current => _inner.Current.Layer;
        object IEnumerator.Current => Current;
        
        public LayerEnumerator(SortedSet<TrackedLayers.TrackedLayer>.Enumerator inner)
        {
            _inner = inner;
        }

        public bool MoveNext()
        {
            return _inner.MoveNext();
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            _inner.Dispose();
        }


    }
}