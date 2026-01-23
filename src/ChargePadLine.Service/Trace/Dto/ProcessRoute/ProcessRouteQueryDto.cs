using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto.ProcessRoute
{
  /// <summary>
  /// 工艺路线查询数据传输对象
  /// </summary>
  public class ProcessRouteQueryDto
  {
    /// <summary>
    /// 工艺路线ID
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// 工艺路线名称
    /// </summary>
    public string? RouteName { get; set; }

    /// <summary>
    /// 工艺路线编码
    /// </summary>
    public string? RouteCode { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 每页条数
    /// </summary>
    public int PageSize { get; set; } = 10;
  }
}
