using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto.Station
{
  /// <summary>
  /// 站点测试项更新数据传输对象
  /// </summary>
  public class StationTestProjectUpdateDto
  {
    /// <summary>
    /// 测试项ID
    /// </summary>
    [Required(ErrorMessage = "测试项ID不能为空")]
    public Guid StationTestProjectId { get; set; }

    /// <summary>
    /// 站点id
    /// </summary>
    [Required(ErrorMessage = "站点ID不能为空")]
    public Guid StationId { get; set; }

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
    [Required(ErrorMessage = "测试项不能为空")]
    public string ParametricKey { get; set; } = "";

    /// <summary>
    /// 搜索值
    /// </summary>
    public string? SearchValue { get; set; }

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