using System;

namespace ChargePadLine.Service.Trace.Dto.MesSnList
{
  /// <summary>
  /// SN实时状态数据传输对象
  /// </summary>
  public class MesSnListCurrentDto
  {
    /// <summary>
    /// 状态ID
    /// </summary>
    public Guid SNListCurrentId { get; set; }

    /// <summary>
    /// 序列号/产品唯一码
    /// </summary>
    public string SnNumber { get; set; } = "";

    /// <summary>
    /// 产品ID
    /// </summary>
    public Guid ProductListId { get; set; }

    /// <summary>
    /// 工单ID
    /// </summary>
    public Guid OrderListId { get; set; }

    /// <summary>
    /// 当前状态：1-合格，2-不合格，3-已包装，4-已入库，5-跳站
    /// </summary>
    public int StationStatus { get; set; }

    /// <summary>
    /// 当前站点ID
    /// </summary>
    public Guid CurrentStationListId { get; set; }

    /// <summary>
    /// 线体ID
    /// </summary>
    public Guid ProductionLineId { get; set; }

    /// <summary>
    /// 当前设备ID
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// 是否异常
    /// </summary>
    public bool IsAbnormal { get; set; }

    /// <summary>
    /// 异常代码
    /// </summary>
    public string AbnormalCode { get; set; } = "";

    /// <summary>
    /// 异常描述
    /// </summary>
    public string AbnormalDescription { get; set; } = "";

    /// <summary>
    /// 是否锁定（异常锁定）
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// 返工次数
    /// </summary>
    public int ReworkCount { get; set; }

    /// <summary>
    /// 是否正在返工
    /// </summary>
    public bool IsReworking { get; set; }

    /// <summary>
    /// 返工原因
    /// </summary>
    public string ReworkReason { get; set; } = "";

    /// <summary>
    /// 返工时间
    /// </summary>
    public DateTimeOffset? ReworkTime { get; set; }

    /// <summary>
    /// 搜索值
    /// </summary>
    public string SearchValue { get; set; } = "";

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreateBy { get; set; } = "";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset? CreateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string UpdateBy { get; set; } = "";

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset? UpdateTime { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = "";
  }
}