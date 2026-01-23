using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto.ProcessRoute
{
  /// <summary>
  /// 工艺路线子项创建数据传输对象
  /// </summary>
  public class ProcessRouteItemCreateDto
  {
    /// <summary>
    /// 主表ID
    /// </summary>
    public Guid HeadId { get; set; }

    /// <summary>
    /// 站点编码
    /// </summary>
    public string StationCode { get; set; } = "";

    /// <summary>
    /// 是否必过站
    /// </summary>
    public bool MustPassStation { get; set; }

    /// <summary>
    /// 验证站点PASS记录，可多选，用逗号隔开
    /// </summary>
    public string CheckStationList { get; set; } = "";

    /// <summary>
    /// 是否首站
    /// </summary>
    public bool FirstStation { get; set; } = false;

    /// <summary>
    /// 是否校验Check StationList中的全部
    /// </summary>
    public bool CheckAll { get; set; } = false;

    /// <summary>
    /// 工艺路线序号
    /// </summary>
    public int? RouteSeq { get; set; } = 0;

    /// <summary>
    /// 最大NG次数
    /// </summary>
    public int? MaxNGCount { get; set; } = 0;

    /// <summary>
    /// 备注信息
    /// </summary>
    public string? Remark { get; set; }
  }
}
