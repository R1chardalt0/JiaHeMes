using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DeviceManage.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 
                    new SolidColorBrush(Colors.Green) :  // 启用状态显示绿色
                    new SolidColorBrush(Colors.Red);     // 禁用状态显示红色
            }
            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}