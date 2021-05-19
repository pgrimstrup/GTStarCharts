using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PanasonicNZ.WPF
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetTyoe, object parameter, CultureInfo culture)
        {
            bool isCollapsed;
            if (value is bool)
                isCollapsed = (bool)value;
            else if (value is bool? && ((bool?)value).HasValue)
                isCollapsed = ((bool?)value).Value;
            else
                return null;

            return isCollapsed ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetTyoe, object parameter, CultureInfo culture)
        {
            Visibility vis;
            if (value is Visibility)
                vis = (Visibility)value;
            else if (value is Visibility? && ((Visibility?)value).HasValue)
                vis = ((Visibility?)value).Value;
            else
                return null;

            return (vis == Visibility.Collapsed);
        }
    }
}
