using System;
using System.Windows;
using System.Windows.Data;

namespace TimeSheet
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var visible = true;
            if (value != null && value is bool)
                visible = (bool)value;

            var reverse = false;
            if (parameter != null)
                reverse = Boolean.Parse((string)parameter);

            visible = reverse ? !visible : visible;

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var visibility = Visibility.Visible;
            if (value != null && value is Visibility)
                visibility = (Visibility)value;

            var reverse = false;
            if (parameter != null && parameter is bool)
                reverse = (bool)parameter;

            var visible = visibility == Visibility.Visible;

            visible = reverse ? !visible : visible;

            return visible;
        }
    }
}
