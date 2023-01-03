using System;
using System.Linq;

namespace Glade2d.Profiling;

internal class ProfileData
{
    private int _nextIndex;
    private bool _rotatedAtLeastOnce;
    private readonly long[] _elapsedTicks;

    public ProfileData(int capacity)
    {
        _nextIndex = 0;
        _rotatedAtLeastOnce = false;
        _elapsedTicks = new long[capacity];
    }

    public void AddTicks(long ticks)
    {
        _elapsedTicks[_nextIndex] = ticks;
        _nextIndex++;
        if (_nextIndex >= _elapsedTicks.Length)
        {
            _nextIndex = 0;
            _rotatedAtLeastOnce = true;
        }
    }

    public float GetAverageTicks()
    {
        var sum = _elapsedTicks.Sum();
        var length = _rotatedAtLeastOnce
            ? _elapsedTicks.Length
            : _nextIndex + 1;

        return (float)sum / length;
    }
}