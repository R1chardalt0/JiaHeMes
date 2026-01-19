using ChargePadLine.Entitys.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace ChargePadLine.Entitys.Trace.Packing
{
    /// <summary>
    /// MES包装表
    /// </summary>
    [Table("mes_packing")]
    public class MesPacking : BaseEntity
    {
        /// <summary>
        /// 包装ID
        /// </summary>
        [Key]
        [Description("包装ID")]
        [Column("PackingId")]
        public Guid PackingId { get; set; }

        /// <summary>
        /// SN
        /// </summary>
        [Required]
        [Description("SN")]
        [Column("SN", TypeName = "text")]
        public string SN { get; set; } = string.Empty;

        /// <summary>
        /// 内箱码
        /// </summary>
        [Required]
        [Description("内箱码")]
        [Column("InnerBoxSN", TypeName = "text")]
        public string InnerBoxSN { get; set; } = string.Empty;

        /// <summary>
        /// 外箱码
        /// </summary>
        [Required]
        [Description("外箱码")]
        [Column("OuterBoxSN", TypeName = "text")]
        public string OuterBoxSN { get; set; } = string.Empty;

        /// <summary>
        /// 客户名称
        /// </summary>
        [Description("客户名称")]
        [Column("CustomerName", TypeName = "text")]
        public string? CustomerName { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        [Description("客户编码")]
        [Column("CustomerCode", TypeName = "text")]
        public string? CustomerCode { get; set; }

        /// <summary>
        /// 搜索值
        /// </summary>
        [Description("搜索值")]
        [Column("SearchValue", TypeName = "text")]
        public string? SearchValue { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        [Column("CreateBy", TypeName = "text")]
        public string? CreateBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        [Column("CreateTime", TypeName = "timestamptz(6)")]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [Description("更新人")]
        [Column("UpdateBy", TypeName = "text")]
        public string? UpdateBy { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        [Column("UpdateTime", TypeName = "timestamptz(6)")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        [Column("Remark", TypeName = "text")]
        public string? Remark { get; set; }
    }

    public class MesPackingEntityTypeConfiguration : IEntityTypeConfiguration<MesPacking>
    {
        public void Configure(EntityTypeBuilder<MesPacking> builder)
        {
            // 主键配置
            builder.HasKey(e => e.PackingId);

            // 索引配置
            builder.HasIndex(e => e.SN);
            builder.HasIndex(e => e.InnerBoxSN);
            builder.HasIndex(e => e.OuterBoxSN);
            builder.HasIndex(e => e.CustomerCode);
            builder.HasIndex(e => e.SearchValue);
        }
    }
}