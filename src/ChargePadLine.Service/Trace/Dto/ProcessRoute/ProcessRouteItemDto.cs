using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto.ProcessRoute
{
  /// <summary>
  /// 工艺路线子项数据传输对象
  /// </summary>
  public class ProcessRouteItemDto
  {
    /// <summary>
    /// 工艺路线子表ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 工艺路线ID
    /// </summary>
    public Guid HeadId { get; set; }

    /// <summary>
    /// 工艺路线编号
    /// </summary>
    public string StationCode { get; set; } = "";

    /// <summary>
    /// 是否必须通过
    /// </summary>
    public bool MustPassStation { get; set; } = false;

    /// <summary>
    /// 检查站列表
    /// </summary>
    public string CheckStationList { get; set; } = "";

    /// <summary>
    /// 是否首站
    /// </summary>
    public bool FirstStation { get; set; } = false;

    /// <summary>
    /// 备注信息
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset? CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset? UpdateTime { get; set; }
  }
}
