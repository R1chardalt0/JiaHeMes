using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePadLine.Entitys.Trace.QualityManagement
{
    /// <summary>
    /// 质量检验明细
    /// </summary>
    [Table("mes_quality_inspection_detail")]
    public class QualityInspectionDetail
    {
        [Key]
        public Guid DetailId { get; set; }

        /// <summary>
        /// 检验记录ID
        /// </summary>
        public Guid RecordId { get; set; }

        /// <summary>
        /// 检验项目ID
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// 检验值
        /// </summary>
        public string? InspectionValue { get; set; }

        /// <summary>
        /// 检验结果（0-不合格 1-合格）
        /// </summary>
        public bool IsQualified { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual QualityInspectionRecord? InspectionRecord { get; set; }
        public virtual QualityInspectionItem? InspectionItem { get; set; }
    }
}