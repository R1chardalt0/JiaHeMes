using DeviceManage.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace DeviceManage.Converters
{
    /// <summary>
    /// 将 TagDetailDataArray 转为可读字符串（用于 DataGrid 显示）
    /// </summary>
    public class TagDetailsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not IEnumerable<TagDetail> list) return string.Empty;
            
            var items = list.Take(3)
                .Select(d => $"{d.TagName}({d.Address})");
            
            var text = string.Join(", ", items);
            if (list.Count() > 3) text += " ...";
            
            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}