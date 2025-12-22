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

namespace ChargePadLine.Entitys.Systems
{
    [Table("sys_userRole")]
    public class SysUserRole
    {
        [Key]
        public long Id { get; set; }  // ✅ 自动识别为主键
        /// <summary>
        /// 用户ID
        /// </summary>
        [Description("用户ID")]
        public long UserId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Description("角色ID")]
        public long RoleId { get; set; }
    }

    public class SysUserRoleClaimEntityConfiguration : IEntityTypeConfiguration<SysUserRole>
    {
        public void Configure(EntityTypeBuilder<SysUserRole> builder)
        {
            var defaultSysUserRole = new SysUserRole
            {
                Id = 1,
                UserId = 1,
                RoleId = 1
            };
            builder.HasData(defaultSysUserRole);
        }
    }
}
