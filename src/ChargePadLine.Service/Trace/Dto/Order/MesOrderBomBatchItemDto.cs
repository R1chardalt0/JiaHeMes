using System;

namespace ChargePadLine.Service.Trace.Dto.Order
{
  /// <summary>
  /// MES工单BOM批次明细数据传输对象
  /// </summary>
  public class MesOrderBomBatchItemDto
  {
    /// <summary>
    /// 工单BOM批次明细ID
    /// </summary>
    public Guid OrderBomBatchItemId { get; set; }

    /// <summary>
    /// 工单BOM批次ID
    /// </summary>
    public Guid? OrderBomBatchId { get; set; }

    /// <summary>
    /// SN编码
    /// </summary>
    public string? SnNumber { get; set; }

    /// <summary>
    /// 搜索值
    /// </summary>
    public string? SearchValue { get; set; }

    /// <summary>
    /// 创建者
    /// </summary>
    public string? CreateBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset? CreateTime { get; set; }

    /// <summary>
    /// 更新者
    /// </summary>
    public string? UpdateBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset? UpdateTime { get; set; }

    /// <summary>
    /// 备注信息
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 关联的工单BOM批次信息
    /// </summary>
    public MESOrderBomBatchDto? MesOrderBomBatch { get; set; }
  }
}