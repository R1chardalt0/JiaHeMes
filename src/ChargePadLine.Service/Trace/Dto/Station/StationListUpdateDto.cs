using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto.Station
{
  /// <summary>
  /// 站点更新数据传输对象
  /// </summary>
  public class StationListUpdateDto
  {
    /// <summary>
    /// 站点ID
    /// </summary>
    [Required(ErrorMessage = "站点ID不能为空")]
    public Guid StationId { get; set; }

    /// <summary>
    /// 站点名称
    /// </summary>
    [Required(ErrorMessage = "站点名称不能为空")]
    public string StationName { get; set; } = "";

    /// <summary>
    /// 站点编号
    /// </summary>
    [Required(ErrorMessage = "站点编号不能为空")]
    public string StationCode { get; set; } = "";

    /// <summary>
    /// 备注信息
    /// </summary>
    public string? Remark { get; set; }
  }
}
