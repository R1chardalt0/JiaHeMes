using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto.ProcessRoute
{
  /// <summary>
  /// 工艺路线子项更新数据传输对象
  /// </summary>
  public class ProcessRouteItemUpdateDto
  {
    /// <summary>
    /// 工艺路线子项ID
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
    public bool MustPassStation { get; set; }

    /// <summary>
    /// 检查站列表
    /// </summary>
    public string CheckStationList { get; set; } = "";

    /// <summary>
    /// 是否首站
    /// </summary>
    public bool FirstStation { get; set; }

    /// <summary>
    /// 是否全检
    /// </summary>
    public bool CheckAll { get; set; }

    /// <summary>
    /// 备注信息
    /// </summary>
    public string? Remark { get; set; }
  }
}
