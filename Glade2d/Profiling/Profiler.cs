using System.Collections.Generic;
using System.Diagnostics;

namespace Glade2d.Profiling;

/// <summary>
/// Makes it easy to track and report on performance characteristics about different operations
/// </summary>
public class Profiler
{
    private readonly Stopwatch _globalStopwatch = Stopwatch.StartNew();
    private readonly Dictionary<string, long> _startingTicks = new();
    private readonly Dictionary<string, List<long>> _elapsedTicks = new();
    private bool _isActive;
    private int _storedSampleSize = 100;

    /// <summary>
    /// Determines if the profiler should be actively tracking operations or not. If disabled most function calls
    /// do no work.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            if (!_isActive)
            {
                Reset();
            }
        }
    }

    /// <summary>
    /// Resets all profiling data. Usually done when profiling is enabled or a screen change.
    /// </summary>
    public void Reset()
    {
        _startingTicks.Clear();
        _elapsedTicks.Clear();
    }
}