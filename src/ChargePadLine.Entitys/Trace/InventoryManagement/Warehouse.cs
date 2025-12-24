using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePadLine.Entitys.Trace.InventoryManagement
{
    /// <summary>
    /// 仓库信息
    /// </summary>
    [Table("mes_inventory_warehouse")]
    public class Warehouse
    {
        [Key]
        public Guid WarehouseId { get; set; }

        /// <summary>
        /// 仓库编码
        /// </summary>
        public string WarehouseCode { get; set; } = "";

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string WarehouseName { get; set; } = "";

        /// <summary>
        /// 仓库类型（原材料仓、半成品仓、成品仓、不良品仓）
        /// </summary>
        public WarehouseType WarehouseType { get; set; }

        /// <summary>
        /// 负责人
        /// </summary>
        public string? Manager { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string? ContactPhone { get; set; }

        /// <summary>
        /// 仓库地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 状态（0-禁用 1-启用）
        /// </summary>
        public int Status { get; set; } = 1;

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
    /// 仓库类型枚举
    /// </summary>
    public enum WarehouseType
    {
        RawMaterial = 1,    // 原材料仓
        SemiFinished = 2,   // 半成品仓
        FinishedGoods = 3,  // 成品仓
        Defective = 4,      // 不良品仓
        Quarantine = 5      // 待检仓
    }
}