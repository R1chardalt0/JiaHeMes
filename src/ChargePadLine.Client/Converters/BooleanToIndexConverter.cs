using System;
using System.Globalization;
using System.Windows.Data;

namespace ChargePadLine.Client.Converters
{
    public class BooleanToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1 : 0; // true 对应索引 1 ("使用中")，false 对应索引 0 ("停用")
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue == 1; // 索引 1 对应 true，索引 0 对应 false
            }
            return false;
        }
    }
}