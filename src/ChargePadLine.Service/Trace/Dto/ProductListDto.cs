using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto
{
  /// <summary>
  /// 产品列表数据传输对象
  /// </summary>
  public class ProductListDto
  {
    /// <summary>
    /// 产品ID
    /// </summary>
    public Guid ProductListId { get; set; }

    /// <summary>
    /// 产品编码
    /// </summary>
    public string ProductCode { get; set; } = "";

    /// <summary>
    /// 产品名称
    /// </summary>
    public string ProductName { get; set; } = "";

    /// <summary>
    /// BOMID
    /// </summary>
    public Guid? BomId { get; set; }

    /// <summary>
    /// 工艺路线ID
    /// </summary>
    public Guid? ProcessRouteId { get; set; }

    /// <summary>
    /// 产品类别
    /// </summary>
    public string ProductType { get; set; } = "";

    /// <summary>
    /// 备注信息
    /// </summary>
    public string? Remark { get; set; }

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
  }
}
