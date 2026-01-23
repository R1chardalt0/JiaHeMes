using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto.Station
{
  /// <summary>
  /// 站点测试项查询数据传输对象
  /// </summary>
  public class StationTestProjectQueryDto
  {
    /// <summary>
    /// 站点ID
    /// </summary>
    public Guid? StationId { get; set; }

    /// <summary>
    /// 测试项
    /// </summary>
    public string ParametricKey { get; set; } = "";

    /// <summary>
    /// 搜索值
    /// </summary>
    public string SearchValue { get; set; } = "";

    /// <summary>
    /// 创建时间起始（包含）
    /// </summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary>
    /// 创建时间结束（包含）
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// 当前页码（最小值为1）
    /// </summary>
    public int Current { get; set; } = 1;

    /// <summary>
    /// 每页记录数（最小值为1）
    /// </summary>
    public int PageSize { get; set; } = 10;
  }
}