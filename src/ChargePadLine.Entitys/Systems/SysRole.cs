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
    [Table("SysRoles")]

    public class SysRole : BaseEntity
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Key]
        [Description("角色ID")]
        public long RoleId { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>
        [Description("角色名称")]
        public string? RoleName { get; set; }
        /// <summary>
        /// 角色权限字符串（用于存储角色的权限标识）
        /// </summary>
        [Description(" 角色权限字符串（用于存储角色的权限标识）")]
        public string? RoleKey { get; set; }
        /// <summary>
        /// 角色类型（0系统用户 1自定义用户）
        /// </summary>
        [Description("角色类型（0系统用户 1自定义用户）")]
        public string? RoleSort { get; set; }
        /// <summary>
        /// 显示顺序（用于控制角色在列表中的显示顺序）
        /// </summary>
        [Description("显示顺序（用于控制角色在列表中的显示顺序）")]
        public string? DataScope { get; set; }
        /// <summary>
        /// 菜单树选择项（用于前端展示角色的菜单权限）
        /// </summary>
        [Description("菜单树选择项（用于前端展示角色的菜单权限）")]
        public string? MenuCheckStrictly { get; set; }
        /// <summary>
        /// 部门树选择项（用于前端展示角色的部门权限）
        /// </summary>
        [Description("部门树选择项（用于前端展示角色的部门权限）")]
        public string? DeptCheckStrictly { get; set; }
        /// <summary>
        /// 角色状态（0正常 1停用）
        /// </summary>
        [Description("角色状态（0正常 1停用）")]
        public string? Status { get; set; }
        /// <summary>
        /// 删除标志（代表存在 2代表删除）
        /// </summary>
        [Description("删除标志（代表存在 2代表删除）")]
        public string? DelFlag { get; set; }
        /// <summary>
        /// 用户是否存在此角色标识 默认不存在
        /// </summary>
        [Description("用户是否存在此角色标识 默认不存在")]
        public bool Flag { get; set; } = false;
        /// <summary>
        /// 菜单对象（用于关联角色与菜单信息）
        /// </summary>
        [Description("菜单对象（用于关联角色与菜单信息）")]
        public long[]? MenuIds { get; set; }
        /// <summary>
        /// 部门对象（用于关联角色与部门信息）
        /// </summary>
        [Description("部门对象（用于关联角色与部门信息）")]
        public long[]? DeptIds { get; set; }
        /// <summary>
        /// 权限集合（用于存储角色的具体权限列表）
        /// </summary>
        [Description("权限集合（用于存储角色的具体权限列表")]
        public HashSet<String>? Permissions;
    }
}
