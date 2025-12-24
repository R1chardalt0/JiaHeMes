using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePadLine.Entitys.Trace.InventoryManagement
{
    /// <summary>
    /// 库存盘点记录
    /// </summary>
    [Table("mes_inventory_stocktake")]
    public class InventoryStocktake
    {
        [Key]
        public Guid StocktakeId { get; set; }

        /// <summary>
        /// 盘点编号
        /// </summary>
        public string StocktakeCode { get; set; } = "";

        /// <summary>
        /// 仓库编码
        /// </summary>
        public string WarehouseCode { get; set; } = "";

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; } = "";

        /// <summary>
        /// 批次号
        /// </summary>
        public string? BatchCode { get; set; }

        /// <summary>
        /// 系统库存数量
        /// </summary>
        public decimal SystemQuantity { get; set; }

        /// <summary>
        /// 实际盘点数量
        /// </summary>
        public decimal ActualQuantity { get; set; }

        /// <summary>
        /// 差异数量
        /// </summary>
        public decimal DifferenceQuantity { get; set; }

        /// <summary>
        /// 差异原因
        /// </summary>
        public string? DifferenceReason { get; set; }

        /// <summary>
        /// 盘点状态（0-待盘点 1-盘点中 2-已完成）
        /// </summary>
        public StocktakeStatus Status { get; set; } = StocktakeStatus.Pending;

        /// <summary>
        /// 盘点人
        /// </summary>
        public string StocktakeBy { get; set; } = "";

        /// <summary>
        /// 盘点时间
        /// </summary>
        public DateTime? StocktakeTime { get; set; }

        /// <summary>
        /// 复盘人
        /// </summary>
        public string? ReviewBy { get; set; }

        /// <summary>
        /// 复盘时间
        /// </summary>
        public DateTime? ReviewTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 盘点状态枚举
    /// </summary>
    public enum StocktakeStatus
    {
        Pending = 0,    // 待盘点
        Processing = 1, // 盘点中
        Completed = 2   // 已完成
    }
}