using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using iRacingAdmin.Models.Drivers;

namespace iRacingAdmin.Converters
{
    public class DeltaTimeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var delta = value as DeltaTime;
            if (delta != null)
            {
                if (!delta.IsLapped)
                {
                    var limit = Properties.Settings.Default.DeltaTimeColorLimit;
                    if (limit > 0)
                    {
                        var time = delta.Time.GetValueOrDefault();

                        if (time < limit)
                        {
                            var percentage = 1d - time/limit;
                            var red = (byte)(255*percentage);
                            if (red > 255) red = 255;

                            return Color.FromArgb(255, red, 0, 0);
                        }
                    }
                }
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
