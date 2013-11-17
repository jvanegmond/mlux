using System;
using System.Management;
using System.Runtime.InteropServices;
using NLog;

namespace Mlux.Lib.Display
{
    public class Monitor
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

        public void SetBrightness(byte targetBrightness)
        {
            var scope = new ManagementScope("root\\WMI");
            var query = new SelectQuery("WmiMonitorBrightnessMethods");
            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                using (var objectCollection = searcher.Get())
                {
                    foreach (ManagementObject mObj in objectCollection)
                    {
                        mObj.InvokeMethod("WmiSetBrightness",
                            new Object[] { UInt32.MaxValue, targetBrightness });
                        break;
                    }
                }
            }
        }

        public byte GetBrightness()
        {
            // http://gyazo.com/2b2e6ecf1e827c77b3902a7c44670edc

            var scope = new ManagementScope("root\\WMI");
            var query = new SelectQuery("WmiMonitorBrightness");
            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                using (var objectCollection = searcher.Get())
                {
                    foreach (ManagementObject mObj in objectCollection)
                    {
                        return (byte)mObj.GetPropertyValue("CurrentBrightness");
                    }
                }
            }

            return byte.MaxValue;
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
