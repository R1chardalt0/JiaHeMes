using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto.BOM
{
  /// <summary>
  /// BOM列表查询数据传输对象
  /// </summary>
  public class BomListQueryDto
  {
    /// <summary>
    /// BOM ID
    /// </summary>
    public Guid? BomId { get; set; }

    /// <summary>
    /// BOM名称
    /// </summary>
    public string? BomName { get; set; }

    /// <summary>
    /// BOM编码
    /// </summary>
    public string? BomCode { get; set; }

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
