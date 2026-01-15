using System.Globalization;
using System.Windows.Data;
using DeviceManage.Models;

namespace DeviceManage.Converters;

/// <summary>
/// DataType 到布尔值转换器（判断是否为 Bool 类型）
/// </summary>
public class DataTypeToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DataType dataType)
        {
            return dataType == DataType.Bool;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

