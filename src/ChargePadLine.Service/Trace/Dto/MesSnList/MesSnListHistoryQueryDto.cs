using System;

namespace ChargePadLine.Service.Trace.Dto.MesSnList
{
  /// <summary>
  /// SN历史记录查询数据传输对象
  /// </summary>
  public class MesSnListHistoryQueryDto
  {
    /// <summary>
    /// 状态ID
    /// </summary>
    public Guid? SNListHistoryId { get; set; }

    /// <summary>
    /// 序列号/产品唯一码
    /// </summary>
    public string SnNumber { get; set; } = "";

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
    /// 异常代码
    /// </summary>
    public string AbnormalCode { get; set; } = "";

    /// <summary>
    /// 是否锁定（异常锁定）
    /// </summary>
    public bool? IsLocked { get; set; }

    /// <summary>
    /// 返工次数
    /// </summary>
    public int? ReworkCount { get; set; }

    /// <summary>
    /// 是否正在返工
    /// </summary>
    public bool? IsReworking { get; set; }

    /// <summary>
    /// 返工原因
    /// </summary>
    public string ReworkReason { get; set; } = "";

    /// <summary>
    /// 返工时间
    /// </summary>
    public DateTimeOffset? ReworkTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreateBy { get; set; } = "";

    /// <summary>
    /// 创建时间开始
    /// </summary>
    public DateTimeOffset? CreateTimeStart { get; set; }

    /// <summary>
    /// 创建时间结束
    /// </summary>
    public DateTimeOffset? CreateTimeEnd { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string UpdateBy { get; set; } = "";

    /// <summary>
    /// 更新时间开始
    /// </summary>
    public DateTimeOffset? UpdateTimeStart { get; set; }

    /// <summary>
    /// 更新时间结束
    /// </summary>
    public DateTimeOffset? UpdateTimeEnd { get; set; }

    /// <summary>
    /// 搜索值
    /// </summary>
    public string SearchValue { get; set; } = "";

    /// <summary>
    /// 页码
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 每页条数
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 排序字段
    /// </summary>
    public string SortField { get; set; } = "";

    /// <summary>
    /// 排序方向
    /// </summary>
    public string SortOrder { get; set; } = "";
  }
}