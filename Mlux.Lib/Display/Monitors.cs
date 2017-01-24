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
    public class Monitors : ITemperatureMonitor
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly List<ITemperatureMonitor> _monitors;

        public Monitors(IEnumerable<ITemperatureMonitor> monitors)
        {
            _monitors = monitors.ToList();

            foreach (var monitor in _monitors)
            {
                monitor.CurrentChanged += Monitor_CurrentChanged;
            }
        }

        private void Monitor_CurrentChanged(object sender, EventArgs e)
        {
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }

        public static List<Monitor> Create()
        {
            var result = new List<Monitor>();

            var zeroDc = MonitorMethods.GetDC(IntPtr.Zero);
            MonitorMethods.EnumDisplayMonitors(zeroDc, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref Rectangle lprcMonitor, IntPtr dwData) =>
            {
                hdcMonitor = zeroDc; // Multi monitor bug? Can't get/set device gamma ramp with given hdcMonitor

                result.Add(new Monitor(hMonitor, hdcMonitor));
                return true;
            }, IntPtr.Zero);

            return result;
        }

        public IReadOnlyList<ITemperatureMonitor> GetMonitors()
        {
            return _monitors.AsReadOnly();
        }

        public event MonitorEvent CurrentChanged;
        public bool SupportBrightness => _monitors.Any(_ => _.SupportBrightness);

        public int GetBrightness()
        {
            return _monitors[0].GetBrightness();
        }

        public void SetBrightness(int brightness)
        {
            foreach (var monitor in _monitors)
            {
                monitor.SetBrightness(brightness);
            }
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

        public int GetTemperature()
        {
            return _monitors[0].GetTemperature();
        }

        public void SetTemperature(int temperature)
        {
            foreach (var monitor in _monitors)
            {
                monitor.SetTemperature(temperature);
            }
        }
    }
}
