using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using DeviceManage.Models;

namespace DeviceManage.Converters
{
    /// <summary>
    /// 搜索角色下拉框转换器：支持 null（全部）和各个角色枚举值
    /// </summary>
    public class SearchRoleToIndexConverter : IValueConverter
    {
        // 按照XAML中ComboBox的顺序定义角色列表（与ComboBoxItem的顺序一致）
        private static readonly List<UserRole> RoleOrder = new List<UserRole>
        {
            UserRole.admin,  // 管理员 (索引1)
            UserRole.QE,     // 工艺工程师 (索引2)
            UserRole.ME,     // 设备管理工程师 (索引3)
            UserRole.TL,     // 班长 (索引4)
            UserRole.OP      // 操作员 (索引5)
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0; // 全部
            }
            
            if (value is UserRole role)
            {
                // 查找角色在列表中的索引（+1 因为索引0是"全部"）
                var index = RoleOrder.IndexOf(role);
                return index >= 0 ? index + 1 : 0;
            }
            
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                if (intValue == 0) return null; // 全部
                
                // 索引-1 因为索引0是"全部"
                var roleIndex = intValue - 1;
                if (roleIndex >= 0 && roleIndex < RoleOrder.Count)
                {
                    return RoleOrder[roleIndex];
                }
            }
            return null;
        }
    }
}

