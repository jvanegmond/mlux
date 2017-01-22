using System;
using System.Management;
using System.Runtime.InteropServices;
using NLog;

namespace Mlux.Lib.Display
{
    public class Monitor : IMonitor
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const int DefaultGamma = 129;
        private readonly GammaRamp _baseGammaRamp;

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceGammaRamp(IntPtr hDc, ref GammaRamp lpGammaRamp);

        [DllImport("gdi32.dll")]
        public static extern bool SetDeviceGammaRamp(IntPtr hDc, ref GammaRamp lpGammaRamp);

        private readonly IntPtr _hMonitor;
        private readonly IntPtr _hdcMonitor;
        private readonly uint _physicalMonitorsCount = 1;
        private MonitorStructures.PhysicalMonitor[] _physicalMonitors;

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

                InitialBrightness = Brightness;

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
                var ramp = GetCurrentGammaRamp();
                _baseGammaRamp = ramp;
            }
            catch (Exception err)
            {
                Log.Log(LogLevel.Error, err, "Unable to get base gamma ramp");
            }
        }

        public int Brightness
        {
            get
            {
                if (!SupportBrightness) throw new ApplicationException("Setting brightness is not supported on this monitor");

                uint brightness = 0;
                uint minBrightness = 0;
                uint maxBrightness = 0;
                if (!MonitorMethods.GetMonitorBrightness(_physicalMonitors[0].hPhysicalMonitor, ref minBrightness, ref brightness, ref maxBrightness))
                {
                    throw new ApplicationException("Unable to retrieve brightness through GetMonitorBrightness call.");
                }

                MinBrightness = minBrightness;
                MaxBrightness = maxBrightness;
                return (int)brightness;
            }
            set
            {
                if (!SupportBrightness) throw new ApplicationException("Setting brightness is not supported on this monitor");

                if (Brightness == value) return;

                var brightness = (uint)value;
                brightness = Math.Min(brightness, Math.Max(0, brightness));
                brightness = (MaxBrightness - MinBrightness) * brightness / 100u + MinBrightness;
                foreach (var physicalMonitor in _physicalMonitors)
                {
                    MonitorMethods.SetMonitorBrightness(physicalMonitor.hPhysicalMonitor, brightness);
                }
            }
        }

        public GammaRamp GetCurrentGammaRamp()
        {
            var result = new GammaRamp();
            int success = GetDeviceGammaRamp(_hdcMonitor, ref result);
            Log.Debug($"GetDeviceGammaRamp handle:{_hdcMonitor} with result {success}");

            return result;
        }

        public void SetColorProfile(ColorAdjustment adjustment)
        {
            SetColorProfile(adjustment, DefaultGamma);
        }

        public void SetColorProfile(ColorAdjustment adjustment, int gamma)
        {
            if (_baseGammaRamp.Blue == null || _baseGammaRamp.Red == null || _baseGammaRamp.Green == null)
            {
                throw new ApplicationException("The monitors current gamma ramp is unavailable.");
            }

            if (gamma <= 256 && gamma >= 1)
            {
                var ramp = new GammaRamp { Red = new ushort[256], Green = new ushort[256], Blue = new ushort[256] };

                for (int i = 1; i < 256; i++)
                {

                    ushort r, g, b;
                    if (_baseGammaRamp.Red == null)
                    {
                        r = (ushort)(i * (gamma + 128));
                    }
                    else
                    {
                        r = _baseGammaRamp.Red[i];
                    }
                    if (_baseGammaRamp.Blue == null)
                    {
                        b = (ushort)(i * (gamma + 128));
                    }
                    else
                    {
                        b = _baseGammaRamp.Blue[i];
                    }
                    if (_baseGammaRamp.Green == null)
                    {
                        g = (ushort)(i * (gamma + 128));
                    }
                    else
                    {
                        g = _baseGammaRamp.Green[i];
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

            if (SupportBrightness) Brightness = InitialBrightness;

            SetColorProfile(ColorAdjustment.Default);
        }
    }
}
