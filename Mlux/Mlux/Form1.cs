using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Mlux.Lib;


namespace Mlux
{
    public partial class Form1 : Form
    {
        byte originalBrightness;

        public Form1()
        {
            InitializeComponent();
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Monitor.SetBrightness(originalBrightness);
                Monitor.SetColorProfile(ColorProfile.Default);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            originalBrightness = Monitor.GetBrightness();
            trackBar1.Value = originalBrightness;
            trackBar2.Value = trackBar2.Maximum;
            label1.Text = String.Format("Current value: {0}", originalBrightness);
            label2.Text = String.Format("Current value: {0}K", "?");
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            var trackbar = sender as TrackBar;
            if (trackbar == null) return;

            Monitor.SetBrightness((byte)trackbar.Value);
            label1.Text = String.Format("Current value: {0}", trackbar.Value);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            var trackbar = sender as TrackBar;
            if (trackbar == null) return;

            var temperature = trackbar.Value * 100;

            Monitor.SetColorProfile(ColorTemperature.GetColorProfile(temperature));
            label2.Text = String.Format("Current value: {0}K", temperature);
        }
    }
}
