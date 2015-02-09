using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using iRacingAdmin.Models.Admins;
using iRacingAdmin.Sync;

namespace iRacingAdmin.Converters
{
    public class AdminBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var users = new List<User>();

            var singleUser = value as User;
            var singleUserContainer = value as UserContainer;
            var usersValue = value as IEnumerable<User>;
            var usersContainerValue = value as IEnumerable<UserContainer>;

            if (singleUser != null)
            {
                users.Add(singleUser);
            }
            else if (singleUserContainer != null)
            {
                users.Add(singleUserContainer.User);
            }
            else if (usersValue != null)
            {
                users = usersValue.ToList();
            }
            else if (usersContainerValue != null)
            {
                users = usersContainerValue.Select(u => u.User).ToList();
            }
            
            if (users.Count > 0)
            {
                int count = users.Count;

                if (count == 1) return new SolidColorBrush(users.First().Color);

                var brush = new LinearGradientBrush();
                brush.StartPoint = new Point(0, 0);
                brush.EndPoint = new Point(1, 0);

                for (int i = 0; i < count; i++)
                {
                    var user = users[i];
                    var start = (float) i/count;
                    var end = (float) (i + 1)/count;

                    brush.GradientStops.Add(new GradientStop(user.Color, start));
                    brush.GradientStops.Add(new GradientStop(user.Color, end));
                }

                return brush;
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
