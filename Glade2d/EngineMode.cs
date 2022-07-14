using System;
using System.Collections.Generic;
using System.Text;

namespace Glade2d
{
    public enum EngineMode
    {
        /// <summary>
        /// In GameLoop mode, Glade will start a gameloop
        /// and Update and Render as fast as possible with
        /// a configurable Sleep value
        /// </summary>
        GameLoop,

        /// <summary>
        /// In RenderOnDemand mode, Glade will initialize the
        /// renderer but will not perform work until Update,
        /// Draw, or Tick (update and draw) are called manually
        /// </summary>
        RenderOnDemand,
    }
}
