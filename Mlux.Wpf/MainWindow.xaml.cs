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
using Mlux.Lib.Database;
using Mlux.Lib.Display;
using Mlux.Lib.Time;
using Mlux.Tray;
using Mlux.Wpf.Bindings;
using NLog;

namespace Mlux.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string SavedProfilePath = "profile.xml";

        private readonly TimeProfileView _profile;
        private readonly TrayIcon _trayIcon;
        private readonly TimeKeeper _timeKeeper;
        private readonly TimeNodeView _currentTimeNodeView;

        private SettingsWindow _settings;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            Closed += MainWindow_Closed;

            var profile = LoadProfile();
            _profile = new TimeProfileView(profile);

            _trayIcon = new TrayIcon();
            _trayIcon.MainClick += _trayIcon_MainClick;
            _trayIcon.ExitClick += _trayIcon_ExitClick;

            _timeKeeper = new TimeKeeper(profile);
            _timeKeeper.NodeElapsed += TimeKeeperNodeElapsed;
            _timeKeeper.CurrentChanged += TimeKeeperCurrentChanged;
            _timeKeeper.Start();

            _currentTimeNodeView = new TimeNodeView();
            CurrentNode.DataContext = _currentTimeNodeView;

            SetCurrentValues();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Open_settings_OnClick(this, new RoutedEventArgs());
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void SetCurrentValues()
        {
            // Show current brightness and color temperature
            _currentTimeNodeView.Brightness = _timeKeeper.CurrentBrightness;
            _currentTimeNodeView.Temperature = _timeKeeper.CurrentTemperature;

            // Show time remaining on current node in TimeOfDay spot
            _currentTimeNodeView.TimeOfDay = _timeKeeper.GetRemainingUntilNextNode(DateTime.Now);
        }

        private void TimeKeeperCurrentChanged(object sender, EventArgs e)
        {
            SetCurrentValues();
        }

        private void TimeKeeperNodeElapsed(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => NextNode.DataContext = _profile.Next(TimeProvider.Now));
        }

        private void _trayIcon_ExitClick(TrayIcon icon, EventArgs e)
        {
            // Close application
            this.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }

        private void _trayIcon_MainClick(TrayIcon icon, EventArgs e)
        {
            // Toggle visibility
            if (Visibility == Visibility.Visible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private static TimeProfile LoadProfile()
        {
            Log.Info("Loading profile");

            TimeProfile result;
            if (File.Exists(SavedProfilePath))
            {
                var savedProfileData = File.ReadAllText(SavedProfilePath, Encoding.UTF8);
                result = TimeProfileSerializer.Deserialize(savedProfileData);
            }
            else
            {
                result = DefaultTimeProfile.Create();
            }

            Log.Info("Profile loaded with {0} nodes", result.Nodes.Count);

            foreach (var node in result.Nodes)
            {
                Log.Info($"Node {node}");
            }

            return result;
        }

        public static void SaveProfile(TimeProfile profile)
        {
            var profileString = TimeProfileSerializer.Serialize(profile);

            File.WriteAllText(SavedProfilePath, profileString, Encoding.UTF8);

            Log.Info("Profile saved");

        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Log.Info("Main window closed");

            SaveProfile(_profile.UnderlyingProfile);

            _timeKeeper.Dispose();
            _trayIcon.Dispose();
        }

        private void Open_settings_OnClick(object sender, RoutedEventArgs e)
        {
            if (_settings == null || !_settings.IsLoaded) _settings = new SettingsWindow(_profile);
            _settings.Show();
        }

        private void NextNodeClick(object sender, RoutedEventArgs e)
        {
            _timeKeeper.Skip();
        }
    }
}
