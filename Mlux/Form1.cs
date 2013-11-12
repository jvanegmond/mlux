using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Mlux.Lib.Display;
using Mlux.Lib.Time;
using Timer = System.Threading.Timer;


namespace Mlux
{
    public partial class Form1 : Form
    {
        private byte _originalBrightness;
        private readonly TimeProfile profile;
        private readonly Timer checkTimer;
        private DateTime _disabledSince = DateTime.MinValue;
        private NotifyIcon _trayIcon;

        public Form1()
        {
            this.Load += new EventHandler(OnLoad);
            this.Closing += new System.ComponentModel.CancelEventHandler(OnClosing);
            Application.ApplicationExit += new EventHandler(ApplicationExit);

            InitializeComponent();

            profile = LoadProfile();

            checkTimer = new Timer(TimerCallBack, null, 500, 10000);

            LoadIcon();
        }

        void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visible = false;
            e.Cancel = true;
        }

        private void LoadIcon()
        {
            // Create a simple tray menu with only one item.
            ContextMenu trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", TrayIconOnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            _trayIcon = new NotifyIcon();
            _trayIcon.Text = "Mlux Demo";
            _trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            _trayIcon.Click += new EventHandler(TrayIconOnClick);

            // Add menu to tray icon and show it.
            _trayIcon.ContextMenu = trayMenu;
            _trayIcon.Visible = true;
        }

        void TrayIconOnClick(object sender, EventArgs e)
        {
            this.Visible = !this.Visible;
        }

        private void TrayIconOnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void TimerCallBack(object a)
        {
            var dateDiffSinceDisable = DateTime.Now - _disabledSince;
            if (dateDiffSinceDisable.TotalHours < 1)
            {
                return;
            }
            else if (dateDiffSinceDisable.TotalHours >= 1 && dateDiffSinceDisable.TotalHours < 2)
            {
                checkBox2.BeginInvoke((Action)delegate
                {
                    checkBox2.Checked = false;
                });
            }

            var time = DateTime.Now;

            var brightness = Convert.ToByte(profile.GetCurrentValue(time, NodeProperty.Brightness));
            var colorTemperature = Convert.ToInt32(profile.GetCurrentValue(time, NodeProperty.ColorTemperature));

            SetBrightness(brightness);
            SetColorTemperature(colorTemperature);
        }

        private static TimeProfile LoadProfile()
        {
            var result = new TimeProfile();

            var wakeUpTime = new TimeNode(TimeSpan.FromHours(7));
            wakeUpTime.Properties.Add(new NodeProperty(NodeProperty.Brightness, 20));
            wakeUpTime.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 3300));
            result.NodeManager.Nodes.Add(wakeUpTime);

            var morning = new TimeNode(TimeSpan.FromHours(8));
            morning.Properties.Add(new NodeProperty(NodeProperty.Brightness, 80));
            morning.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 6500));
            result.NodeManager.Nodes.Add(morning);

            var afterDinner = new TimeNode(TimeSpan.FromHours(18));
            afterDinner.Properties.Add(new NodeProperty(NodeProperty.Brightness, 80));
            afterDinner.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 6500));
            result.NodeManager.Nodes.Add(afterDinner);

            var bedTime = new TimeNode(TimeSpan.FromHours(22));
            bedTime.Properties.Add(new NodeProperty(NodeProperty.Brightness, 20));
            bedTime.Properties.Add(new NodeProperty(NodeProperty.ColorTemperature, 3300));
            result.NodeManager.Nodes.Add(bedTime);

            return result;
        }

        void ApplicationExit(object sender, EventArgs e)
        {
            _trayIcon.Dispose();
            checkTimer.Dispose();

            if (checkBox1.Checked)
            {
                Monitor.SetBrightness(_originalBrightness);
                Monitor.SetColorProfile(ColorProfile.Default);
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.Visible = false;
            this.ShowInTaskbar = true;

            _originalBrightness = Monitor.GetBrightness();
            trackBar1.Value = _originalBrightness;
            trackBar2.Value = trackBar2.Maximum;

            SetBrightness(_originalBrightness);
            SetColorTemperature(6500);
        }

        private void TrackBar1Scroll(object sender, EventArgs e)
        {
            var trackbar = sender as TrackBar;
            if (trackbar == null) return;

            SetBrightness((byte)trackbar.Value);
        }

        private void TrackBar2Scroll(object sender, EventArgs e)
        {
            var trackbar = sender as TrackBar;
            if (trackbar == null) return;

            var temperature = trackbar.Value * 100;

            SetColorTemperature(temperature);
        }

        private byte _lastBrightness = 0;
        private void SetBrightness(byte val)
        {
            if (_lastBrightness == val) return;
            _lastBrightness = val;

            try {
                Monitor.SetBrightness(val);
                label1.BeginInvoke((Action) delegate()
                {
                    label1.Text = String.Format("Current value: {0}", val);
                    trackBar1.Value = (int) val;
                });
            }
            catch {
                
            }
        }

        private double _lastTemperature = 0;
        private void SetColorTemperature(double temperature)
        {
            if (temperature < 3300) temperature = 3300;
            if (temperature > 6500) temperature = 6500;

            if (Math.Abs(_lastTemperature - temperature) < 1) return;
            _lastTemperature = temperature;
            try {
                Monitor.SetColorProfile(ColorTemperature.GetColorProfile(temperature));
                label2.BeginInvoke((Action) delegate()
                {
                    label2.Text = String.Format("Current value: {0}K", temperature);
                    trackBar2.Value = (int) temperature/100;
                });
            }
            catch {
                
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                _disabledSince = DateTime.Now;
                SetBrightness(_originalBrightness);
                SetColorTemperature(6500);
            }
            else
            {
                _disabledSince = DateTime.MinValue;
                TimerCallBack(null);
            }
        }
    }
}
