using ChargePadLine.Entitys.Systems;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePadLine.Entitys.Trace.ProcessRouting
{
  /// <summary>
  /// 工艺路线工位测试表
  /// </summary>
  [Table("mes_processRoute_item_station_test")]
  public class ProcessRouteItemTest : BaseEntity
  {
    /// <summary>
    /// 工艺路线工位测试ID
    /// </summary>
    [Key]
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
    public string? Units { get; set; }

    /// <summary>
    /// 测试项
    /// </summary>
    public string? ParametricKey { get; set; }

    /// <summary>
    /// 是否检查
    /// </summary>
    public bool? IsCheck { get; set; }
  }
}