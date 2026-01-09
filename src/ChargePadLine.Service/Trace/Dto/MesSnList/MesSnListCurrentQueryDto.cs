using System;

namespace ChargePadLine.Service.Trace.Dto.MesSnList
{
  /// <summary>
  /// SN实时状态查询数据传输对象
  /// </summary>
  public class MesSnListCurrentQueryDto
  {
    /// <summary>
    /// 序列号/产品唯一码
    /// </summary>
    public string? SnNumber { get; set; }

    /// <summary>
    /// 产品ID
    /// </summary>
    public Guid? ProductListId { get; set; }

    /// <summary>
    /// 工单ID
    /// </summary>
    public Guid? OrderListId { get; set; }

    /// <summary>
    /// 当前状态：1-合格，2-不合格，3-已包装，4-已入库，5-跳站
    /// </summary>
    public int? StationStatus { get; set; }

    /// <summary>
    /// 当前站点ID
    /// </summary>
    public Guid? CurrentStationListId { get; set; }

    /// <summary>
    /// 线体ID
    /// </summary>
    public Guid? ProductionLineId { get; set; }

    /// <summary>
    /// 当前设备ID
    /// </summary>
    public Guid? ResourceId { get; set; }

    /// <summary>
    /// 是否异常
    /// </summary>
    public bool? IsAbnormal { get; set; }

    /// <summary>
    /// 是否锁定（异常锁定）
    /// </summary>
    public bool? IsLocked { get; set; }

    /// <summary>
    /// 是否正在返工
    /// </summary>
    public bool? IsReworking { get; set; }

    /// <summary>
    /// 搜索值
    /// </summary>
    public string? SearchValue { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

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
    /// 排序方向：asc 或 desc
    /// </summary>
    public string? SortOrder { get; set; }
  }
}