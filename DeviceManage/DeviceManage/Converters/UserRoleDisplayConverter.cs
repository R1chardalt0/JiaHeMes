using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using DeviceManage.Models;

namespace DeviceManage.Converters
{
    /// <summary>
    /// 用户角色显示转换器（用于ComboBox显示，支持null显示为"全部"）
    /// </summary>
    public class UserRoleDisplayConverter : IValueConverter
    {
        /// <summary>
        /// 枚举转字符串（显示描述），null显示为"全部"
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "全部";

            if (value is UserRole role)
            {
                return GetDescription(role);
            }
            return string.Empty;
        }

        /// <summary>
        /// 字符串转枚举（通过描述匹配）
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (str == "全部" || string.IsNullOrWhiteSpace(str))
                    return null;

                // 通过描述匹配
                foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
                {
                    if (GetDescription(role) == str)
                        return role;
                }

                // 尝试直接解析枚举名称
                if (Enum.TryParse<UserRole>(str, true, out var parsedRole))
                    return parsedRole;
            }
            return null;
        }

        /// <summary>
        /// 获取枚举的描述
        /// </summary>
        private string GetDescription(UserRole role)
        {
            var field = role.GetType().GetField(role.ToString());
            if (field != null)
            {
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                return attribute?.Description ?? role.ToString();
            }
            return role.ToString();
        }
    }
}

