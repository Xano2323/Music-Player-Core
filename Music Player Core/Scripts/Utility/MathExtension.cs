using System;


namespace Music_Player.Utility
{
    public static class Mathf
    {
        /// <summary>
        /// Clamps value between two numbers
        /// </summary>
        public static double Clamp(double value, double min, double max)
        {
            if (min > max)
                throw new Exception(message: "Wartość min nie może większa od max");

            if (value <= min)
                value = min;
            else if (value >= max)
                value = max;

            return value;
        }
    }
}