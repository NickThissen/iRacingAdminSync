using System;
using System.Globalization;
using System.Windows.Data;

namespace iRacingAdmin.Converters
{
    public class IRatingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var irating = (int) value;
            var k = irating/1000f;
            return k.ToString("0.0k");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
