using System;

namespace ChargePadLine.Service.Trace.Dto.Order
{
  /// <summary>
  /// MES工单BOM批次数据传输对象
  /// </summary>
  public class MESOrderBomBatchDto
  {
    /// <summary>
    /// 工单BOM批次ID
    /// </summary>
    public Guid OrderBomBatchId { get; set; }

    /// <summary>
    /// 物料ID
    /// </summary>
    public Guid? ProductListId { get; set; }

    /// <summary>
    /// 批次编码
    /// </summary>
    public string? BatchCode { get; set; }

    /// <summary>
    /// 站点
    /// </summary>
    public Guid? StationListId { get; set; }

    /// <summary>
    /// 状态：1-正常，2-已使用完，3-已下料
    /// </summary>
    public int? OrderBomBatchStatus { get; set; }

    /// <summary>
    /// 批次数量
    /// </summary>
    public decimal? BatchQty { get; set; }

    /// <summary>
    /// 已使用数量
    /// </summary>
    public decimal? CompletedQty { get; set; }

    /// <summary>
    /// 工单ID
    /// </summary>
    public Guid? OrderListId { get; set; }

    /// <summary>
    /// 设备ID
    /// </summary>
    public Guid? ResourceId { get; set; }

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
  }
}