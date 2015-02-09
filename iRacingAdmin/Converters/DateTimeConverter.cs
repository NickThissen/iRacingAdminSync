using System;
using System.Globalization;
using System.Windows.Data;

namespace iRacingAdmin.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
            {
                var local = ((DateTime) value).ToLocalTime();
                if (parameter != null && parameter.ToString() == "time") return ToTime(local);
                if (parameter != null && parameter.ToString() == "date") return ToDate(local);
                return ToLongDate(local);
            }
            return value;
        }

        private static string ToLongDate(DateTime value)
        {
            return value.ToString("g", CultureInfo.InvariantCulture);
        }

        private static string ToDate(DateTime value)
        {
            return value.ToString("d", CultureInfo.InvariantCulture);
        }

        private static string ToTime(DateTime value)
        {
            return value.ToString("t", CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
