using DeviceManage.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManage.Models
{
    /// <summary>
    /// 用户表
    /// </summary>
    [Table("dm_user")]
    public class User : BaseModel
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 密码（加密存储）
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 用户角色（数据库存储字符串）
        /// </summary>
        [Required]
        [Column("Role")]
        public string RoleString { get; set; } = string.Empty;

        /// <summary>
        /// 用户角色（枚举类型，不映射到数据库）
        /// </summary>
        [NotMapped]
        public UserRole Role
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RoleString))
                    return UserRole.OP; // 默认值（操作员）

                // 尝试通过描述匹配
                foreach (UserRole role in System.Enum.GetValues(typeof(UserRole)))
                {
                    var field = role.GetType().GetField(role.ToString());
                    if (field != null)
                    {
                        var description = ((DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)))?.Description;
                        if (description == RoleString)
                            return role;
                    }
                }

                // 尝试直接解析枚举名称
                if (System.Enum.TryParse<UserRole>(RoleString, true, out var parsedRole))
                    return parsedRole;

                return UserRole.OP; // 默认值（操作员）
            }
            set
            {
                var field = value.GetType().GetField(value.ToString());
                if (field != null)
                {
                    var description = ((DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)))?.Description;
                    RoleString = description ?? value.ToString();
                }
                else
                {
                    RoleString = value.ToString();
                }
            }
        }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [MaxLength(50)]
        public string? RealName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [MaxLength(200)]
        public string? Remarks { get; set; }

        /// <summary>
        /// 是否已删除（软删除标记）
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
    /// <summary>
    /// 用户角色枚举
    /// </summary>
    public enum UserRole
    {
        [Description("管理员")]
        admin = 1,

        [Description("工艺工程师")]
        QE = 2,

        [Description("设备管理工程师")]
        ME = 3,

        [Description("班长")]
        TL = 4,

        [Description("操作员")]
        OP = 5
    }

    /// <summary>
    /// User实体配置类，用于配置初始数据
    /// </summary>
    public class UserEntityConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
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

