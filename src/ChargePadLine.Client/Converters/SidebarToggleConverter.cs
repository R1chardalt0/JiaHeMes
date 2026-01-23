using System;
using System.Globalization;
using System.Windows.Data;

namespace ChargePadLine.Client.Converters;

public class SidebarToggleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isVisible)
        {
            return isVisible ? "«" : "»";  // « means close (hide sidebar), » means open (show sidebar)
        }
        return "»";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}