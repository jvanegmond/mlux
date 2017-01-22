

using System;

namespace Mlux.Lib.Display
{
    public struct ColorAdjustment : IEquatable<ColorAdjustment>
    {
        public readonly double Red;
        public readonly double Green;
        public readonly double Blue;

        public static readonly ColorAdjustment Default = new ColorAdjustment(1, 1, 1);

        public ColorAdjustment(double red, double green, double blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public bool Equals(ColorAdjustment other)
        {
            return other.Red == Red && other.Green == Green && other.Blue == Blue;
        }

        public override string ToString()
        {
            return $"Adjust {Red} {Green} {Blue}";
        }
    }
}
