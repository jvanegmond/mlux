using System;
using System.Management;
using System.Runtime.InteropServices;
using NLog;

namespace Mlux.Lib.Display
{
    public class Monitor : IMonitor
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private const int _defaultGamma = 129;
        private readonly GammaRamp _baseGammaRamp;

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceGammaRamp(IntPtr hDc, ref GammaRamp lpGammaRamp);

        [DllImport("gdi32.dll")]
        public static extern bool SetDeviceGammaRamp(IntPtr hDc, ref GammaRamp lpGammaRamp);

        private readonly IntPtr _hMonitor;
        private readonly IntPtr _hdcMonitor;

        public bool SupportBrightness { get; }
        public uint MinBrightness { get; private set; }
        public uint MaxBrightness { get; private set; }
        public uint Brightness { get; private set; }
        public uint InitialBrightness { get; }

        public Monitor(IntPtr hMonitor, IntPtr hdcMonitor)
        {
            _hMonitor = hMonitor;
            _hdcMonitor = hdcMonitor;

            // Get the current brightness
            try
            {
                SupportBrightness = true; // Set this first otherwise GetBrightness returns without doing work
                GetBrightness();
                InitialBrightness = Brightness;
            }
            catch (Exception err)
            {
                _log.Log(LogLevel.Error, err, "Unable to get brightness");
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
                _log.Log(LogLevel.Error, err, "Unable to get base gamma ramp");
            }
        }

        public void SetBrightness(uint brightness)
        {
            if (!SupportBrightness) return;

            brightness = Math.Min(brightness, Math.Max(0, brightness));
            Brightness = (MaxBrightness - MinBrightness) * (uint)brightness / 100u + MinBrightness;
            MonitorMethods.SetMonitorBrightness(_hMonitor, Brightness);
        }

        public byte GetBrightness()
        {
            if (!SupportBrightness) return 0;

            uint brightness = 0;
            uint minBrightness = 0;
            uint maxBrightness = 0;
            if (!MonitorMethods.GetMonitorBrightness(_hMonitor, ref minBrightness, ref brightness, ref maxBrightness))
            {
                throw new ApplicationException("Unable to retrieve brightness through GetMonitorBrightness call.");
            }
            MinBrightness = minBrightness;
            MaxBrightness = maxBrightness;
            Brightness = brightness;
            return (byte)Brightness;
        }

        public GammaRamp GetCurrentGammaRamp()
        {
            var result = new GammaRamp();
            GetDeviceGammaRamp(_hdcMonitor, ref result);
            return result;
        }

        public void SetColorProfile(ColorAdjustment adjustment, int gamma = _defaultGamma)
        {
            if (_baseGammaRamp.Blue == null || _baseGammaRamp.Red == null || _baseGammaRamp.Green == null)
            {
                throw new ApplicationException("The monitors current gamma ramp is unavailable. Falling back to default gamma ramp.");
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

                SetDeviceGammaRamp(_hdcMonitor, ref ramp);
            }
        }

        public void Reset()
        {
            if (SupportBrightness) SetBrightness(InitialBrightness);
        }
    }
}
