using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DeviceManage.Models;

namespace DeviceManage.Converters;

/// <summary>
/// DataType 到可见性转换器（当是 Bool 类型时显示）
/// </summary>
public class DataTypeToBoolVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DataType dataType)
        {
            return dataType == DataType.Bool ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

