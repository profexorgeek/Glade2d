using Glade2d.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace Glade2d.Utility
{
    public class GameTime
    {
        Stopwatch stopwatch;

        public DateTime StartTime { get; set; }
        public double TotalElapsedSeconds { get; set; }
        public double LastFrameCompletedTime { get; set; }
        public double FrameSeconds { get; set; }
        public float FPS => 1f / (float)FrameSeconds;

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
            FrameSeconds = stopwatch.Elapsed.TotalSeconds;
            LastFrameCompletedTime = TotalElapsedSeconds;
            TotalElapsedSeconds += FrameSeconds;
            stopwatch.Restart();
        }
    }
}
