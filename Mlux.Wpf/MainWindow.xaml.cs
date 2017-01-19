using System;
using System.Collections.Generic;
using System.IO;
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
using Mlux.Lib;
using Mlux.Lib.Display;
using Mlux.Lib.Time;
using NLog;

namespace Mlux.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly Monitors _allMonitors;
        private const string _savedProfilePath = "profile.xml";

        private TimeProfile _profile;
        private TrayIcon _trayIcon;
        private SettingsWindow _settings;

        public MainWindow()
        {
            InitializeComponent();

            _allMonitors = new Monitors();
            Closed += MainWindow_Closed;

            _profile = LoadProfile();

            _trayIcon = new TrayIcon();
            _trayIcon.MainClick += _trayIcon_MainClick;
            _trayIcon.ExitClick += _trayIcon_ExitClick;
        }

        private void _trayIcon_ExitClick(TrayIcon icon, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
            // Close application
        }

        private void _trayIcon_MainClick(TrayIcon icon, EventArgs e)
        {
            // Toggle visibility
            if (WindowState != WindowState.Minimized)
            {
                WindowState = WindowState.Minimized;
                if (_settings != null) _settings.WindowState = WindowState.Minimized;
            }
            else
            {
                WindowState = WindowState.Normal;
                if (_settings != null) _settings.WindowState = WindowState.Normal;
            }
        }

        private static TimeProfile LoadProfile()
        {
            _log.Info("Loading profile");

            TimeProfile result;
            if (File.Exists(_savedProfilePath))
            {
                var savedProfileData = File.ReadAllText(_savedProfilePath, Encoding.UTF8);
                result = TimeProfileSerializer.Deserialize(savedProfileData);
            }
            else
            {
                result = TimeProfile.GetDefault();
            }

            _log.Info("Profile loaded with {0} time nodes", result.Nodes.Count);

            return result;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _trayIcon.Dispose();
            _allMonitors.Reset();
        }

        private void Open_settings_OnClick(object sender, RoutedEventArgs e)
        {
            if (_settings == null) _settings = new SettingsWindow();
            
            _settings.Profile = _profile;
            _settings.Show();
        }
    }
}
