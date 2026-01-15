using System;

namespace ChargePadLine.Service.Trace.Dto.ProcessRoute
{
  /// <summary>
  /// 工艺路线工位测试数据传输对象
  /// </summary>
  public class ProcessRouteItemTestDto
  {
    /// <summary>
    /// 工艺路线工位测试ID
    /// </summary>
    public Guid ProRouteItemStationTestId { get; set; }

    /// <summary>
    /// 工艺路线明细id
    /// </summary>
    public Guid? ProcessRouteItemId { get; set; }

    /// <summary>
    /// 上限
    /// </summary>
    public decimal? UpperLimit { get; set; }

    /// <summary>
    /// 下限
    /// </summary>
    public decimal? LowerLimit { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    public string? Units { get; set; } = "";

    /// <summary>
    /// 测试项
    /// </summary>
    public string? ParametricKey { get; set; } = "";

    /// <summary>
    /// 创建者
    /// </summary>
    public string? CreateBy { get; set; } = "";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset? CreateTime { get; set; }

    /// <summary>
    /// 更新者
    /// </summary>
    public string? UpdateBy { get; set; } = "";

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset? UpdateTime { get; set; }

    /// <summary>
    /// 备注信息
    /// </summary>
    public string? Remark { get; set; } = "";

    /// <summary>
    /// 是否检查
    /// </summary>
    public bool? IsCheck { get; set; }
  }
}