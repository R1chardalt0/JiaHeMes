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
  /// MES工单BOM批次明细表
  /// </summary>
  [Table("mes_order_bom_batch_item")]
  public class MesOrderBomBatchItem : BaseEntity
  {
    /// <summary>
    /// 工单BOM批次明细ID
    /// </summary>
    [Key]
    [Description("工单BOM批次明细ID")]
    [Column("OrderBomBatchItemId")]
    public Guid OrderBomBatchItemId { get; set; }

    /// <summary>
    /// 工单BOM批次ID
    /// </summary>
    [Description("工单BOM批次ID")]
    [Column("OrderBomBatchId")]
    public Guid? OrderBomBatchId { get; set; }

    /// <summary>
    /// 关联的批次主表
    /// </summary>
    [ForeignKey("OrderBomBatchId")]
    public MesOrderBomBatch? MesOrderBomBatch { get; set; }

    /// <summary>
    /// SN编码
    /// </summary>
    [Description("SN编码")]
    [Column("SnNumber")]
    public string? SnNumber { get; set; } = "";
  }

  public class MesOrderBomBatchItemEntityTypeConfiguration : IEntityTypeConfiguration<MesOrderBomBatchItem>
  {
    public void Configure(EntityTypeBuilder<MesOrderBomBatchItem> builder)
    {
      // 主键配置
      builder.HasKey(e => e.OrderBomBatchItemId);

      // 外键配置
      builder.HasOne(e => e.MesOrderBomBatch)
          .WithMany()
          .HasForeignKey(e => e.OrderBomBatchId)
          .OnDelete(DeleteBehavior.Cascade);

      // 索引配置
      builder.HasIndex(e => e.OrderBomBatchId);
      builder.HasIndex(e => e.SnNumber);
    }
  }
}