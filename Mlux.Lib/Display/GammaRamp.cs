using System;
using System.Runtime.InteropServices;

namespace Mlux.Lib.Display
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GammaRamp
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public UInt16[] Red;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public UInt16[] Green;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public UInt16[] Blue;
    }
}