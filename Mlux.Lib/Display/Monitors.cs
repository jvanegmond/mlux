using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mlux.Lib.Display
{
    public class Monitors : IMonitor
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        private readonly List<Monitor> _monitors = new List<Monitor>();

        public Monitors()
        {
            MonitorMethods.EnumDisplayMonitors(GetDC(IntPtr.Zero), IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref Rectangle lprcMonitor, IntPtr dwData) =>
            {
                _monitors.Add(new Monitor(hMonitor, hdcMonitor));
                return true;
            }, IntPtr.Zero);
        }

        public IReadOnlyList<Monitor> GetMonitors()
        {
            return _monitors.AsReadOnly();
        }

        public byte GetBrightness()
        {
            return _monitors[0].GetBrightness();
        }

        public void SetBrightness(uint brightness)
        {
            foreach (var monitor in _monitors)
            {
                monitor.SetBrightness(brightness);
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
