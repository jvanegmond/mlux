using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mlux.Lib;
using Mlux.Lib.Display;
using Mlux.Lib.Time;
using NLog;
using Monitor = Mlux.Lib.Display.Monitor;
using Timer = System.Threading.Timer;

namespace Mlux
{
    public partial class MainForm : Form
    {
        private bool _forceHide;
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly Monitors _allMonitors;

        private readonly TimeProfile _profile;
        private readonly Timer _checkTimer;
        
        private DateTime _disabledSince = DateTime.MinValue;
        private NotifyIcon _trayIcon;

        private const string _savedProfilePath = "profile.xml";

        public MainForm(bool startHidden)
        {
            _allMonitors = new Monitors();

            _forceHide = startHidden;
            _log.Debug("Loading Form1");

            Load += OnLoad;
            Closing += OnClosing;
            Application.ApplicationExit += ApplicationExit;

            InitializeComponent();

            _profile = LoadProfile();

            _checkTimer = new Timer(TimerCallBack, null, 500, 10000);

            LoadTray();
        }

        protected override void SetVisibleCore(bool value)
        {
            if (_forceHide)
            {
                base.SetVisibleCore(false);
                return;
            }
            base.SetVisibleCore(value);
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Visible = false;
            e.Cancel = true;
        }

        private void LoadTray()
        {
            _log.Debug("Loading tray icon");

            // Create a simple tray menu with only one item.
            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", TrayIconOnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            _trayIcon = new NotifyIcon
            {
                Text = "Mlux Demo",
                Icon = new Icon(SystemIcons.Application, 40, 40)
            };
            _trayIcon.Click += TrayIconOnClick;

            // Add menu to tray icon and show it.
            _trayIcon.ContextMenu = trayMenu;
            _trayIcon.Visible = true;
        }

        private void TrayIconOnClick(object sender, EventArgs e)
        {
            _log.Info("Tray icon click");

            _forceHide = false;
            Visible = !Visible;
            if (Visible) {
                Activate();
            }
        }

        private void TrayIconOnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void TimerCallBack(object a)
        {
            _log.Debug("TimerCallBack");

            var dateDiffSinceDisable = DateTime.Now - _disabledSince;
            if (dateDiffSinceDisable.TotalHours < 1)
            {
                _log.Debug("Mlux has been disabled since {0} ago", dateDiffSinceDisable);
                return;
            }
            else if (dateDiffSinceDisable.TotalHours >= 1 && dateDiffSinceDisable.TotalHours < 2)
            {
                _log.Debug("TimerCallBack will automatically re-enable because it's been over an hour");
                checkBox2.BeginInvoke((Action)delegate
                {
                    checkBox2.Checked = false;
                });
            }

            try
            {
                var time = DateTime.Now;

                var brightness = Convert.ToByte(_profile.GetCurrentValue(time, NodeProperty.Brightness));
                var colorTemperature = Convert.ToInt32(_profile.GetCurrentValue(time, NodeProperty.ColorTemperature));

                SetBrightness(brightness);
                SetColorTemperature(colorTemperature);
            }
            catch (Exception err)
            {
                _log.Error(err);
            }
        }

        private static TimeProfile LoadProfile()
        {
            _log.Info("Loading profile");

            TimeProfile result;
            if (File.Exists(_savedProfilePath)) {
                var savedProfileData = File.ReadAllText(_savedProfilePath, Encoding.UTF8);
                result = TimeProfileSerializer.Deserialize(savedProfileData);
            }
            else {
                result = TimeProfile.GetDefault();
            }

            _log.Info("Profile loaded with {0} time nodes", result.Nodes.Count);

            return result;
        }

        private void ApplicationExit(object sender, EventArgs e)
        {
            _log.Info("Exiting Mlux");

            if (_trayIcon != null) _trayIcon.Dispose();
            if (_checkTimer != null) _checkTimer.Dispose();

            _log.Info("Saving profile");

            var profileData = TimeProfileSerializer.Serialize(_profile);
            File.WriteAllText(_savedProfilePath, profileData, Encoding.UTF8);

            if (checkBox1.Checked)
            {
                _log.Info("Restoring original brightness");

                try
                {
                    lock (_allMonitors)
                    {
                        _allMonitors.Reset();
                    }
                }
                catch (Exception err)
                {
                    _log.Error(err);
                    ShowError(err);
                }
            }
        }

        private static void ShowError(Exception err)
        {
            MessageBox.Show("I'm sorry! :(\r\n\r\n" + err, "Mlux");
        }

        private void OnLoad(object sender, EventArgs e)
        {
            _log.Debug("Form1 loaded");
            
            ShowInTaskbar = true;

            trackBar1.Value = _allMonitors.GetBrightness();
            trackBar2.Value = trackBar2.Maximum;

            // Initialize the base gamma ramp with however the current gamma ramp is

            SetColorTemperature(6500);
        }

        private void TrackBar1Scroll(object sender, EventArgs e)
        {
            var trackbar = sender as TrackBar;
            if (trackbar == null) return;

            _log.Debug("TrackBar1Scroll new value {0}", trackbar.Value);

            SetBrightness((byte)trackbar.Value);
        }

        private void TrackBar2Scroll(object sender, EventArgs e)
        {
            var trackbar = sender as TrackBar;
            if (trackbar == null) return;

            _log.Debug("TrackBar2Scroll new value {0}", trackbar.Value);

            var temperature = trackbar.Value * 100;

            SetColorTemperature(temperature);
        }

        private byte _lastBrightness = 0;
        private void SetBrightness(byte val)
        {
            const int minChange = 5;

            val -= (byte)(val % minChange);

            if (_lastBrightness == val) return;
            _lastBrightness = val;

            _log.Debug("Setting monitor brightness to {0}", val);

            try
            {
                _allMonitors.SetBrightness(val);
                if (!Visible) return;
                label1.BeginInvoke((Action)delegate()
                {
                    label1.Text = $"Current value: {val}";
                    trackBar1.Value = (int)val;
                });
            }
            catch (Exception err)
            {
                _log.Error(err);
                ShowError(err);
            }
        }

        private double _lastTemperature = 0;
        private void SetColorTemperature(double temperature)
        {
            if (temperature < 3300) temperature = 3300;
            if (temperature > 6500) temperature = 6500;

            if (Math.Abs(_lastTemperature - temperature) < 1) return;
            _lastTemperature = temperature;

            _log.Debug("Setting monitor color temperature to {0}", temperature);

            try
            {
                _allMonitors.SetColorProfile(ColorTemperature.GetColorProfile(temperature), 129);
                if (!Visible) return;
                label2.BeginInvoke((Action)delegate()
                {
                    label2.Text = $"Current value: {temperature}K";
                    trackBar2.Value = (int)temperature / 100;
                });
            }
            catch (Exception err)
            {
                _log.Error(err);
                ShowError(err);
            }
        }

        private void CheckBox2CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                _disabledSince = DateTime.Now;
                _log.Info("Disabling Mlux since {0}", _disabledSince);

                _allMonitors.Reset();
            }
            else
            {
                _log.Info("Enabling Mlux");

                _disabledSince = DateTime.MinValue;
                TimerCallBack(null);
            }
        }

        private void Form1Deactivate(object sender, EventArgs e)
        {
            var timer = new Timer(delegate(object state)
            {
                BeginInvoke((Action)delegate
                {
                    if (!Focused) {
                        Visible = false;
                    }
                });
            }, null, 1000, Timeout.Infinite);
        }
    }
}
