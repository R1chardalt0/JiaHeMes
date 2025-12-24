using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePadLine.Entitys.Trace.QualityManagement
{
    /// <summary>
    /// 质量检验项目
    /// </summary>
    [Table("mes_quality_inspection_item")]
    public class QualityInspectionItem
    {
        [Key]
        public Guid ItemId { get; set; }

        /// <summary>
        /// 检验标准ID
        /// </summary>
        public Guid StandardId { get; set; }

        /// <summary>
        /// 检验项目名称
        /// </summary>
        public string ItemName { get; set; } = "";

        /// <summary>
        /// 检验项目编码
        /// </summary>
        public string ItemCode { get; set; } = "";

        /// <summary>
        /// 检验类型（外观、尺寸、功能等）
        /// </summary>
        public string InspectionCategory { get; set; } = "";

        /// <summary>
        /// 检验工具
        /// </summary>
        public string? InspectionTool { get; set; }

        /// <summary>
        /// 标准值
        /// </summary>
        public string? StandardValue { get; set; }

        /// <summary>
        /// 上限值
        /// </summary>
        public decimal? UpperLimit { get; set; }

        /// <summary>
        /// 下限值
        /// </summary>
        public decimal? LowerLimit { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string? Unit { get; set; }

        /// <summary>
        /// 是否关键检验项
        /// </summary>
        public bool IsKeyItem { get; set; } = false;

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual QualityInspectionStandard? InspectionStandard { get; set; }
    }
}