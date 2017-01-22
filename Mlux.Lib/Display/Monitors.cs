using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Mlux.Lib.Display
{
    public class Monitors : IMonitor
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public int Brightness
        {
            get { return _monitors[0].Brightness; }
            set
            {
                foreach (var monitor in _monitors)
                {
                    monitor.Brightness = value;
                }
            }
        }

        private readonly List<Monitor> _monitors = new List<Monitor>();

        public Monitors()
        {
            var zeroDc = MonitorMethods.GetDC(IntPtr.Zero);
            MonitorMethods.EnumDisplayMonitors(zeroDc, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref Rectangle lprcMonitor, IntPtr dwData) =>
            {
                hdcMonitor = zeroDc; // Multi monitor bug? Can't get/set device gamma ramp with given hdcMonitor

                _monitors.Add(new Monitor(hMonitor, hdcMonitor));
                return true;
            }, IntPtr.Zero);
        }

        public IReadOnlyList<Monitor> GetMonitors()
        {
            return _monitors.AsReadOnly();
        }

        public void SetColorProfile(ColorAdjustment adjustment)
        {
            foreach (var monitor in _monitors)
            {
                monitor.SetColorProfile(adjustment);
            }
        }

        public void SetColorProfile(ColorAdjustment adjustment, int gamma)
        {
            foreach (var monitor in _monitors)
            {
                monitor.SetColorProfile(adjustment, gamma);
            }
        }

        public void Reset()
        {
            foreach (var monitor in _monitors)
            {
                monitor.Reset();
            }
        }
    }
}
