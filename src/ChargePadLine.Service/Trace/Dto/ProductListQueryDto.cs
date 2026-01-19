using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto
{
  /// <summary>
  /// 产品列表查询数据传输对象
  /// </summary>
  public class ProductListQueryDto
  {
    /// <summary>
    /// 产品编码（模糊匹配）
    /// </summary>
    public string? ProductCode { get; set; }

    /// <summary>
    /// 产品名称（模糊匹配）
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// 产品类别（模糊匹配）
    /// </summary>
    public string? ProductType { get; set; }

    /// <summary>
    /// 备注信息（模糊匹配）
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 当前页码（最小值为1）
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 每页记录数（最小值为1）
    /// </summary>
    public int PageSize { get; set; } = 10;
  }
}
