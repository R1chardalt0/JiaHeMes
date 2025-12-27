using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChargePadLine.Entitys.Systems
{
    /// <summary>
    /// 用户表
    /// </summary>
    [Table("sys_user")]
    public class SysUser : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Description("用户ID")]
        [Key]
        public long UserId { get; set; }
        /// <summary>
        /// 部门ID
        /// </summary>
        [Description("部门ID")]
        public long? DeptId { get; set; }
        /// <summary>
        /// 用户账号
        /// </summary>
        [Description("用户账号")]
        public string? UserName { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        [Description("用户昵称")]
        public string? NickName { get; set; }
        /// <summary>
        /// 用户邮箱（用于找回密码等功能）
        /// </summary>
        [Description("用户邮箱（用于找回密码等功能")]
        public string? Email { get; set; }
        /// <summary>
        /// 用户密码（加密存储）
        /// </summary>
        [Description("用户密码（加密存储）")]
        public string? Password { get; set; }
        /// <summary>
        /// 性别
        /// </summary>  
        [Description("性别")]
        public string? Sex { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        [Description("电话号码")]
        public string? PhoneNumber { get; set; }
        /// <summary>
        /// 头像地址（存储用户头像的URL或路径）
        /// </summary>
        [Description("头像地址（存储用户头像的URL或路径")]
        public string? Avatar { get; set; }
        /// <summary>
        /// 账号状态（0正常 1停用）
        /// </summary>
        [Description("账号状态（0正常 1停用")]
        public string? Status { get; set; }
        /// <summary>
        /// 删除标志（0代表存在 2代表删除）
        /// </summary>
        [Description("删除标志（0代表存在 2代表删除")]
        public string? DelFlag { get; set; }
        /// <summary>
        /// 最后登录IP地址（用于记录用户的登录来源）·
        /// </summary>
        [Description("最后登录IP地址（用于记录用户的登录来源)")]
        public string? LoginIp { get; set; }
        /// <summary>
        /// 最后登录时间
        /// </summary>
        [Description("最后登录时间")]
        public DateTimeOffset LoginDate { get; set; }
        /// <summary>
        /// 密码更新时间（用于记录用户密码的最后修改时间，便于安全管理）
        /// </summary>
        [Description("密码更新时间（用于记录用户密码的最后修改时间，便于安全管理")]
        public DateTimeOffset PwdUpdateDate { get; set; }
        /// <summary>
        /// 部门对象（用于关联用户与部门信息）
        /// </summary>
        [Description("部门对象（用于关联用户与部门信息")]
        public SysDept? Dept { get; set; }
        /// <summary>
        /// 角色对象 
        /// </summary>
        [Description("角色对象")]
        public List<SysRole> Roles { get; set; } = new List<SysRole>();
        /// <summary>
        /// 角色组
        /// </summary>
        [Description("角色组")]
        public long[]? RoleIds { get; set; }
        /// <summary>
        /// 岗位组
        /// </summary>
        [Description("岗位组")]
        public long[]? PostIds { get; set; }
        /// <summary>
        /// 角色ID（用于关联用户与角色）
        /// </summary>
        [Description("角色ID（用于关联用户与角色")]
        public long? RoleId { get; set; }
    }
    public class SysUserClaimEntityConfiguration : IEntityTypeConfiguration<SysUser>
    {
        public void Configure(EntityTypeBuilder<SysUser> builder)
        {
            //builder.HasOne(uc => uc.Email).WithMany(u => u.).HasForeignKey(uc => uc.UserId).IsRequired();
            //builder.HasOne(uc => uc.ClaimEntity)
            //    .WithMany(u => u.UserClaims)
            //    .HasForeignKey(uc => uc.ClaimEntityId)
            //    .IsRequired();

            var defaultUser = new SysUser
            {
                UserId = 1, // 通常建议自增，若手动指定请确保唯一
                            // 假设存在ID为1的部门
                UserName = "admin",
                NickName = "系统管理员",
                Email = "admin@example.com",
                Password = "D3283606DABAC98BEF13E80ABF9E11C9", // 例如用BCrypt: BCrypt.Net.BCrypt.HashPassword("默认密码")
                Sex = "0", // 假设"0"代表男，"1"代表女
                PhoneNumber = "15195028555",
                Avatar = "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png", // 默认头像路径
                Status = "0", // "0"代表正常
                DelFlag = "0", // "0"代表存在
                LoginIp = "127.0.0.1",
                LoginDate = DateTimeOffset.Now,
                PwdUpdateDate = DateTimeOffset.Now,
                CreateTime=DateTime.Now,
                UpdateTime=DateTime.Now,
                RoleIds = new long[] { 1 }, // 假设角色ID数组
                PostIds = new long[] { 1 }  // 假设岗位ID数组
            };
            builder.HasData(defaultUser);
        }
    }
}
