using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.TraceInfo
{
    /// <summary>
    /// 生产BOM信息项
    /// </summary>
    public class TraceBomItem
    {
        public Guid Id { get; set; }
        public uint Vsn { get; set; }

        #region 外键
        //public int TraceInfoId { get; set; }
        public Guid TraceInfoId { get; set; }
        public TraceInfo? TraceInfo { get; set; }
        #endregion


        /// <summary>
        /// BOM子项的编号。该编号在父BOM范围内唯一
        /// </summary>
        public BomItemCode BomItemCode { get; set; } = "";

        #region 
        public int BomId { get; set; }
        public BomRecipe? Bom { get; set; }
        #endregion

        #region 物料信息
        /// <summary>
        /// 物料编码
        /// </summary>
        public MaterialCode MaterialCode { get; set; } = null!;
        /// <summary>
        /// 缓存的物料名称
        /// </summary>
        public string MaterialName = "";
        /// <summary>
        /// 缓存的计量单位
        /// </summary>
        public MeasureUnit MeasureUnit { get; set; } = null!;
        #endregion

        public decimal Quota { get; set; }

        public string Description = "";


        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset DeletedAt { get; set; }
        public bool IsDeleted { get; set; }


        /// <summary>
        /// 库存码
        /// </summary>
        public SKU SKU { get; set; } = "";

        /// <summary>
        /// 消耗量
        /// </summary>
        public decimal Consumption { get; set; }
    }

}
