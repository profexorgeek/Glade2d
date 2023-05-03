using System;
using System.Collections.Generic;

namespace Glade2d.Graphics.Layers;

internal class TrackedLayers
{
    internal readonly record struct TrackedLayer(Layer Layer, int ZIndex);

    private readonly Dictionary<Layer, int> _knownLayerZIndexes = new();
    private readonly SortedSet<TrackedLayer> _backgroundLayers = new(new TrackedLayerComparer());
    private readonly SortedSet<TrackedLayer> _foregroundLayers = new(new TrackedLayerComparer());
    
    public void AddLayer(Layer layer, int zIndex)
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

    public void RemoveLayer(Layer layer)
    {
        if (layer == null) throw new ArgumentNullException(nameof(layer));
        
        if (_knownLayerZIndexes.TryGetValue(layer, out var oldIndex))
        {
            _backgroundLayers.Remove(new TrackedLayer(layer, oldIndex));
        }

        _knownLayerZIndexes.Remove(layer);
    }

    public void RemoveAllLayers()
    {
        _backgroundLayers.Clear();
        _foregroundLayers.Clear();
        _knownLayerZIndexes.Clear();
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