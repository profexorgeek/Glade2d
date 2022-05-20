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

            // instantiate and run new meadow app
            app = new Glade2d();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
