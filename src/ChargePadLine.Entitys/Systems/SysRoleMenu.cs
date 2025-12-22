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
    /// <summary>
    /// 角色菜单关联表
    /// </summary>
    [Table("sys_roleMenu")]
    public class SysRoleMenu
    {
        [Key]
        public long Id { get; set; }  // ✅ 自动识别为主键
        /// <summary>
        /// 角色ID
        /// </summary>
        [Description("角色ID")]
        public long RoleId { get; set; }

        /// <summary>
        /// 菜单ID
        /// </summary>
        [Description("菜单ID")]
        public long MenuId { get; set; }
    }

    public class SysRoleMenuClaimEntityConfiguration : IEntityTypeConfiguration<SysRoleMenu>
    {
        public void Configure(EntityTypeBuilder<SysRoleMenu> builder)
        {
            var defaultSysRoleMenus = new[]
            {
            new SysRoleMenu { Id = 1,  RoleId = 1, MenuId = 1   },
            new SysRoleMenu { Id = 2,  RoleId = 1, MenuId = 2   },
            new SysRoleMenu { Id = 3,  RoleId = 1, MenuId = 201 },
            new SysRoleMenu { Id = 4,  RoleId = 1, MenuId = 202 },
            new SysRoleMenu { Id = 5,  RoleId = 1, MenuId = 203 },
            new SysRoleMenu { Id = 6,  RoleId = 1, MenuId = 3   },
            new SysRoleMenu { Id = 7,  RoleId = 1, MenuId = 301 },
            new SysRoleMenu { Id = 8,  RoleId = 1, MenuId = 302 },
            new SysRoleMenu { Id = 9,  RoleId = 1, MenuId = 303 },
            new SysRoleMenu { Id = 10, RoleId = 1, MenuId = 4   },
            new SysRoleMenu { Id = 11, RoleId = 1, MenuId = 401 },
            new SysRoleMenu { Id = 12, RoleId = 1, MenuId = 402 },
            new SysRoleMenu { Id = 13, RoleId = 1, MenuId = 403 },
            new SysRoleMenu { Id = 14, RoleId = 1, MenuId = 404 },
            new SysRoleMenu { Id = 15, RoleId = 1, MenuId = 405 },
            new SysRoleMenu { Id = 16, RoleId = 1, MenuId = 406 },
            new SysRoleMenu { Id = 17, RoleId = 1, MenuId = 407 },
            new SysRoleMenu { Id = 18, RoleId = 1, MenuId = 5   },
            new SysRoleMenu { Id = 19, RoleId = 1, MenuId = 501 },
            new SysRoleMenu { Id = 20, RoleId = 1, MenuId = 502 },
            new SysRoleMenu { Id = 21, RoleId = 1, MenuId = 503 },
            new SysRoleMenu { Id = 22, RoleId = 1, MenuId = 504 },
            new SysRoleMenu { Id = 23, RoleId = 1, MenuId = 6   },
            new SysRoleMenu { Id = 24, RoleId = 1, MenuId = 601 },
            new SysRoleMenu { Id = 25, RoleId = 1, MenuId = 602 },
            new SysRoleMenu { Id = 26, RoleId = 1, MenuId = 603 }
        };

            builder.HasData(defaultSysRoleMenus);
        }
    }
}
