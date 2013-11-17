

namespace Mlux.Lib.Display
{
    public struct ColorAdjustment
    {
        public double Red;
        public double Green;
        public double Blue;

        public static readonly ColorAdjustment Default = new ColorAdjustment(1, 1, 1);

        public ColorAdjustment(double r, double g, double b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }

        
    }
}
