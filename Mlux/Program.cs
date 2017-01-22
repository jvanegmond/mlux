using System;
using System.Collections.Generic;
using System.Linq;
using Mlux.Wpf;
using NLog;

namespace Mlux
{
    internal static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [STAThread]
        private static void Main(string[] args)
        {
            bool startHidden = false;

            if (args.Length > 0)
            {
                var firstParam = args[0];
                if (firstParam == "/startup")
                {
                    startHidden = true;
                }
            }

            Log.Info("Mlux starting");

            Log.Info(() => $"Start hidden parameter: {startHidden}");

            var window = new WindowManager();
            window.Start(startHidden);
        }
    }
}
