using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Mlux.Lib.Display
{
    public static class MonitorMethods
    {
        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rectangle lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
           MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum,
            ref MonitorStructures.DisplayDevice lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hmon, ref MonitorStructures.MonitorInfoEx mi);

        [DllImport("user32.dll", EntryPoint = "MonitorFromWindow", SetLastError = true)]
        public static extern IntPtr MonitorFromWindow([In] IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", EntryPoint = "MonitorFromPoint", SetLastError = true)]
        public static extern IntPtr MonitorFromPoint([In] Point pt, uint dwFlags);

        [DllImport("user32.dll", EntryPoint = "MonitorFromRect", SetLastError = true)]
        public static extern IntPtr MonitorFromRect(ref Rectangle lprect, uint dwFlags);


        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("dxva2.dll", EntryPoint = "GetNumberOfPhysicalMonitorsFromHMONITOR", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPhysicalMonitorsFromHMONITOR(
            IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] MonitorStructures.PhysicalMonitor[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitors", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyPhysicalMonitors(
            uint dwPhysicalMonitorArraySize, [Out] MonitorStructures.PhysicalMonitor[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitor", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyPhysicalMonitor(IntPtr hMonitor);

        [DllImport("dxva2.dll", EntryPoint = "GetMonitorTechnologyType", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorTechnologyType(
            IntPtr hMonitor, ref MonitorStructures.McDisplayTechnologyType pdtyDisplayTechnologyType);

        [DllImport("dxva2.dll", EntryPoint = "GetMonitorCapabilities", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorCapabilities(
            IntPtr hMonitor, ref uint pdwMonitorCapabilities, ref uint pdwSupportedColorTemperatures);

        [DllImport("dxva2.dll", EntryPoint = "SetMonitorBrightness", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetMonitorBrightness([In]IntPtr hMonitor, [In]uint pdwNewBrightness);

        [DllImport("dxva2.dll", EntryPoint = "GetMonitorBrightness", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorBrightness(
            [In]IntPtr hMonitor, ref uint pdwMinimumBrightness, ref uint pdwCurrentBrightness, ref uint pdwMaximumBrightness);

        [DllImport("dxva2.dll", EntryPoint = "SetMonitorContrast", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetMonitorContrast([In]IntPtr hMonitor, [In]uint pdwNewContrast);

        [DllImport("dxva2.dll", EntryPoint = "GetMonitorContrast", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorContrast(
            [In]IntPtr hMonitor, ref uint pdwMinimumContrast, ref uint pdwCurrentContrast, ref uint pdwMaximumContrast);

        [DllImport("dxva2.dll", EntryPoint = "GetMonitorRedGreenOrBlueDrive", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorRedGreenOrBlueDrive(
            [In]IntPtr hMonitor, [In]MonitorStructures.McDriveType dtDriveType,
            ref uint pdwMinimumDrive, ref uint pdwCurrentDrive, ref uint pdwMaximumDrive);

        [DllImport("dxva2.dll", EntryPoint = "GetMonitorRedGreenOrBlueGain", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorRedGreenOrBlueGain(
            [In]IntPtr hMonitor, [In]MonitorStructures.McGainType dtDriveType,
            ref uint pdwMinimumGain, ref uint pdwCurrentGain, ref uint pdwMaximumGain);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
    }

    public static class MonitorConstants
    {
        public const int MonitorDefaulttoprimary = 1;
        public const int MonitorDefaulttonearest = 2;
        public const int MonitorDefaulttonull = 0;

    }

    public static class MonitorStructures
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PhysicalMonitor
        {
            public IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MonitorInfoEx
        {
            public int cbSize;
            public Rect rcMonitor; // Total area  
            public Rect rcWork; // Working area  
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDeviceName;
        }

        public enum McDisplayTechnologyType
        {
            McShadowMaskCathodeRayTube,
            McApertureGrillCathodeRayTube,
            McThinFilmTransistor,
            McLiquidCrystalOnSilicon,
            McPlasma,
            McOrganicLightEmittingDiode,
            McElectroluminescent,
            McMicroelectromechanical,
            McFieldEmissionDevice,
        }

        public enum McMonitorCapabilities
        {
            McCapsNone = 0x00000000,
            McCapsMonitorTechnologyType = 0x00000001,
            McCapsBrightness = 0x00000002,
            McCapsContrast = 0x00000004,
            McCapsColorTemperature = 0x00000008,
            McCapsRedGreenBlueGain = 0x00000010,
            McCapsRedGreenBlueDrive = 0x00000020,
            McCapsDegauss = 0x00000040,
            McCapsDisplayAreaPosition = 0x00000080,
            McCapsDisplayAreaSize = 0x00000100,
            McCapsRestoreFactoryDefaults = 0x00000400,
            McCapsRestoreFactoryColorDefaults = 0x00000800,
            McRestoreFactoryDefaultsEnablesMonitorSettings = 0x00001000
        }

        public enum McDriveType
        {
            McRedDrive = 0,
            McGreenDrive = 1,
            McBlueDrive = 2
        }

        public enum McGainType
        {
            McRedGain = 0,
            McGreenGain = 1,
            McBlueGain = 2
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DisplayDevice
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [Flags()]
        public enum DisplayDeviceStateFlags : int
        {
            /// <summary>The device is part of the desktop.</summary>
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            /// <summary>The device is part of the desktop.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VgaCompatible = 0x16,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }
    }
}