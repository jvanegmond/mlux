using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NLog;

namespace Mlux
{
    static class Program
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        [STAThread]
        static void Main(string[] args)
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

            _log.Info(() => String.Format("Start hidden parameter: {0}", startHidden));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(startHidden));
        }
    }
}
