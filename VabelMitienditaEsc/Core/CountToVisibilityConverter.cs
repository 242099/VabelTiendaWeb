using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace VabelMitienditaEsc.Core
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                // Si count es 0, muestra el elemento (mensaje vacío)
                // Si count > 0, oculta el elemento
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            // Si el valor es null, ocultar
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
