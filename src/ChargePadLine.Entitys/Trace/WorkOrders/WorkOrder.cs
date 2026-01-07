using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.BOM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.WorkOrders
{
    /// <summary>
    /// 工单
    /// </summary>|
    [Table("mes_prod_workorder")]
    public class WorkOrder
    {
        public int Id { get; set; }
        public WorkOrderCode Code { get; set; } = "";

        public ProductCode ProductCode { get; set; } = "";

        #region 工单所使用的BOM
        public Guid BomRecipeId { get; set; }
        public BomList? BomList { get; set; } = null!;
        #endregion

        /// <summary>
        /// 是否是无限生产?
        /// </summary>
        public bool IsInfinite { get; set; }

        /// <summary>
        /// 生产数量。仅在有限生产条件下才有意义
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

        #region 创建工单
        public static WorkOrder MakeInfiniteWorkOrder(WorkOrderCode workOrderCode, BomList bomList)
        {
            var x = new WorkOrder
            {
                Code = workOrderCode,
                BomList = bomList,
                BomRecipeId = bomList.BomId,
                PerTraceInfo = 0,
                DocStatus = WorkOrderDocStatus.Drafting,
            };
            x.SetWorkOrderAmount(new WorkOrderAmount.Infinity());
            return x;
        }

        public static WorkOrder MakeWorkOrder(WorkOrderCode workOrderCode, BomList bomList, decimal quota, decimal perTraceInfo)
        {
            var x = new WorkOrder
            {
                Code = workOrderCode,
                BomList = bomList,
                BomRecipeId = bomList.BomId,
                DocStatus = WorkOrderDocStatus.Drafting,
                PerTraceInfo = perTraceInfo,
            };
            x.SetWorkOrderAmount(new WorkOrderAmount.Quota(quota));
            return x;
        }
        #endregion
    }

  public class WorkOrderEntityTypeConfiguration : IEntityTypeConfiguration<WorkOrder>
  {
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
      // 配置Id为主键并设置为自增
      builder.HasKey(e => e.Id);
      builder.Property(e => e.Id).ValueGeneratedOnAdd();

      builder.OwnsOne(e => e.Code, e =>
      {
        e.HasIndex(e => e.Value);
      });

      // 配置外键关联到BomList
      builder.HasOne(e => e.BomList)
             .WithMany()
             .HasForeignKey(e => e.BomRecipeId)
             .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
