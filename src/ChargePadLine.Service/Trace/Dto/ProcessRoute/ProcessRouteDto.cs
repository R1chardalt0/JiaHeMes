using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto.ProcessRoute
{
  /// <summary>
  /// 工艺路线数据传输对象
  /// </summary>
  public class ProcessRouteDto
  {
    /// <summary>
    /// 工艺路线ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 工艺路线名称
    /// </summary>
    public string RouteName { get; set; } = "";

    /// <summary>
    /// 工艺路线编码
    /// </summary>
    public string RouteCode { get; set; } = "";

    /// <summary>
    /// 状态 0-启用 1-关闭
    /// </summary>
    public int Status { get; set; } = 0;

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
