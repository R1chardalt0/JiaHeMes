using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePadLine.Entitys.Trace.InventoryManagement
{
    /// <summary>
    /// 库存操作记录
    /// </summary>
    [Table("mes_inventory_transaction")]
    public class InventoryTransaction
    {
        [Key]
        public Guid TransactionId { get; set; }

        /// <summary>
        /// 交易编号
        /// </summary>
        public string TransactionCode { get; set; } = "";

        /// <summary>
        /// 库存ID
        /// </summary>
        public Guid InventoryId { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; } = "";

        /// <summary>
        /// 操作类型（入库、出库、移库、盘点、报废）
        /// </summary>
        public TransactionType TransactionType { get; set; }

        /// <summary>
        /// 操作数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 操作前数量
        /// </summary>
        public decimal QuantityBefore { get; set; }

        /// <summary>
        /// 操作后数量
        /// </summary>
        public decimal QuantityAfter { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; } = "";

        /// <summary>
        /// 源仓库编码
        /// </summary>
        public string? SourceWarehouse { get; set; }

        /// <summary>
        /// 目标仓库编码
        /// </summary>
        public string? TargetWarehouse { get; set; }

        /// <summary>
        /// 源库位编码
        /// </summary>
        public string? SourceLocation { get; set; }

        /// <summary>
        /// 目标库位编码
        /// </summary>
        public string? TargetLocation { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string? BatchCode { get; set; }

        /// <summary>
        /// 关联工单ID
        /// </summary>
        public int? WorkOrderId { get; set; }

        /// <summary>
        /// 关联生产订单号
        /// </summary>
        public string? ProductionOrder { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string OperatedBy { get; set; } = "";

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OperatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual InventoryMaterial? InventoryMaterial { get; set; }
    }

    /// <summary>
    /// 库存操作类型枚举
    /// </summary>
    public enum TransactionType
    {
        Inbound = 1,        // 入库
        Outbound = 2,       // 出库
        Transfer = 3,       // 移库
        Stocktake = 4,      // 盘点
        Scrap = 5,          // 报废
        Return = 6,         // 退料
        Replenishment = 7   // 补料
    }
}