using System;
using System.Windows;
using System.Windows.Data;

namespace TimeSheet.Converters
{
    public class ReverseBoolConverter : IValueConverter
    {
        /// <summary>
        /// Takes a bool and returns the opposite value.
        /// </summary>
        /// <param name="value">Bool</param>
        /// <param name="targetType">Not used</param>
        /// <param name="parameter">Not used></param>
        /// <param name="culture">Not used</param>
        /// <returns>Bool</returns>
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool returnBool = false;

            if (value != null && value is bool)
            {
                returnBool = !(bool)value;
            }

            return returnBool;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool returnBool = false;

            if (value != null && value is bool)
            {
                returnBool = !(bool)value;
            }

            return returnBool;
        }
    }
}
