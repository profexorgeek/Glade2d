using Glade2d.Screens;
using Glade2d.Utility;
using Meadow.Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace Glade2d.Services
{
    public class GameService
    {
        static GameService instance;

        public static GameService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameService();
                }
                return instance;
            }
        }

        public GameTime Time { get; private set; }

        public Screen CurrentScreen { get; set; } = new Screen();

        public Random Random { get; set; } = new Random();

        public Game GameInstance { get; set; }

        private GameService() { }

        public void Initialize()
        {
            LogService.Log.Trace("Initializing GameService");
            Time = new GameTime();
        }
    }
}
