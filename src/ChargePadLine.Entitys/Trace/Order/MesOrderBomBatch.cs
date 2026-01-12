using ChargePadLine.Entitys.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace ChargePadLine.Entitys.Trace.Order
{
    /// <summary>
    /// MES工单BOM批次表
    /// </summary>
    [Table("mes_order_bom_batch")]
    public class MesOrderBomBatch : BaseEntity
    {
        /// <summary>
        /// 工单BOM批次ID
        /// </summary>
        [Key]
        [Description("工单BOM批次ID")]
        [Column("OrderBomBatchId")]
        public Guid OrderBomBatchId { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        [Description("物料ID")]
        [Column("ProductListId")]
        public Guid? ProductListId { get; set; }

        /// <summary>
        /// 批次编码
        /// </summary>
        [Description("批次编码")]
        [Column("BatchCode")]
        public string BatchCode { get; set; } = "";

        /// <summary>
        /// 站点
        /// </summary>
        [Description("站点")]
        [Column("StationListId")]
        public Guid? StationListId { get; set; }

        /// <summary>
        /// 状态：1-正常，2-已使用完，3-已下料
        /// </summary>
        [Description("状态：1-正常，2-已使用完，3-已下料")]
        [Column("OrderBomBatchStatus")]
        public int OrderBomBatchStatus { get; set; } = 1;

        /// <summary>
        /// 批次数量
        /// </summary>
        [Description("批次数量")]
        [Column("BatchQty", TypeName = "numeric(15,4)")]
        public decimal BatchQty { get; set; } = 0;

        /// <summary>
        /// 已使用数量
        /// </summary>
        [Description("已使用数量")]
        [Column("CompletedQty", TypeName = "numeric(15,4)")]
        public decimal CompletedQty { get; set; } = 0;

        /// <summary>
        /// 工单ID
        /// </summary>
        [Description("工单ID")]
        [Column("OrderListId")]
        public Guid? OrderListId { get; set; }
    }

    public class MesOrderBomBatchEntityTypeConfiguration : IEntityTypeConfiguration<MesOrderBomBatch>
    {
        public void Configure(EntityTypeBuilder<MesOrderBomBatch> builder)
        {
            // 主键配置
            builder.HasKey(e => e.OrderBomBatchId);

            // 索引配置
            builder.HasIndex(e => e.ProductListId);
            builder.HasIndex(e => e.BatchCode);
            builder.HasIndex(e => e.StationListId);
            builder.HasIndex(e => e.OrderBomBatchStatus);
            builder.HasIndex(e => e.OrderListId);
        }
    }
}