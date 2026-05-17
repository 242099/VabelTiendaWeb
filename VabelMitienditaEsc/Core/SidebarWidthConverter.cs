using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VabelMitienditaEsc.Core
{
    public class SidebarWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool showSidebar && showSidebar)
            {
                return new GridLength(250);
            }
            return new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}