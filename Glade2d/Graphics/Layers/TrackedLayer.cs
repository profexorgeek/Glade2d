using System;
using System.Collections.Generic;

namespace Glade2d.Graphics.Layers;

/// <summary>
/// A layer that is being actively tracked by the layer manager
/// </summary>
internal class TrackedLayer : IEquatable<TrackedLayer>
{
    public Layer Layer { get; }
    public int ZIndex { get; }

    public TrackedLayer(Layer layer, int zIndex)
    {
        Layer = layer;
        ZIndex = zIndex;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && 
               Equals((TrackedLayer)obj);
    }

    public bool Equals(TrackedLayer other)
    {
        if (ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || 
               Equals(Layer, other.Layer);
    }

    public override int GetHashCode()
    {
        return Layer.GetHashCode();
    }

    public static bool operator ==(TrackedLayer left, TrackedLayer right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TrackedLayer left, TrackedLayer right)
    {
        return !Equals(left, right);
    }

    public class Comparer : IComparer<TrackedLayer>
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