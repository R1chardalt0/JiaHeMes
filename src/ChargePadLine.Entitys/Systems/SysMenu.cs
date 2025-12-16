using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChargePadLine.Entitys.Systems
{
    [Table("SysMenus")]
    public class SysMenu : BaseEntity
    {
        /// <summary>
        /// 菜单ID
        /// </summary>
        [Description("菜单ID")]
        [Key]
        public long MenuId { get; set; }
        /// <summary>
        /// 菜单名称
        /// </summary>
        [Description("菜单名称")]
        public string? MenuName { get; set; }
        /// <summary>
        /// 父菜单ID
        /// </summary>
        [Description("父菜单ID")]
        public string? ParentName { get; set; }
        /// <summary>
        /// 父菜单ID（用于表示当前菜单的上级菜单）
        /// </summary>
        [Description("父菜单ID（用于表示当前菜单的上级菜单)")]
        public long? ParentId { get; set; }
        /// <summary>
        /// 显示顺序（用于控制菜单在列表中的显示顺序）
        /// </summary>
        [Description("显示顺序（用于控制菜单在列表中的显示顺序)")]
        public int? OrderNum { get; set; }
        /// <summary>
        /// 路由地址（用于前端路由跳转）
        /// </summary>
        [Description("路由地址（用于前端路由跳转）")]
        public string? Path { get; set; }
        /// <summary>
        /// 组件路径（用于前端组件加载）
        /// </summary>
        [Description("组件路径（用于前端组件加载）")]
        public string? Component { get; set; }
        /// <summary>
        /// 路由参数（用于传递路由参数）
        /// </summary>
        [Description("路由参数（用于传递路由参数）")]
        public string? Query { get; set; }
        /// <summary>
        /// 路由名称（用于前端路由命名）
        /// </summary>
        [Description("路由名称（用于前端路由命名）")]
        public string? RouteName { get; set; }
        /// <summary>
        /// 是否为外链（0是 1否）
        /// </summary>
        [Description("是否为外链（0是 1否）")]
        public string? IsFrame { get; set; }
        /// <summary>
        /// 是否缓存（0缓存 1不缓存）
        /// </summary>
        [Description("是否缓存（0缓存 1不缓存）")]
        public string? IsCache { get; set; }
        /// <summary>
        /// 类型（M目录 C菜单 F按钮）
        /// </summary>
        [Description("类型（M目录 C菜单 F按钮）")]
        public string? MenuType { get; set; }
        /// <summary>
        /// 显示状态（0显示 1隐藏）
        /// </summary>
       [Description("显示状态（0显示 1隐藏）")]
        public string? Visible { get; set; }
        /// <summary>
        /// 菜单状态（0正常 1停用）
        /// </summary>
        [Description("菜单状态（0正常 1停用）")]
        public string? Status { get; set; }
        /// <summary>
        /// 权限字符串
        /// </summary>
        [Description("权限字符串")]
        public string? Perms { get; set; }
        /// <summary>
        /// 菜单图标
        /// </summary>
        [Description("菜单图标")]
        public string? Icon { get; set; }
        /// <summary>
        /// 子菜单
        /// </summary>
        [Description("子菜单")]
        public List<SysMenu> Children { get; set; } = new List<SysMenu>();

        public static bool IsAdmin(long userId)
        {
            return userId != null && 1L == userId;
        }
    }
}
