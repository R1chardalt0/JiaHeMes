using System;
using System.Globalization;
using System.Windows.Data;

namespace DeviceManage.Converters
{
    /// <summary>
    /// 搜索状态下拉框转换器：支持 null（全部）、true（启用）、false（禁用）
    /// </summary>
    public class SearchStatusToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0; // 全部
            }
            if (value is bool boolValue)
            {
                return boolValue ? 1 : 2; // true 对应索引 1 ("启用")，false 对应索引 2 ("禁用")
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                if (intValue == 0) return null; // 全部
                if (intValue == 1) return true; // 启用
                if (intValue == 2) return false; // 禁用
            }
            return null;
        }
    }
}

