using Glade2d.Services;
using System;
using System.Diagnostics;

namespace Glade2d.Utility
{
    public class GameTime
    {
        Stopwatch stopwatch;

        public DateTime StartTime { get; set; }
        public double TotalElapsedSeconds { get; set; }
        public double LastFrameCompletedTime { get; set; }
        public double FrameDelta { get; set; }
        public double FPS => 1.0 / (double)FrameDelta;

        public GameTime()
        {
            LogService.Log.Trace("Creating new GameTime object.");
            stopwatch = new Stopwatch();
            StartTime = DateTime.Now;
            TotalElapsedSeconds = 0;
            stopwatch.Start();
        }

        public void Update()
        {
            FrameDelta = stopwatch.Elapsed.TotalSeconds;
            LastFrameCompletedTime = TotalElapsedSeconds;
            TotalElapsedSeconds += FrameDelta;
            stopwatch.Restart();
        }
    }
}
