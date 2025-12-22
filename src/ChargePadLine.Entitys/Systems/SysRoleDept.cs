using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Systems
{
    [Table("sys_roleDept")]
    public class SysRoleDept
    {
        [Key]
        public long Id { get; set; }  // ✅ 自动识别为主键
        /// <summary>
        /// 角色ID
        /// </summary>
        [Description("角色ID")]
        public long RoleId { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        [Description("部门ID")]
        public long DeptId { get; set; }
    }

    public class SysRoleDeptClaimEntityConfiguration : IEntityTypeConfiguration<SysRoleDept>
    {
        public void Configure(EntityTypeBuilder<SysRoleDept> builder)
        {
            var defaultSysRoleDept = new SysRoleDept
            {
                Id = 1,
                DeptId = 1,
                RoleId = 1
            };
            builder.HasData(defaultSysRoleDept);
        }
    }
}
