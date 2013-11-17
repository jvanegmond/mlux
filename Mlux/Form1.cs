using System;
using System.Drawing;
using System.Windows.Forms;
using Mlux.Lib.Display;
using Mlux.Lib.Time;
using NLog;
using Timer = System.Threading.Timer;


namespace Mlux
{
    public partial class Form1 : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly Monitor AllMonitors = new Monitor();

        private readonly TimeProfile _profile;
        private readonly Timer _checkTimer;

        private byte _originalBrightness;
        private DateTime _disabledSince = DateTime.MinValue;
        private NotifyIcon _trayIcon;

        public Form1()
        {
            Log.Debug("Loading Form1");

            this.Load += OnLoad;
            this.Closing += OnClosing;
            Application.ApplicationExit += ApplicationExit;

            InitializeComponent();

            _profile = LoadProfile();

            _checkTimer = new Timer(TimerCallBack, null, 500, 10000);

            LoadTray();
        }

        void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visible = false;
            e.Cancel = true;
        }

        private void LoadTray()
        {
            Log.Debug("Loading tray icon");

            // Create a simple tray menu with only one item.
            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", TrayIconOnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            _trayIcon = new NotifyIcon();
            _trayIcon.Text = "Mlux Demo";
            _trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            _trayIcon.Click += TrayIconOnClick;

            // Add menu to tray icon and show it.
            _trayIcon.ContextMenu = trayMenu;
            _trayIcon.Visible = true;
        }

        void TrayIconOnClick(object sender, EventArgs e)
        {
            Log.Info("Tray icon click");
            this.Visible = !this.Visible;
        }

        private void TrayIconOnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void TimerCallBack(object a)
        {
            Log.Debug("TimerCallBack");

            var dateDiffSinceDisable = DateTime.Now - _disabledSince;
            if (dateDiffSinceDisable.TotalHours < 1)
            {
                Log.Debug("Mlux has been disabled since {0} ago", dateDiffSinceDisable);
                return;
            }
            else if (dateDiffSinceDisable.TotalHours >= 1 && dateDiffSinceDisable.TotalHours < 2)
            {
                Log.Debug("TimerCallBack will automatically re-enable because it's been over an hour");
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
                Log.Error(err);
            }
        }

        private static TimeProfile LoadProfile()
        {
            Log.Info("Loading profile");

            var result = new TimeProfile();

            var wakeUpTime = new TimeNode(TimeSpan.FromHours(7));
            wakeUpTime.Properties.Add(new NodeProperty(NodeProperty.Brightness, 20));
            wakeUpTime.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 3300));
            result.NodeManager.Nodes.Add(wakeUpTime);

            var morning = new TimeNode(TimeSpan.FromHours(8));
            morning.Properties.Add(new NodeProperty(NodeProperty.Brightness, 80));
            morning.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 6500));
            result.NodeManager.Nodes.Add(morning);

            var srsModeOver = new TimeNode(TimeSpan.FromHours(17));
            srsModeOver.Properties.Add(new NodeProperty(NodeProperty.Brightness, 80));
            srsModeOver.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 6500));
            result.NodeManager.Nodes.Add(srsModeOver);

            var afterDinner = new TimeNode(TimeSpan.FromHours(19));
            afterDinner.Properties.Add(new NodeProperty(NodeProperty.Brightness, 40));
            afterDinner.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 5000));
            result.NodeManager.Nodes.Add(afterDinner);

            var bedTime = new TimeNode(TimeSpan.FromHours(22));
            bedTime.Properties.Add(new NodeProperty(NodeProperty.Brightness, 20));
            bedTime.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 3300));
            result.NodeManager.Nodes.Add(bedTime);

            Log.Info("Profile loaded with {0} time nodes", result.NodeManager.Nodes.Count);

            return result;
        }

        void ApplicationExit(object sender, EventArgs e)
        {
            Log.Info("Exiting Mlux");

            _trayIcon.Dispose();
            _checkTimer.Dispose();

            if (checkBox1.Checked)
            {
                Log.Info("Restoring original brightness");

                try
                {
                    AllMonitors.SetBrightness(_originalBrightness);
                    AllMonitors.SetColorProfile(ColorAdjustment.Default);
                }
                catch (Exception err)
                {
                    Log.Error(err);
                    ShowError(err);
                }
            }
        }

        private static void ShowError(Exception err)
        {
            MessageBox.Show("I'm sorry! :(\r\n\r\n" + err.ToString(), "Mlux");
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Log.Debug("Form1 loaded");

            this.Visible = false;
            this.ShowInTaskbar = true;

            try
            {
                _originalBrightness = AllMonitors.GetBrightness();
            }
            catch (Exception err)
            {
                Log.Error(err);
                ShowError(err);
                return;
            }

            Log.Debug("Storing original monitor brightness {0}", _originalBrightness);

            trackBar1.Value = _originalBrightness;
            trackBar2.Value = trackBar2.Maximum;

            // Initialize the base gamma ramp with however the current gamma ramp is
            var ramp = AllMonitors.GetCurrentGammaRAMP();
            AllMonitors.SetBaseGammaRAMP(ramp);

            SetBrightness(_originalBrightness);
            SetColorTemperature(6500);
        }

        private void TrackBar1Scroll(object sender, EventArgs e)
        {
            var trackbar = sender as TrackBar;
            if (trackbar == null) return;

            Log.Debug("TrackBar1Scroll new value {0}", trackbar.Value);

            SetBrightness((byte)trackbar.Value);
        }

        private void TrackBar2Scroll(object sender, EventArgs e)
        {
            var trackbar = sender as TrackBar;
            if (trackbar == null) return;

            Log.Debug("TrackBar2Scroll new value {0}", trackbar.Value);

            var temperature = trackbar.Value * 100;

            SetColorTemperature(temperature);
        }

        private byte _lastBrightness = 0;
        private void SetBrightness(byte val)
        {
            if (_lastBrightness == val) return;
            _lastBrightness = val;

            Log.Debug("Setting monitor brightness to {0}", val);

            try
            {
                AllMonitors.SetBrightness(val);
                label1.BeginInvoke((Action)delegate()
                {
                    label1.Text = String.Format("Current value: {0}", val);
                    trackBar1.Value = (int)val;
                });
            }
            catch (Exception err)
            {
                Log.Error(err);
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

            Log.Debug("Setting monitor color temperature to {0}", temperature);

            try
            {
                AllMonitors.SetColorProfile(ColorTemperature.GetColorProfile(temperature));
                label2.BeginInvoke((Action)delegate()
                {
                    label2.Text = String.Format("Current value: {0}K", temperature);
                    trackBar2.Value = (int)temperature / 100;
                });
            }
            catch (Exception err)
            {
                Log.Error(err);
                ShowError(err);
            }
        }

        private void CheckBox2CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                _disabledSince = DateTime.Now;
                Log.Info("Disabling Mlux since {0}", _disabledSince);

                SetBrightness(_originalBrightness);
                SetColorTemperature(6500);
            }
            else
            {
                Log.Info("Enabling Mlux");

                _disabledSince = DateTime.MinValue;
                TimerCallBack(null);
            }
        }
    }
}
