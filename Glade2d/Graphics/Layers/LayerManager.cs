using System.Collections.Generic;
using Meadow.Foundation.Graphics;

namespace Glade2d.Graphics.Layers;

/// <summary>
/// Makes it easy for classes to create, order, and remove layers
/// </summary>
public class LayerManager
{
    private readonly RotationType _rotation;
    private readonly SortedSet<TrackedLayer> _trackedLayers = new(new TrackedLayerComparer());

    public LayerManager(RotationType rotation)
    {
        _rotation = rotation;
    }

    public Layer CreateNew(Dimensions dimensions)
    {
        return Layer.Create(dimensions, _rotation);
    }
    
    
    /// <summary>
    /// Compares different tracked layers and ensures that tracked layers with a lower z index come first
    /// </summary>
    private class TrackedLayerComparer : IComparer<TrackedLayer>
    {
        public int Compare(TrackedLayer x, TrackedLayer y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return x.ZIndex.CompareTo(y.ZIndex);
        }
    }
}