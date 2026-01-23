using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Entitys.Trace.Recipes.Entities;

namespace ChargePadLine.Service.Trace.Dto
{
  /// <summary>
  /// 创建工单数据传输对象
  /// </summary>
  public class CreateWorkOrderDto
  {
    /// <summary>
    /// 工单编码
    /// </summary>
    public string? Code { get; set; }

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
    public decimal PerTraceInfo { get; set; } = 1;

  }
}