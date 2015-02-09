using System;
using System.Globalization;
using System.Windows.Media;

namespace iRacingAdmin.Classes
{
    public static class Parser
    {
        public static double ParseTrackLength(string value)
        {
           // if (value == null) return 0;

            // value = "6.93 km"
            double length = 0;

            var indexOfKm = value.IndexOf("km");
            if (indexOfKm > 0) value = value.Substring(0, indexOfKm);

            if (double.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out length))
            {
                return length;
            }
            return 0;
        }

        public static double ParseSec(string value)
        {
            // value = "600.00 sec"
            double length = 0;

            var indexOfSec = value.IndexOf(" sec");
            if (indexOfSec > 0) value = value.Substring(0, indexOfSec);

            if (double.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out length))
            {
                return length;
            }
            return 0;
        }

        public static int ParseInt(string value, int @default = 0)
        {
            int val;
            if (int.TryParse(value, out val)) return val;
            return @default;
        }

        public static float ParseFloat(string value, float @default = 0f)
        {
            float val;
            if (float.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite,
                CultureInfo.InvariantCulture, out val)) return val;
            return @default;
        }

        public static Color ParseColor(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && value.StartsWith("0x"))
            {
                try
                {
                    var hex = value.Replace("0x", "#");
                    var colorObj = ColorConverter.ConvertFromString(hex);
                    if (colorObj != null)
                    {
                        var color = (Color)colorObj;
                        if (color != Colors.Black)
                        {
                            return color;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            return Colors.White;
        }
    }
}
