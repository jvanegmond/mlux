using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mlux.Wpf;
using NLog;

namespace Mlux
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private MainWindow _mainWindow;

        public App()
        {
            bool startHidden = false;

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 0)
            {
                if (args.Any(_ => _.Equals("/startup")))
                {
                    startHidden = true;
                }
            }

            Log.Info("Mlux starting");

            Log.Info(() => $"Start hidden parameter: {startHidden}");

            _mainWindow = new MainWindow();
            if (startHidden)
            {
                _mainWindow.Hide();
            }
            else
            {
                _mainWindow.Show();
            }
        }
    }
}
