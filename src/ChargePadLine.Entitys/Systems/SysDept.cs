using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace ChargePadLine.Entitys.Systems
{
    [Table("sys_dept")]
    public class SysDept: BaseEntity
    {
        /// <summary>
        /// 部门ID
        /// </summary>
        [Description("部门ID")]
        [Key]
        public long DeptId { get; set; }
        /// <summary>
        /// 父部门ID（用于表示当前部门的上级部门）
        /// </summary>
        [Description("父部门ID（用于表示当前部门的上级部门")]
        public long? ParentId { get; set; }
        /// <summary>
        /// 祖级列表
        /// </summary>
        [Description("祖级列表")]
        public string? Ancestors;
        /// <summary>
        /// 部门名称（用于存储部门的名称信息）
        /// </summary>
        [Description("部门名称（用于存储部门的名称信息")]
        public string? DeptName { get; set; }
        /// <summary>
        /// 显示顺序（用于控制部门在列表中的显示顺序）
        /// </summary>
        [Description("显示顺序（用于控制部门在列表中的显示顺序")]
        public int? OrderNum { get; set; }
        /// <summary>
        /// 负责人（用于存储部门负责人的姓名或标识）
        /// </summary>
        [Description("负责人（用于存储部门负责人的姓名或标识")]
        public string? Leader { get; set; }
        /// <summary>
        /// 联系电话（用于存储部门的联系电话信息）
        /// </summary>
        [Description("联系电话（用于存储部门的联系电话信息")]
        public string? Phone { get; set; }
        /// <summary>
        /// 电子邮箱（用于存储部门的联系邮箱信息）
        /// </summary>
        [Description("电子邮箱（用于存储部门的联系邮箱信息")]
        public string? Email { get; set; }
        /// <summary>
        /// 部门状态（0正常 1停用）
        /// </summary>
        [Description("部门状态（0正常 1停用")]
        public string? Status { get; set; }
        /// <summary>
        /// 删除标志（0代表存在 2代表删除）
        /// </summary>
        [Description("删除标志（0代表存在 2代表删除")]
        public string? DelFlag { get; set; }
        /// <summary>
        /// 父部门名称
        /// </summary>
        [Description("父部门名称")]
        public string? ParentName { get; set; }
        /// <summary>
        /// 子部门列表（用于存储当前部门的所有子部门信息）
        /// </summary>
        [Description("子部门列表（用于存储当前部门的所有子部门信息")]
        public List<SysDept> Children { get; set; } = new List<SysDept>();
    }

    public class SysDeptClaimEntityConfiguration : IEntityTypeConfiguration<SysDept>
    {
        public void Configure(EntityTypeBuilder<SysDept> builder)
        {
            var defaultSysDept = new SysDept
            {
                // 主键
                DeptId = 100,

                // 部门自身字段
                ParentId = null,
                Ancestors = "0",
                DeptName = "总公司",
                OrderNum = 0,
                Leader = "系统管理员",
                Phone = "13800138000",
                Email = "admin@example.com",
                Status = "0",      // 正常
                DelFlag = "0",     // 未删除

                // BaseEntity 字段（这些会被映射到数据库）
                CreateBy = "system",
                CreateTime = new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.FromHours(8)), // UTC+8
                UpdateBy = "system",
                UpdateTime = new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.FromHours(8)),
                Remark = "系统默认根部门"

         
            };

            builder.HasData(defaultSysDept);
        }
    }
}