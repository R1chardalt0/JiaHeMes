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
    [Table("SysRoleMenu")]
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
}
