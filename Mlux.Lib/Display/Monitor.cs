using System;
using System.Management;
using System.Runtime.InteropServices;
using NLog;

namespace Mlux.Lib.Display
{
    public class Monitor : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public const int DefaultGamma = 129;
        private RAMP _baseRamp;

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);

        [DllImport("gdi32.dll")]
        public static extern bool SetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct RAMP
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Red;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Green;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Blue;
        }

        private uint _physicalMonitorsCount = 0;
        private MonitorStructures.PHYSICAL_MONITOR[] _physicalMonitorArray;

        private IntPtr _hMonitor;

        private uint _minValue = 0;
        private uint _maxValue = 0;
        private uint _currentValue = 0;


        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        public Monitor()
        {
            var windowHandle = GetDesktopWindow();

            uint dwFlags = 0u;
            IntPtr ptr = MonitorMethods.MonitorFromWindow(windowHandle, dwFlags);
            if (!MonitorMethods.GetNumberOfPhysicalMonitorsFromHMONITOR(ptr, ref _physicalMonitorsCount))
            {
                throw new Exception("Cannot get monitor count!");
            }
            _physicalMonitorArray = new MonitorStructures.PHYSICAL_MONITOR[_physicalMonitorsCount];

            if (!MonitorMethods.GetPhysicalMonitorsFromHMONITOR(ptr, _physicalMonitorsCount, _physicalMonitorArray))
            {
                throw new Exception("Cannot get phisical monitor handle!");
            }
            _hMonitor = _physicalMonitorArray[0].hPhysicalMonitor;

            if (!MonitorMethods.GetMonitorBrightness(_hMonitor, ref _minValue, ref _currentValue, ref _maxValue))
            {
                throw new Exception("Cannot get monitor brightness!");
            }
        }

        public void SetBrightness(int newValue)
        {
            newValue = Math.Min(newValue, Math.Max(0, newValue));
            _currentValue = (_maxValue - _minValue) * (uint)newValue / 100u + _minValue;
            MonitorMethods.SetMonitorBrightness(_hMonitor, _currentValue);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_physicalMonitorsCount > 0)
                {
                    MonitorMethods.DestroyPhysicalMonitors(_physicalMonitorsCount, _physicalMonitorArray);
                }
            }
        }

        public byte GetBrightness()
        {
            MonitorMethods.GetMonitorBrightness(_hMonitor, ref _minValue, ref _currentValue, ref _maxValue);
            return (byte)_currentValue;
        }

        public RAMP GetCurrentGammaRAMP()
        {
            var result = new RAMP();
            GetDeviceGammaRamp(GetDC(IntPtr.Zero), ref result);
            return result;
        }

        public void SetBaseGammaRAMP(RAMP baseRamp)
        {
            this._baseRamp = baseRamp;
        }

        public void SetColorProfile(ColorAdjustment adjustment, int gamma = DefaultGamma)
        {
            if (_baseRamp.Blue == null || _baseRamp.Red == null || _baseRamp.Green == null)
            {
                Log.Warn("The monitors current gamma ramp is unavailable. Falling back to default gamma ramp.");
            }

            if (gamma <= 256 && gamma >= 1)
            {
                var ramp = new RAMP { Red = new ushort[256], Green = new ushort[256], Blue = new ushort[256] };

                for (int i = 1; i < 256; i++)
                {

                    ushort r, g, b;
                    if (_baseRamp.Red == null)
                    {
                        r = (ushort)(i * (gamma + 128));
                    }
                    else
                    {
                        r = _baseRamp.Red[i];
                    }
                    if (_baseRamp.Blue == null)
                    {
                        b = (ushort)(i * (gamma + 128));
                    }
                    else
                    {
                        b = _baseRamp.Blue[i];
                    }
                    if (_baseRamp.Green == null)
                    {
                        g = (ushort)(i * (gamma + 128));
                    }
                    else
                    {
                        g = _baseRamp.Green[i];
                    }

                    ramp.Red[i] = (ushort)(r * adjustment.Red);
                    ramp.Green[i] = (ushort)(g * adjustment.Green);
                    ramp.Blue[i] = (ushort)(b * adjustment.Blue);
                }

                SetDeviceGammaRamp(GetDC(IntPtr.Zero), ref ramp);
            }
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value > max) return max;
            if (value < min) return min;
            return value;
        }
    }
}
