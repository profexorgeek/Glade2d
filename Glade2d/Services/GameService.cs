using Glade2d.Utility;
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

        private GameService() { }

        public void Initialize()
        {
            LogService.Log.Trace("Initializing GameService");
            Time = new GameTime();
        }


    }
}
