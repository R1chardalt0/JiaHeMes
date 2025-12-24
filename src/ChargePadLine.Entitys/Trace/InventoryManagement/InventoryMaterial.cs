using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePadLine.Entitys.Trace.InventoryManagement
{
    /// <summary>
    /// 库存物料
    /// </summary>
    [Table("mes_inventory_material")]
    public class InventoryMaterial
    {
        [Key]
        public Guid InventoryId { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; } = "";

        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; } = "";

        /// <summary>
        /// 物料类型（原材料、半成品、成品）
        /// </summary>
        public MaterialType MaterialType { get; set; }

        /// <summary>
        /// 仓库编码
        /// </summary>
        public string WarehouseCode { get; set; } = "";

        /// <summary>
        /// 库区编码
        /// </summary>
        public string? AreaCode { get; set; }

        /// <summary>
        /// 库位编码
        /// </summary>
        public string? LocationCode { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string? BatchCode { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; } = "";

        /// <summary>
        /// 单价
        /// </summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ProductionDate { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// 库存状态（正常、冻结、待检、报废）
        /// </summary>
        public InventoryStatus Status { get; set; } = InventoryStatus.Normal;

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
    /// 物料类型枚举
    /// </summary>
    public enum MaterialType
    {
        RawMaterial = 1,    // 原材料
        SemiFinished = 2,   // 半成品
        FinishedGoods = 3   // 成品
    }

    /// <summary>
    /// 库存状态枚举
    /// </summary>
    public enum InventoryStatus
    {
        Normal = 1,     // 正常
        Frozen = 2,     // 冻结
        Pending = 3,    // 待检
        Scrap = 4       // 报废
    }
}