using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Mlux.Wpf;
using NLog;

namespace Mlux
{
    internal static class Program
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

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

            _log.Info("Mlux starting");

            _log.Info(() => $"Start hidden parameter: {startHidden}");

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm(startHidden));

            var window = new WindowManager();
            window.Start(startHidden);
        }
    }
}
