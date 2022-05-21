using Meadow;
using System.Threading;

namespace Glade2d
{
    internal class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--exitOnDebug") return;

            // instantiate and run Glade2d
            app = new Glade2d();

            // instantiate and run micrographics test app
            // app = new MicroGraphicsTest();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
