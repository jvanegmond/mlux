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
        static void Main()
        {
            _log.Info("Mlux starting");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
