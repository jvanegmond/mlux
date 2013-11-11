using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlux.Lib
{
    public struct ColorProfile
    {
        public double Red;
        public double Green;
        public double Blue;

        public static readonly ColorProfile Default = new ColorProfile(1, 1, 1);

        public ColorProfile(double r, double g, double b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }
    }
}
