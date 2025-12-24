using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePadLine.Entitys.Trace.QualityManagement
{
    /// <summary>
    /// 不良品记录
    /// </summary>
    [Table("mes_quality_defect_record")]
    public class DefectRecord
    {
        [Key]
        public Guid DefectId { get; set; }

        /// <summary>
        /// 不良品编号
        /// </summary>
        public string DefectCode { get; set; } = "";

        /// <summary>
        /// 产品编码/SFC
        /// </summary>
        public string ProductCode { get; set; } = "";

        /// <summary>
        /// 工单ID
        /// </summary>
        public int? WorkOrderId { get; set; }

        /// <summary>
        /// 工序ID
        /// </summary>
        public Guid? OperationId { get; set; }

        /// <summary>
        /// 不良类型
        /// </summary>
        public string DefectType { get; set; } = "";

        /// <summary>
        /// 不良原因
        /// </summary>
        public string DefectReason { get; set; } = "";

        /// <summary>
        /// 不良描述
        /// </summary>
        public string DefectDescription { get; set; } = "";

        /// <summary>
        /// 不良等级（轻微、中等、严重）
        /// </summary>
        public DefectLevel DefectLevel { get; set; }

        /// <summary>
        /// 发现人员工号
        /// </summary>
        public string FoundBy { get; set; } = "";

        /// <summary>
        /// 发现时间
        /// </summary>
        public DateTime FoundTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 处理状态（0-待处理 1-处理中 2-已处理 3-报废）
        /// </summary>
        public DefectStatus Status { get; set; } = DefectStatus.Pending;

        /// <summary>
        /// 处理措施
        /// </summary>
        public string? HandleMeasures { get; set; }

        /// <summary>
        /// 处理人员工号
        /// </summary>
        public string? HandledBy { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        public DateTime? HandledTime { get; set; }

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
    /// 不良等级枚举
    /// </summary>
    public enum DefectLevel
    {
        Minor = 1,      // 轻微
        Moderate = 2,   // 中等
        Severe = 3      // 严重
    }

    /// <summary>
    /// 不良状态枚举
    /// </summary>
    public enum DefectStatus
    {
        Pending = 0,    // 待处理
        Processing = 1, // 处理中
        Completed = 2,  // 已处理
        Scrapped = 3    // 报废
    }
}