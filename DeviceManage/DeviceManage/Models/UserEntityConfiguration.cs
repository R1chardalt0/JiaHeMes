using DeviceManage.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel;

namespace DeviceManage.Models
{
    /// <summary>
    /// User实体配置类，用于配置初始数据
    /// </summary>
    public class UserEntityConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // 配置默认管理员用户
            var defaultUser = new User
            {
                Id = 1,
                Username = "admin",
                Password = MD5Helper.Encrypt("admin123"),
                RoleString = GetRoleDescription(UserRole.admin),
                RealName = "管理员",
                Email = "admin@example.com",
                Phone = "15195028555",
                IsEnabled = true,
                IsDeleted = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                LastLoginAt = null,
                Remarks = "管理员账户"
            };

            // 使用HasData配置初始数据（仅在数据库创建时插入，如果已存在则不会重复插入）
            builder.HasData(defaultUser);
        }

        /// <summary>
        /// 获取角色描述
        /// </summary>
        private string GetRoleDescription(UserRole role)
        {
            var field = role.GetType().GetField(role.ToString());
            if (field != null)
            {
                var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
                return attribute?.Description ?? role.ToString();
            }
            return role.ToString();
        }
    }
}

