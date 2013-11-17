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
            var result = new ColorAdjustment();

            // Code from http://www.tannerhelland.com/4435/convert-temperature-rgb-algorithm-code/

            temperature = temperature / 100;

            if (temperature <= 66)
            {
                result.Red = 255;
            }
            else
            {
                result.Red = temperature - 60;
                result.Red = 329.698727446 * (Math.Pow(result.Red, -0.1332047592));
                if (result.Red < 0) result.Red = 0;
                if (result.Red > 255) result.Red = 255;
            }

            if (temperature <= 66)
            {
                result.Green = temperature;
                result.Green = 99.4708025861 * Math.Log(result.Green) - 161.1195681661;
                if (result.Green < 0) result.Green = 0;
                if (result.Green > 255) result.Green = 255;
            }
            else
            {
                result.Green = temperature - 60;
                result.Green = 288.1221695283 * (Math.Pow(result.Green, -0.0755148492));
                if (result.Green < 0) result.Green = 0;
                if (result.Green > 255) result.Green = 255;
            }

            if (temperature >= 66)
            {
                result.Blue = 255;
            }
            else
            {
                result.Blue = temperature - 10;
                result.Blue = 138.5177312231 * Math.Log(result.Blue) - 305.0447927307;
                if (result.Blue < 0) result.Blue = 0;
                if (result.Blue > 255) result.Blue = 255;
            }

            result.Red /= 255;
            result.Green /= 255;
            result.Blue /= 255;

            return result;
        }
    }
}
