using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mlux.Lib.Display
{
    public class LerpedValuesMonitor : ITemperatureMonitor, IDisposable
    {
        private readonly ITemperatureMonitor _monitor;
        private Thread _thread;
        private readonly CancellationTokenSource _cancelSource;
        private readonly AutoResetEvent _runNow = new AutoResetEvent(false);
        private int _destinationBrightness = -1;
        private int _destinationTemperature = -1;
        private const int MaxBrightnessDeltaPerSecond = 5;
        private const int MaxTemperatureDeltaPerSecond = 50;

        public bool SupportBrightness => _monitor.SupportBrightness;

        public LerpedValuesMonitor(ITemperatureMonitor monitor)
        {
            _monitor = monitor;

            _cancelSource = new CancellationTokenSource();
            _thread = new Thread(LerpedValuesMonitorMainThread)
            {
                Name = ""
            };
            _thread.Start();
        }

        private void LerpedValuesMonitorMainThread()
        {
            var cancel = _cancelSource.Token;

            while (true)
            {
                if (_destinationBrightness != -1)
                {
                    var currentBrightness = _monitor.GetBrightness();
                    var newBrightness = Lerp(currentBrightness, _destinationBrightness, MaxBrightnessDeltaPerSecond);
                    _monitor.SetBrightness(newBrightness);
                }

                if (_destinationBrightness != -1)
                {
                    var currentTemperature = _monitor.GetTemperature();
                    var newTemperature = Lerp(currentTemperature, _destinationTemperature, MaxTemperatureDeltaPerSecond);
                    _monitor.SetTemperature(newTemperature);
                }

                var result = WaitHandle.WaitAny(new[] { cancel.WaitHandle, _runNow }, TimeSpan.FromSeconds(0.1));

                if (result == 0) break; // Stop
            }
        }

        private static int Lerp(int current, int dest, int maxDelta)
        {
            var delta = dest - current; // yields the total distance to travel with correct sign
            var step = Math.Min(maxDelta, Math.Abs(delta)); // clamps distance to maximum with positive sign fixed
            if (delta < 0) step *= -1; // corrects signage of step
            return (current + step); // applies step to current value
        }

        public int GetBrightness()
        {
            return _monitor.GetBrightness();
        }

        public void SetBrightness(int brightness)
        {
            _destinationBrightness = brightness;
            _runNow.Set();
        }

        public void SetColorProfile(ColorAdjustment adjustment)
        {
            _monitor.SetColorProfile(adjustment);
            _runNow.Set();
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
            return _monitor.GetTemperature();
        }

        public void SetTemperature(int temperature)
        {
            _destinationTemperature = temperature;
        }

        public void Dispose()
        {
            _cancelSource.Cancel();
        }
    }
}
