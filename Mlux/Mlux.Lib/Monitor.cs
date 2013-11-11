using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;

namespace Mlux.Lib
{
    public class Monitor
    {
        public const int DefaultGamma = 129;

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

        public static void SetBrightness(byte targetBrightness)
        {
            ManagementScope scope = new ManagementScope("root\\WMI");
            SelectQuery query = new SelectQuery("WmiMonitorBrightnessMethods");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                using (ManagementObjectCollection objectCollection = searcher.Get())
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

        public static byte GetBrightness()
        {

            ManagementScope scope = new ManagementScope("root\\WMI");
            SelectQuery query = new SelectQuery("WmiMonitorBrightness");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                using (ManagementObjectCollection objectCollection = searcher.Get())
                {
                    foreach (ManagementObject mObj in objectCollection)
                    {
                        return (byte)mObj.GetPropertyValue("CurrentBrightness");
                    }
                }
            }

            return byte.MaxValue;
        }

        public static void SetColorProfile(ColorProfile profile, int gamma = DefaultGamma)
        {
            if (gamma <= 256 && gamma >= 1)
            {
                RAMP ramp = new RAMP();
                ramp.Red = new ushort[256];
                ramp.Green = new ushort[256];
                ramp.Blue = new ushort[256];
                for (int i = 1; i < 256; i++)
                {
                    double newGamma = i * (gamma + 128);

                    if (newGamma > 65535)
                        newGamma = 65535;

                    ramp.Red[i] = ramp.Blue[i] = ramp.Green[i] = (ushort)newGamma;
                    ramp.Red[i] = (ushort)(newGamma * profile.Red);
                    ramp.Green[i] = (ushort)(newGamma * profile.Green);
                    ramp.Blue[i] = (ushort)(newGamma * profile.Blue);
                }

                SetDeviceGammaRamp(GetDC(IntPtr.Zero), ref ramp);
            }
        }

        public static ColorProfile GetColorProfile(int gamma = DefaultGamma)
        {
            var result = new ColorProfile();

            if (gamma <= 256 && gamma >= 1)
            {
                RAMP ramp = new RAMP();
                GetDeviceGammaRamp(GetDC(IntPtr.Zero), ref ramp);

                for (int i = 1; i < 256; i++)
                {
                    double newGamma = i * (gamma + 128);

                    if (newGamma > 65535)
                        newGamma = 65535;

                    result.Red = ramp.Red[i] / newGamma;
                    result.Green = ramp.Green[i] / newGamma;
                    result.Blue = ramp.Blue[i] / newGamma;

                    break;
                }
            }

            return result;
        }
    }
}
