using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePadLine.Entitys.Trace.QualityManagement
{
    /// <summary>
    /// 质量检验标准
    /// </summary>
    [Table("mes_quality_inspection_standard")]
    public class QualityInspectionStandard
    {
        [Key]
        public Guid StandardId { get; set; }

        /// <summary>
        /// 标准编号
        /// </summary>
        public string StandardCode { get; set; } = "";

        /// <summary>
        /// 标准名称
        /// </summary>
        public string StandardName { get; set; } = "";

        /// <summary>
        /// 产品编码
        /// </summary>
        public string ProductCode { get; set; } = "";

        /// <summary>
        /// 工序ID
        /// </summary>
        public Guid? OperationId { get; set; }

        /// <summary>
        /// 检验类型（首检、巡检、末检、成品检）
        /// </summary>
        public InspectionType InspectionType { get; set; }

        /// <summary>
        /// 检验方式（全检、抽检）
        /// </summary>
        public InspectionMethod InspectionMethod { get; set; }

        /// <summary>
        /// 抽检比例（百分比，仅抽检时有效）
        /// </summary>
        public decimal? SamplingRate { get; set; }

        /// <summary>
        /// 状态（0-禁用 1-启用）
        /// </summary>
        public int Status { get; set; } = 1;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 检验项目
        /// </summary>
        public List<QualityInspectionItem> InspectionItems { get; set; } = new();
    }

    /// <summary>
    /// 检验类型枚举
    /// </summary>
    public enum InspectionType
    {
        FirstInspection = 1,    // 首检
        PatrolInspection = 2,   // 巡检
        FinalInspection = 3,    // 末检
        FinishedInspection = 4  // 成品检
    }

    /// <summary>
    /// 检验方式枚举
    /// </summary>
    public enum InspectionMethod
    {
        FullInspection = 1,     // 全检
        SamplingInspection = 2 // 抽检
    }
}