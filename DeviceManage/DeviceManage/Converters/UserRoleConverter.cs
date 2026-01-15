using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using DeviceManage.Models;

namespace DeviceManage.Converters
{
    /// <summary>
    /// 用户角色枚举转换器
    /// </summary>
    public class UserRoleConverter : IValueConverter
    {
        /// <summary>
        /// 枚举转字符串（显示描述）
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
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
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
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
            return UserRole.OP; // 默认值（操作员）
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

