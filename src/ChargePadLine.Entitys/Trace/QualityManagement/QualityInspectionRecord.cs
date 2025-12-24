using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePadLine.Entitys.Trace.QualityManagement
{
    /// <summary>
    /// 质量检验记录
    /// </summary>
    [Table("mes_quality_inspection_record")]
    public class QualityInspectionRecord
    {
        [Key]
        public Guid RecordId { get; set; }

        /// <summary>
        /// 检验标准ID
        /// </summary>
        public Guid StandardId { get; set; }

        /// <summary>
        /// 工单ID
        /// </summary>
        public int? WorkOrderId { get; set; }

        /// <summary>
        /// 产品追溯ID
        /// </summary>
        public Guid? ProductTraceId { get; set; }

        /// <summary>
        /// 产品编码/SFC
        /// </summary>
        public string ProductCode { get; set; } = "";

        /// <summary>
        /// 工序ID
        /// </summary>
        public Guid? OperationId { get; set; }

        /// <summary>
        /// 检验员工号
        /// </summary>
        public string InspectorCode { get; set; } = "";

        /// <summary>
        /// 检验时间
        /// </summary>
        public DateTime InspectionTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 检验结果（0-不合格 1-合格）
        /// </summary>
        public bool IsQualified { get; set; }

        /// <summary>
        /// 检验数量
        /// </summary>
        public int InspectionQuantity { get; set; } = 1;

        /// <summary>
        /// 合格数量
        /// </summary>
        public int QualifiedQuantity { get; set; } = 0;

        /// <summary>
        /// 不合格数量
        /// </summary>
        public int UnqualifiedQuantity { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual QualityInspectionStandard? InspectionStandard { get; set; }
        public List<QualityInspectionDetail> InspectionDetails { get; set; } = new();
    }
}