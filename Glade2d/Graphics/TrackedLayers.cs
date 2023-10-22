using System;
using System.Collections.Generic;

namespace Glade2d.Graphics;

internal class TrackedLayers
{
    internal readonly record struct TrackedLayer(ILayer Layer, int ZIndex);

    private readonly Dictionary<ILayer, int> _knownLayerZIndexes = new();
    private readonly SortedSet<TrackedLayer> _backgroundLayers = new(new TrackedLayerComparer());
    private readonly SortedSet<TrackedLayer> _foregroundLayers = new(new TrackedLayerComparer());
    
    /// <summary>
    /// Adds a layer to the scene graph at the specified index
    /// </summary>
    /// <param name="layer">The layer to add</param>
    /// <param name="zIndex">The Z index (which controls render order, higher is on top)</param>
    /// <exception cref="ArgumentNullException">Thrown if a bad layer is passed</exception>
    /// <exception cref="InvalidOperationException">Thrown if layers are added at Z 0, 
    /// which is reserved for sprite drawing</exception>
    public void AddLayer(ILayer layer, int zIndex)
    {
        if (layer == null) throw new ArgumentNullException(nameof(layer));
        
        if (zIndex == 0)
        {
            const string message = "Z index of 0 not allowed in AddLayer calls, as " +
                                   "it is reserved for sprite layers.";

            throw new InvalidOperationException(message);
        }
        
        // If the layer already exists, we want to update it's z index to the
        // new one being specified.
        if (_knownLayerZIndexes.ContainsKey(layer))
        {
            RemoveLayer(layer);
        }

        var newTrackedLayer = new TrackedLayer(layer, zIndex);
        if (zIndex < 0)
        {
            _backgroundLayers.Add(newTrackedLayer);
        }
        else 
        {
            _foregroundLayers.Add(newTrackedLayer);
        }
        
        _knownLayerZIndexes[layer] = zIndex;
    }

    /// <summary>
    /// Removes a layer from the scene graph
    /// </summary>
    /// <param name="layer">The layer to remove</param>
    /// <exception cref="ArgumentNullException">Thrown if a bad layer is passed</exception>
    public void RemoveLayer(ILayer layer)
    {
        if (layer == null) throw new ArgumentNullException(nameof(layer));
        
        if (_knownLayerZIndexes.TryGetValue(layer, out var oldIndex))
        {
            if(oldIndex > 0)
            {
                _foregroundLayers.Remove(new TrackedLayer(layer, oldIndex));
            }
            else
            {
                _backgroundLayers.Remove(new TrackedLayer(layer, oldIndex));
            }
        }
        _knownLayerZIndexes.Remove(layer);
    }

    /// <summary>
    /// Removes all layers from the scene graph
    /// </summary>
    public void RemoveAllLayers()
    {
        _backgroundLayers.Clear();
        _foregroundLayers.Clear();
        _knownLayerZIndexes.Clear();
    }

    /// <summary>
    /// Checks if the provided layer is already
    /// in the scene graph
    /// </summary>
    /// <param name="layer">The layer to check</param>
    /// <returns>True if this layer is already tracked in the scene graph</returns>
    public bool ContainsLayer(ILayer layer)
    {
        return _knownLayerZIndexes.ContainsKey(layer);
    }

    /// <summary>
    /// Returns all background layers in order by their z index
    /// </summary>
    public SortedSet<TrackedLayer>.Enumerator BackgroundLayerEnumerator()
    {
        return _backgroundLayers.GetEnumerator();
    }

    /// <summary>
    /// Returns all background layers in order by their z index
    /// </summary>
    public SortedSet<TrackedLayer>.Enumerator ForegroundLayerEnumerator()
    {
        return _foregroundLayers.GetEnumerator();
    }

    /// <summary>
    /// Used for sorting tracked layers by z index.
    /// </summary>
    private class TrackedLayerComparer : IComparer<TrackedLayer>
    {
        public int Compare(TrackedLayer x, TrackedLayer y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            
            // One layer can only be part of 1 zIndex, so if two tracked layers
            // for the same layer are passed in, consider them the same
            if (ReferenceEquals(x.Layer, y.Layer)) return 0;

            if (x.ZIndex != y.ZIndex)
            {
                return x.ZIndex.CompareTo(y.ZIndex);
            }
            
            // If the z indexes are the same, arbitrarily sort them but don't
            // consider them equal, as this can cause collections like
            // sorted sets to only contain one layer per z index, which isn't
            // what we want
            return x.GetHashCode().CompareTo(y.GetHashCode());
        }
    }
}