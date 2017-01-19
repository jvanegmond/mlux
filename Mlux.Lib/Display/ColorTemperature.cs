using System;

namespace Mlux.Lib.Display
{
    public static class ColorTemperature
    {
        /// <summary>
        /// Turns a temperature (between 0K and 70000K) into a color profile.
        /// </summary>
        public static ColorAdjustment GetColorProfile(double temperature)
        {
            // Code from http://www.tannerhelland.com/4435/convert-temperature-rgb-algorithm-code/

            temperature = temperature / 100;

            // Red
            double red;
            if (temperature <= 66)
            {
                red = 255;
            }
            else
            {
                red = temperature - 60;
                red = 329.698727446 * (Math.Pow(red, -0.1332047592));
                if (red < 0) red = 0;
                if (red > 255) red = 255;
            }

            // Green
            double green;
            if (temperature <= 66)
            {
                green = temperature;
                green = 99.4708025861 * Math.Log(green) - 161.1195681661;
                if (green < 0) green = 0;
                if (green > 255) green = 255;
            }
            else
            {
                green = temperature - 60;
                green = 288.1221695283 * (Math.Pow(green, -0.0755148492));
                if (green < 0) green = 0;
                if (green > 255) green = 255;
            }

            // Blue
            double blue;
            if (temperature >= 66)
            {
                blue = 255;
            }
            else
            {
                blue = temperature - 10;
                blue = 138.5177312231 * Math.Log(blue) - 305.0447927307;
                if (blue < 0) blue = 0;
                if (blue > 255) blue = 255;
            }

            red /= 255;
            green /= 255;
            blue /= 255;

            return new ColorAdjustment(red, green, blue);
        }
    }
}
