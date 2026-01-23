using System;

namespace ChargePadLine.Service.Trace.Dto.Order
{
  /// <summary>
  /// MES工单BOM批次明细查询数据传输对象
  /// </summary>
  public class MESOrderBomBatchItemQueryDto
  {
    /// <summary>
    /// 工单BOM批次明细ID
    /// </summary>
    public Guid OrderBomBatchItemId { get; set; }

    /// <summary>
    /// 工单BOM批次ID
    /// </summary>
    public Guid OrderBomBatchId { get; set; }

    /// <summary>
    /// SN编码
    /// </summary>
    public string? SnNumber { get; set; }

    /// <summary>
    /// 搜索值
    /// </summary>
    public string? SearchValue { get; set; }

    /// <summary>
    /// 是否解绑
    /// </summary>
    public bool? IsUnbind { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? SortField { get; set; }

    /// <summary>
    /// 排序方向：asc/desc
    /// </summary>
    public string? SortOrder { get; set; }
  }
}