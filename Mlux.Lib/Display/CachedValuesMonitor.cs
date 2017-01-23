using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlux.Lib.Display
{
    public class CachedValuesMonitor : ITemperatureMonitor
    {
        private readonly ITemperatureMonitor _monitor;
        private int _lastTemperature;
        private int _lastBrightness;

        public bool SupportBrightness => _monitor.SupportBrightness;

        public CachedValuesMonitor(ITemperatureMonitor monitor)
        {
            _monitor = monitor;

            _lastBrightness = _monitor.GetBrightness();
            _lastTemperature = 6500;
        }

        public int GetBrightness()
        {
            return _lastBrightness;
        }

        public void SetBrightness(int brightness)
        {
            var changed = _lastBrightness != brightness;
            _lastBrightness = brightness;
            if (changed)
            {
                _monitor.SetBrightness(_lastBrightness);
            }
        }

        public void SetColorProfile(ColorAdjustment adjustment)
        {
            _monitor.SetColorProfile(adjustment);
        }

        public void SetColorProfile(ColorAdjustment adjustment, int gamma)
        {
            _monitor.SetColorProfile(adjustment, gamma);
        }

        public void Reset()
        {
            _monitor.Reset();
        }

        public int GetTemperature()
        {
            return _lastTemperature;
        }

        public void SetTemperature(int temperature)
        {
            var changed = _lastTemperature != temperature;
            _lastTemperature = temperature;
            if (changed)
            {
                SetColorProfile(ColorTemperature.GetColorProfile(_lastTemperature));
            }
        }
    }
}
