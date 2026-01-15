using ChargePadLine.Entitys.Systems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.ProcessRouting
{
  /// <summary>
  /// 站点测试项表
  /// </summary>
  [Table("mes_station_test_project")]
  public class StationTestProject : BaseEntity
  {

    [Key]
    public Guid StationTestProjectId { get; set; }

    /// <summary>
    /// 站点id
    /// </summary>
    public Guid? StationId { get; set; }

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
    /// 是否检查
    /// </summary>
    public bool? IsCheck { get; set; }

  }
}