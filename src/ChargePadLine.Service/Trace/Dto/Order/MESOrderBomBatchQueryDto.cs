using System;

namespace ChargePadLine.Service.Trace.Dto.Order
{
  /// <summary>
  /// MES工单BOM批次查询数据传输对象
  /// </summary>
  public class MESOrderBomBatchQueryDto
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
    /// 工单ID
    /// </summary>
    public Guid? OrderListId { get; set; }

    /// <summary>
    /// 搜索值
    /// </summary>
    public string? SearchValue { get; set; }

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