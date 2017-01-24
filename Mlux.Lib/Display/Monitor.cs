using System;
using System.Management;
using System.Runtime.InteropServices;
using NLog;

namespace Mlux.Lib.Display
{
    public class Monitor : ITemperatureMonitor
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const int DefaultGamma = 129;
        private readonly GammaRamp _linearGammaRamp;

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceGammaRamp(IntPtr hDc, ref GammaRamp lpGammaRamp);

        [DllImport("gdi32.dll")]
        public static extern bool SetDeviceGammaRamp(IntPtr hDc, ref GammaRamp lpGammaRamp);

        private readonly IntPtr _hMonitor;
        private readonly IntPtr _hdcMonitor;
        private readonly uint _physicalMonitorsCount = 1;
        private MonitorStructures.PhysicalMonitor[] _physicalMonitors;

        public event MonitorEvent CurrentChanged;
        public bool SupportBrightness { get; }
        public uint MinBrightness { get; private set; }
        public uint MaxBrightness { get; private set; }
        public int InitialBrightness { get; }

        public Monitor(IntPtr hMonitor, IntPtr hdcMonitor)
        {
            _hMonitor = hMonitor;
            _hdcMonitor = hdcMonitor;

            Log.Info($"Create monitor hMonitor:{_hMonitor} hdcMonitor:{_hdcMonitor}");

            // Get the current brightness
            try
            {
                SupportBrightness = true;

                if (!MonitorMethods.GetNumberOfPhysicalMonitorsFromHMONITOR(_hMonitor, ref _physicalMonitorsCount))
                {
                    throw new ApplicationException($"Cannot GetNumberOfPhysicalMonitorsFromHMONITOR for hmonitor {_hMonitor}");
                }

                _physicalMonitors = new MonitorStructures.PhysicalMonitor[_physicalMonitorsCount];

                if (!MonitorMethods.GetPhysicalMonitorsFromHMONITOR(_hMonitor, _physicalMonitorsCount, _physicalMonitors))
                {
                    throw new ApplicationException($"Cannot GetPhysicalMonitorsFromHMONITOR for hmonitor {_hMonitor}");
                }

                InitialBrightness = GetBrightness();

                foreach (var monitor in _physicalMonitors)
                {
                    Log.Info($"Monitor physicalmonitor handle:{monitor.hPhysicalMonitor} description:{monitor.szPhysicalMonitorDescription} with brightness {InitialBrightness}");
                }
            }
            catch (Exception err)
            {
                Log.Log(LogLevel.Error, err, "Unable to get brightness. Brightness will not be supported.");
                SupportBrightness = false;
            }

            // Get the current gamma Ramp
            try
            {
                _linearGammaRamp = GetDefaultGammaRamp();
                SetDeviceGammaRamp(_hdcMonitor, ref _linearGammaRamp);
            }
            catch (Exception err)
            {
                Log.Log(LogLevel.Error, err, "Unable to get base gamma ramp");
            }
        }

        public int GetBrightness()
        {
            if (!SupportBrightness) return 100;

            uint brightness = 0;
            uint minBrightness = 0;
            uint maxBrightness = 0;
            if (!MonitorMethods.GetMonitorBrightness(_physicalMonitors[0].hPhysicalMonitor, ref minBrightness, ref brightness, ref maxBrightness))
            {
                throw new ApplicationException("Unable to retrieve brightness through GetMonitorBrightness call.");
            }

            Log.Info($"Brightness {brightness} Max {maxBrightness} min {minBrightness}");

            MinBrightness = minBrightness;
            MaxBrightness = maxBrightness;
            return (int)brightness;
        }

        public void SetBrightness(int brightness)
        {
            if (!SupportBrightness) return;

            uint ubrightness = (uint)brightness;
            ubrightness = Math.Min(ubrightness, Math.Max(0, ubrightness));
            ubrightness = (MaxBrightness - MinBrightness) * ubrightness / 100u + MinBrightness;
            foreach (var physicalMonitor in _physicalMonitors)
            {
                Log.Info($"Set brightness {physicalMonitor.hPhysicalMonitor} to {brightness}");

                MonitorMethods.SetMonitorBrightness(physicalMonitor.hPhysicalMonitor, ubrightness);
            }
        }

        public static GammaRamp GetDefaultGammaRamp()
        {
            var result = new GammaRamp
            {
                Red = new ushort[256],
                Blue = new ushort[256],
                Green = new ushort[256]
            };

            for (var n = 0; n < 256; n++)
            {
                var value = (ushort)((n / 255d) * ushort.MaxValue);
                result.Red[n] = result.Blue[n] = result.Green[n] = value;
            }

            return result;
        }

        public void SetColorProfile(ColorAdjustment adjustment)
        {
            SetColorProfile(adjustment, DefaultGamma);
        }

        public void SetColorProfile(ColorAdjustment adjustment, int gamma)
        {
            Log.Info($"SetColorProfile {adjustment}, gamma {gamma}");

            if (gamma <= 256 && gamma >= 1)
            {
                var ramp = new GammaRamp { Red = new ushort[256], Green = new ushort[256], Blue = new ushort[256] };

                for (int i = 1; i < 256; i++)
                {

                    ushort r, g, b;
                    if (_linearGammaRamp.Red == null)
                    {
                        r = (ushort)(i * (gamma + 128));
                    }
                    else
                    {
                        r = _linearGammaRamp.Red[i];
                    }
                    if (_linearGammaRamp.Blue == null)
                    {
                        b = (ushort)(i * (gamma + 128));
                    }
                    else
                    {
                        b = _linearGammaRamp.Blue[i];
                    }
                    if (_linearGammaRamp.Green == null)
                    {
                        g = (ushort)(i * (gamma + 128));
                    }
                    else
                    {
                        g = _linearGammaRamp.Green[i];
                    }

                    ramp.Red[i] = (ushort)(r * adjustment.Red);
                    ramp.Green[i] = (ushort)(g * adjustment.Green);
                    ramp.Blue[i] = (ushort)(b * adjustment.Blue);
                }

                var success = SetDeviceGammaRamp(_hdcMonitor, ref ramp);
                Log.Debug($"SetDeviceGammaRamp {_hdcMonitor} with result {success}");
            }
        }

        public void Reset()
        {
            Log.Info($"{this} Start reset");

            if (SupportBrightness) SetBrightness(InitialBrightness);

            SetColorProfile(ColorAdjustment.Default);
        }

        public int GetTemperature()
        {
            throw new NotImplementedException("Hardware monitor doesn't know about temperature");
        }

        public void SetTemperature(int temperature)
        {
            throw new NotImplementedException("Hardware monitor doesn't know about temperature");
        }
    }
}
