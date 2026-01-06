using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto
{
  /// <summary>
  /// 站点数据传输对象
  /// </summary>
  public class StationListDto
  {
    /// <summary>
    /// 站点ID
    /// </summary>
    public Guid StationId { get; set; }

    /// <summary>
    /// 站点名称
    /// </summary>
    public string StationName { get; set; } = "";

    /// <summary>
    /// 站点编号
    /// </summary>
    public string StationCode { get; set; } = "";

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
  }
}
