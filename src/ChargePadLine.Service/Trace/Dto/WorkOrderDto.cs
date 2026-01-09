using ChargePadLine.Entitys.Trace.WorkOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto
{
  /// <summary>
  /// 工单数据传输对象
  /// </summary>
  public class WorkOrderDto
  {
    /// <summary>
    /// 工单ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 工单编码
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// 产品编码
    /// </summary>
    public string ProductCode { get; set; } = "";

    /// <summary>
    /// BOM配方ID
    /// </summary>
    public Guid BomRecipeId { get; set; }

    /// <summary>
    /// 是否无限生产
    /// </summary>
    public bool IsInfinite { get; set; }

    /// <summary>
    /// 生产数量
    /// </summary>
    public decimal WorkOrderAmount { get; set; }

    /// <summary>
    /// 每个追踪信息的完成增量
    /// </summary>
    public decimal PerTraceInfo { get; set; }

    /// <summary>
    /// 工单状态
    /// </summary>
    public WorkOrderDocStatus DocStatus { get; set; }

    /// <summary>
    /// BOM配方名称
    /// </summary>
    public string BomRecipeName { get; set; } = "";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
  }
}