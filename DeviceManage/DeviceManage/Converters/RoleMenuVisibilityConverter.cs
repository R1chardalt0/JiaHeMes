using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DeviceManage.Converters
{
    /// <summary>
    /// 根据当前登录用户角色与菜单标识返回可见性。
    /// 绑定方式：
    ///     Visibility = "{Binding CurrentUserRole.Value, Converter={StaticResource RoleMenuVisibilityConverter}, ConverterParameter=RecipeManagement}"
    /// </summary>
    public class RoleMenuVisibilityConverter : IValueConverter
    {
        private static readonly HashSet<string> _allMenus = new()
        {
            "Dashboard",
            "PLCDeviceManagement",
            "RecipeManagement",
            "TagManagement",
            "LogManagement",
            "UserManagement"
        };

        private static readonly Dictionary<string, HashSet<string>> _roleMenuPermissions = new(StringComparer.OrdinalIgnoreCase)
        {
            // 班长
            { "班长", new HashSet<string>{ "Dashboard", "RecipeManagement" } },
            // 操作员
            { "操作员", new HashSet<string>{ "Dashboard" } },
            // 工艺工程师 / 工艺管理工程师
            { "工艺工程师", new HashSet<string>{ "Dashboard", "RecipeManagement", "TagManagement", "LogManagement" } },
            { "工艺管理工程师", new HashSet<string>{ "Dashboard", "RecipeManagement", "TagManagement", "LogManagement" } },
            // 设备管理工程师
            { "设备管理工程师", new HashSet<string>{ "Dashboard", "PLCDeviceManagement", "RecipeManagement", "TagManagement" } },
            // 管理员
            { "管理员", new HashSet<string>{ "Dashboard", "PLCDeviceManagement", "RecipeManagement", "TagManagement", "LogManagement", "UserManagement" } },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (parameter is null)
                {
                    // 未提供菜单标识，默认显示
                    return Visibility.Visible;
                }

                var menuKey = parameter.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(menuKey) || !_allMenus.Contains(menuKey))
                {
                    return Visibility.Collapsed;
                }

                var roleDesc = (value?.ToString() ?? string.Empty).Trim();

                // 如果没有角色信息，默认隐藏
                if (string.IsNullOrWhiteSpace(roleDesc))
                {
                    return Visibility.Collapsed;
                }

                // 管理员拥有所有权限
                if (string.Equals(roleDesc, "管理员", StringComparison.OrdinalIgnoreCase))
                {
                    return Visibility.Visible;
                }

                return _roleMenuPermissions.TryGetValue(roleDesc, out var allowedMenus) && allowedMenus.Contains(menuKey)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            catch
            {
                // 出现异常时默认隐藏，避免影响界面
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
