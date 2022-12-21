using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Glade2d.Profiling;

/// <summary>
/// Makes it easy to track and report on performance characteristics about different operations
/// </summary>
public class Profiler
{
    private const int StoredSampleSize = 100;
    private readonly Stopwatch _globalStopwatch = Stopwatch.StartNew();
    private readonly Dictionary<string, long> _startingTicks = new();
    private readonly Dictionary<string, ProfileData> _elapsedTicks = new();
    private readonly StringBuilder _reportString = new(500);
    private bool _isActive;
    private DateTime? _lastReportAt;

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
    /// How often profiling reports should be generated
    /// </summary>
    public TimeSpan TimeBetweenReports { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Resets all profiling data. Usually done when profiling is enabled or a screen change.
    /// </summary>
    public void Reset()
    {
        _startingTicks.Clear();
        _elapsedTicks.Clear();
        _lastReportAt = null;
    }

    /// <summary>
    /// Starts timing for a named timer. If start is called for the same timer before stop is called,
    /// then the first start time will be overwritten.
    /// </summary>
    public void StartTiming(string name)
    {
        _startingTicks[name] = _globalStopwatch.ElapsedTicks;
    }

    /// <summary>
    /// Stops timing for the specified named timer and catalogs the duration between start and stop calls. If no
    /// timer was started with this name than this is ignored.
    /// </summary>
    public void StopTiming(string name)
    {
        var endTicks = _globalStopwatch.ElapsedTicks;
        if (!_isActive || !_startingTicks.TryGetValue(name, out var startTicks) || startTicks > endTicks)
        {
            return;
        }

        var totalTicks = endTicks - startTicks;
        _startingTicks.Remove(name);
        
        if (!_elapsedTicks.TryGetValue(name, out var data))
        {
            data = new ProfileData(StoredSampleSize);
            _elapsedTicks.Add(name, data);
        }
        
        data.AddTicks(totalTicks);
    }

    /// <summary>
    /// Writes the profiling report to stdout
    /// </summary>
    public void WriteReport()
    {
        if (!_isActive)
        {
            return;
        }

        if (_lastReportAt == null)
        {
            _lastReportAt = DateTime.Now;
        }

        if (DateTime.Now - _lastReportAt < TimeBetweenReports)
        {
            return;
        }

        const long ticksPerMilli = TimeSpan.TicksPerMillisecond;
        _reportString.Clear();
        _reportString.AppendLine("Profiling timings:");
        foreach (var name in _elapsedTicks.Keys.OrderBy(x => x))
        {
            var averageTicks = _elapsedTicks[name].GetAverageTicks();
            var milliseconds = averageTicks / ticksPerMilli;
            _reportString.AppendLine($"{name}: {milliseconds}ms");
        }
        
        Console.WriteLine(_reportString);
        _lastReportAt = DateTime.Now;
    }
}