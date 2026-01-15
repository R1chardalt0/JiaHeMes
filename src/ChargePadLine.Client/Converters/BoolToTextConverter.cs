using System;
using System.Globalization;
using System.Windows.Data;

namespace ChargePadLine.Client.Converters
{
    /// <summary>
    /// 根据 bool 返回两个不同的字符串，中间用 | 分隔
    /// 参数示例： "编辑|新增"
    /// </summary>
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = parameter as string;
            if (param == null) return value;
            var parts = param.Split('|');
            if (parts.Length != 2) return value;
            var flag = value is bool b && b;
            return flag ? parts[0] : parts[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

