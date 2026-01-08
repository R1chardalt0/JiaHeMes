using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto.Order
{
  /// <summary>
  /// 工单更新数据传输对象
  /// </summary>
  public class OrderListUpdateDto
  {
    /// <summary>
    /// 工单ID
    /// </summary>
    public Guid OrderListId { get; set; }

    /// <summary>
    /// 工单编码
    /// </summary>
    public string OrderCode { get; set; } = "";

    /// <summary>
    /// 工单名称
    /// </summary>
    public string OrderName { get; set; } = "";

    /// <summary>
    /// 产品ID
    /// </summary>
    public Guid ProductListId { get; set; }

    /// <summary>
    /// BOM ID
    /// </summary>
    public Guid? BomId { get; set; }

    /// <summary>
    /// 工艺路线ID
    /// </summary>
    public Guid? ProcessRouteId { get; set; }

    /// <summary>
    /// 工单类型：1-生产工单，2-返工工单
    /// </summary>
    public int OrderType { get; set; }

    /// <summary>
    /// 工单状态：1-新建，2-已排产，3-生产中，4-已完成，5-已关闭
    /// </summary>
    public int OrderStatus { get; set; }

    /// <summary>
    /// 计划数量
    /// </summary>
    public decimal PlanQty { get; set; }

    /// <summary>
    /// 已完成数量
    /// </summary>
    public decimal CompletedQty { get; set; }

    /// <summary>
    /// 计划开始时间
    /// </summary>
    public DateTime? PlanStartTime { get; set; }

    /// <summary>
    /// 计划结束时间
    /// </summary>
    public DateTime? PlanEndTime { get; set; }

    // /// <summary>
    // /// 实际开始时间
    // /// </summary>
    // public DateTime? ActualStartTime { get; set; }

    // /// <summary>
    // /// 实际结束时间
    // /// </summary>
    // public DateTime? ActualEndTime { get; set; }

    /// <summary>
    /// 优先级：1-紧急，3-高，5-中，7-低
    /// </summary>
    public int PriorityLevel { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
  }
}
