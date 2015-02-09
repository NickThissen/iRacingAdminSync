using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace iRacingAdmin.Converters
{
    public class PingValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ping = (int) value;
            if (ping > 999) ping = 999;
            return ping;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PingColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ping = (int)value;
            if (ping < 100)
                return Brushes.LawnGreen;
            if (ping < 200)
                return Brushes.Gold;
            if (ping < 500)
                return Brushes.Orange;
            if (ping < 999)
                return Brushes.DarkOrange;
            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
