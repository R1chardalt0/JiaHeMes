using ChargePadLine.Entitys.Systems;
using ChargePadLine.Entitys.Trace.Product;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.Order
{
  /// <summary>
  /// MES工单表
  /// </summary>
  [Table("mes_order_list")]
  public class OrderList : BaseEntity
  {
    /// <summary>
    /// 工单ID
    /// </summary>
    [Key]
    [Column("OrderListId")]
    public Guid OrderListId { get; set; }

    /// <summary>
    /// 工单编码
    /// </summary>
    [Required]
    [Column("OrderCode", TypeName = "varchar(50)")]
    public string OrderCode { get; set; } = "";

    /// <summary>
    /// 工单名称
    /// </summary>
    [Required]
    [Column("OrderName", TypeName = "varchar(100)")]
    public string OrderName { get; set; } = "";

    /// <summary>
    /// 产品ID
    /// </summary>
    [Required]
    [Column("ProductListId")]
    public Guid ProductListId { get; set; }
    [ForeignKey("ProductListId")]
    public ProductList? ProductList { get; set; }
    /// <summary>
    /// BOM ID
    /// </summary>
    [Column("BomId")]
    public Guid? BomId { get; set; }

    /// <summary>
    /// 工艺路线ID
    /// </summary>
    [Column("ProcessRouteId")]
    public Guid? ProcessRouteId { get; set; }

    /// <summary>
    /// 工单类型：1-生产工单，2-返工工单
    /// </summary>
    [Column("OrderType")]
    public int OrderType { get; set; } = 1;

    /// <summary>
    /// 工单状态：1-新建，2-已排产，3-生产中，4-已完成，5-已关闭
    /// </summary>
    [Column("OrderStatus")]
    public int OrderStatus { get; set; } = 1;

    /// <summary>
    /// 计划数量
    /// </summary>
    [Column("PlanQty", TypeName = "numeric(15,4)")]
    public decimal PlanQty { get; set; } = 0;

    /// <summary>
    /// 已完成数量
    /// </summary>
    [Column("CompletedQty", TypeName = "numeric(15,4)")]
    public decimal CompletedQty { get; set; } = 0;

    /// <summary>
    /// 计划开始时间
    /// </summary>
    [Column("PlanStartTime")]
    public DateTime? PlanStartTime { get; set; }

    /// <summary>
    /// 计划结束时间
    /// </summary>
    [Column("PlanEndTime")]
    public DateTime? PlanEndTime { get; set; }

    /// <summary>
    /// 实际开始时间
    /// </summary>
    [Column("ActualStartTime")]
    public DateTime? ActualStartTime { get; set; }

    /// <summary>
    /// 实际结束时间
    /// </summary>
    [Column("ActualEndTime")]
    public DateTime? ActualEndTime { get; set; }

    /// <summary>
    /// 优先级：1-紧急，3-高，5-中，7-低
    /// </summary>
    [Column("PriorityLevel")]
    public int PriorityLevel { get; set; } = 5;
  }
}
