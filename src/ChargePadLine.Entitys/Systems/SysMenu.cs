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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChargePadLine.Entitys.Systems
{
    /// <summary>
    /// 菜单表
    /// </summary>
    [Table("sys_menu")]
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

    public class SysMenuClaimEntityConfiguration : IEntityTypeConfiguration<SysMenu>
    {
        public void Configure(EntityTypeBuilder<SysMenu> builder)
        {
            // 所有菜单种子数据
            var menus = new[]
            {
                new SysMenu
                {
                    MenuId = 1,
                    MenuName = "欢迎",
                    ParentName = null,
                    ParentId = 0,
                    OrderNum = 1,
                    Path = "/welcome",
                    Component = "Welcome",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = "smile",
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 853, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 2,
                    MenuName = "Dashboard",
                    ParentName = null,
                    ParentId = 0,
                    OrderNum = 2,
                    Path = "/dashboard",
                    Component = null,
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "M",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = "dashboard",
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 865, TimeSpan.FromHours(8)),
                    Remark = null
                },
                 new SysMenu
                {
                    MenuId = 201,
                    MenuName = "分析页",
                    ParentName = null,
                    ParentId = 2,
                    OrderNum = 1,
                    Path = "/dashboard/analysis",
                    Component = "dashboard/analysis",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = "smile",
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 903, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 202,
                    MenuName = "监控页",
                    ParentName = null,
                    ParentId = 2,
                    OrderNum = 2,
                    Path = "/dashboard/monitor",
                    Component = "dashboard/monitor",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = "smile",
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 908, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 203,
                    MenuName = "工作台",
                    ParentName = null,
                    ParentId = 2,
                    OrderNum = 3,
                    Path = "/dashboard/workplace",
                    Component = "dashboard/workplace",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = "smile",
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 913, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 3,
                    MenuName = "系统设置",
                    ParentName = null,
                    ParentId = 0,
                    OrderNum = 3,
                    Path = "/systems",
                    Component = null,
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = null,
                    MenuType = "M",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = "setting",
                    CreateTime = new DateTimeOffset(2025, 12, 15, 11, 54, 38, 879, TimeSpan.FromHours(8)),
                    Remark = null
                },

                new SysMenu
                {
                    MenuId = 301,
                    MenuName = "用户管理",
                    ParentName = null,
                    ParentId = 3,
                    OrderNum = 1,
                    Path = "/systems/user",
                    Component = "systems/user",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = "system:user:list",
                    Icon = "user",
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 932, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 302,
                    MenuName = "角色管理",
                    ParentName = null,
                    ParentId = 3,
                    OrderNum = 2,
                    Path = "/systems/role",
                    Component = "systems/role",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = "system:role:list",
                    Icon = "setting",
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 937, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 303,
                    MenuName = "菜单管理",
                    ParentName = null,
                    ParentId = 3,
                    OrderNum = 3,
                    Path = "/systems/menu",
                    Component = "systems/menu",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = "system:menu:list",
                    Icon = "menu",
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 944, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 304,
                    MenuName = "部门管理",
                    ParentName = null,
                    ParentId = 3,
                    OrderNum = 4,
                    Path = "/systems/dept",
                    Component = "systems/dept",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = "system:dept:list",
                    Icon = "setting",
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 949, TimeSpan.FromHours(8)),
                    Remark = null
                },
                 new SysMenu
                 {
                    MenuId = 305,
                    MenuName = "岗位管理",
                    ParentName = null,
                    ParentId = 3,
                    OrderNum = 5,
                    Path = "/systems/position",
                    Component = "systems/position",
                    Query = null,
                     RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = "system:position:list",
                    Icon = "team",
                    CreateTime = new DateTimeOffset(2025, 11, 24, 10, 12, 44, 9, TimeSpan.FromHours(8)),
                    Remark = null
                 },
                 new SysMenu
                {
                    MenuId = 306,
                    MenuName = "日志记录",
                    ParentName = null,
                    ParentId = 3,
                    OrderNum = 6,
                    Path = "/systems/operateLog",
                    Component = "systems/operateLog",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = "system:operateLog:list",
                    Icon = "file-text",
                    CreateBy = "admin",
                    CreateTime = new DateTimeOffset(2025, 11, 29, 14, 30, 24, 50, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 4,
                    MenuName = "产线设备管理",
                    ParentName = null,
                    ParentId = 0,
                    OrderNum = 4,
                    Path = "/productionEquipment",
                    Component = null,
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = null,
                    MenuType = "M",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = "tool",
                    CreateTime = new DateTimeOffset(2025, 12, 15, 11, 54, 11, 96, TimeSpan.FromHours(8)),
                    Remark = null
                },
                 new SysMenu
                {
                    MenuId = 401,
                    MenuName = "产线管理",
                    ParentName = null,
                    ParentId = 4,
                    OrderNum = 1,
                    Path = "/productionEquipment/productionLine",
                    Component = "productionEquipment/productionLine",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = null,
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 960, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 402,
                    MenuName = "设备管理",
                    ParentName = null,
                    ParentId = 4,
                    OrderNum = 2,
                    Path = "/productionEquipment/equipment",
                    Component = "productionEquipment/equipment",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = null,
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 965, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 5,
                    MenuName = "信息追溯",
                    ParentName = null,
                    ParentId = 0,
                    OrderNum = 5,
                    Path = "/trace",
                    Component = null,
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = null,
                    MenuType = "M",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = "table",
                    CreateTime = new DateTimeOffset(2025, 12, 15, 11, 54, 17, 282, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 501,
                    MenuName = "产品信息追溯",
                    ParentName = null,
                    ParentId = 5,
                    OrderNum = 1,
                    Path = "/trace/productTraceInfo",
                    Component = "Trace/ProductTraceInfo",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = null,
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 983, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 502,
                    MenuName = "产量报表",
                    ParentName = null,
                    ParentId = 5,
                    OrderNum = 2,
                    Path = "/trace/productionRecords",
                    Component = "Trace/ProductionRecords",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = null,
                    CreateTime = new DateTimeOffset(2025, 11, 21, 9, 14, 41, 993, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 6,
                    MenuName = "历史记录",
                    ParentName = null,
                    ParentId = 0,
                    OrderNum = 6,
                    Path = "/history",
                    Component = null,
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = null,
                    MenuType = "M",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = "HistoryOutlined",
                    CreateTime = new DateTimeOffset(2025, 12, 15, 11, 54, 26, 873, TimeSpan.FromHours(8)),
                    Remark = null
                },
                new SysMenu
                {
                    MenuId = 601,
                    MenuName = "产品信息追溯",
                    ParentName = "",
                    ParentId = 6,
                    OrderNum = 1,
                    Path = "/history/product",
                    Component = "History/ProductTraceInfo/index",
                    Query = null,
                    RouteName = null,
                    IsFrame = "1",
                    IsCache = "0",
                    MenuType = "C",
                    Visible = "0",
                    Status = "0",
                    Perms = null,
                    Icon = "AppstoreOutlined",
                    CreateBy = "admin",
                    CreateTime = new DateTimeOffset(2025, 12, 10, 10, 53, 26, 117, TimeSpan.FromHours(8)),
                    Remark = "产品信息追溯菜单"
                },               
            };
            builder.HasData(menus);
        }
    }
}