using System;
using System.Collections.Generic;

namespace ChargePadLine.Service.Trace.Dto
{
    /// <summary>
    /// 每小时产出统计DTO
    /// </summary>
    public class HourlyOutputDto
    {
        /// <summary>
        /// 小时（0-23）
        /// </summary>
        public int Hour { get; set; }

        /// <summary>
        /// 产出数量
        /// </summary>
        public int OutputQuantity { get; set; }

        /// <summary>
        /// 合格数量
        /// </summary>
        public int PassQuantity { get; set; }

        /// <summary>
        /// 不合格数量
        /// </summary>
        public int FailQuantity { get; set; }

        /// <summary>
        /// 小时开始时间
        /// </summary>
        public DateTime HourStartTime { get; set; }

        /// <summary>
        /// 小时结束时间
        /// </summary>
        public DateTime HourEndTime { get; set; }
    }

    /// <summary>
    /// OEE计算请求参数DTO
    /// </summary>
    public class OeeCalculationRequestDto
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 生产线ID
        /// </summary>
        public Guid ProductionLineId { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public Guid ResourceId { get; set; }

        /// <summary>
        /// 计划生产时间（分钟）
        /// </summary>
        public double PlannedProductionTime { get; set; }

        /// <summary>
        /// 理论节拍时间（秒/件）
        /// </summary>
        public double TheoreticalCycleTime { get; set; }

        /// <summary>
        /// 实际运行时间（分钟）
        /// </summary>
        public double? ActualRunTime { get; set; }

        /// <summary>
        /// 实际产出数量
        /// </summary>
        public int? ActualOutput { get; set; }

        /// <summary>
        /// 合格产出数量
        /// </summary>
        public int? GoodOutput { get; set; }
    }

    /// <summary>
    /// OEE计算结果DTO
    /// </summary>
    public class OeeCalculationResultDto
    {
        /// <summary>
        /// 可用性
        /// </summary>
        public double Availability { get; set; }

        /// <summary>
        /// 性能效率
        /// </summary>
        public double PerformanceEfficiency { get; set; }

        /// <summary>
        /// 质量合格率
        /// </summary>
        public double QualityRate { get; set; }

        /// <summary>
        /// OEE值
        /// </summary>
        public double OeeValue { get; set; }

        /// <summary>
        /// 理论产出
        /// </summary>
        public int TheoreticalOutput { get; set; }

        /// <summary>
        /// 实际产出
        /// </summary>
        public int ActualOutput { get; set; }

        /// <summary>
        /// 合格产出
        /// </summary>
        public int GoodOutput { get; set; }

        /// <summary>
        /// 计划生产时间（分钟）
        /// </summary>
        public double PlannedProductionTime { get; set; }

        /// <summary>
        /// 实际运行时间（分钟）
        /// </summary>
        public double ActualRunTime { get; set; }

        /// <summary>
        /// 停机时间（分钟）
        /// </summary>
        public double Downtime { get; set; }
    }

    /// <summary>
    /// 各站NG件统计DTO
    /// </summary>
    public class StationNGStatisticsDto
    {
        /// <summary>
        /// 站点ID
        /// </summary>
        public Guid StationId { get; set; }

        /// <summary>
        /// 站点编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 站点名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// NG数量
        /// </summary>
        public int NGCount { get; set; }

        /// <summary>
        /// 总测试数量
        /// </summary>
        public int TotalTestCount { get; set; }

        /// <summary>
        /// NG率
        /// </summary>
        public double NGRate { get; set; }

        /// <summary>
        /// 缺陷明细
        /// </summary>
        public List<DefectDetailDto> DefectDetails { get; set; } = new List<DefectDetailDto>();
    }

    /// <summary>
    /// 缺陷明细DTO
    /// </summary>
    public class DefectDetailDto
    {
        /// <summary>
        /// 缺陷类型
        /// </summary>
        public string DefectType { get; set; }

        /// <summary>
        /// 缺陷数量
        /// </summary>
        public int DefectCount { get; set; }
    }

    /// <summary>
    /// 一次通过率DTO
    /// </summary>
    public class FirstPassYieldDto
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 总投入数量
        /// </summary>
        public int TotalInput { get; set; }

        /// <summary>
        /// 一次通过数量
        /// </summary>
        public int FirstPassCount { get; set; }

        /// <summary>
        /// 一次通过率
        /// </summary>
        public double FirstPassYield { get; set; }
    }

    /// <summary>
    /// 合格率/不良率DTO
    /// </summary>
    public class QualityRateDto
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 总测试数量
        /// </summary>
        public int TotalTestCount { get; set; }

        /// <summary>
        /// 合格数量
        /// </summary>
        public int PassCount { get; set; }

        /// <summary>
        /// 不合格数量
        /// </summary>
        public int FailCount { get; set; }

        /// <summary>
        /// 合格率
        /// </summary>
        public double PassRate { get; set; }

        /// <summary>
        /// 不良率
        /// </summary>
        public double FailRate { get; set; }
    }

    /// <summary>
    /// 过程能力指数计算请求参数DTO
    /// </summary>
    public class ProcessCapabilityRequestDto
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 站点ID
        /// </summary>
        public Guid StationId { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParametricKey { get; set; }

        /// <summary>
        /// 规格上限
        /// </summary>
        public double UpperSpecLimit { get; set; }

        /// <summary>
        /// 规格下限
        /// </summary>
        public double LowerSpecLimit { get; set; }

        /// <summary>
        /// 目标值
        /// </summary>
        public double? TargetValue { get; set; }
    }

    /// <summary>
    /// 过程能力指数DTO
    /// </summary>
    public class ProcessCapabilityDto
    {
        /// <summary>
        /// 站点ID
        /// </summary>
        public Guid StationId { get; set; }

        /// <summary>
        /// 站点编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParametricKey { get; set; }

        /// <summary>
        /// 样本数量
        /// </summary>
        public int SampleSize { get; set; }

        /// <summary>
        /// 均值
        /// </summary>
        public double Mean { get; set; }

        /// <summary>
        /// 标准差
        /// </summary>
        public double StandardDeviation { get; set; }

        /// <summary>
        /// 规格上限
        /// </summary>
        public double UpperSpecLimit { get; set; }

        /// <summary>
        /// 规格下限
        /// </summary>
        public double LowerSpecLimit { get; set; }

        /// <summary>
        /// Cp：过程能力指数
        /// </summary>
        public double Cp { get; set; }

        /// <summary>
        /// Cpk：过程能力指数（考虑中心偏移）
        /// </summary>
        public double Cpk { get; set; }

        /// <summary>
        /// Ca：过程准确度
        /// </summary>
        public double Ca { get; set; }

        /// <summary>
        /// Cpu：上单侧过程能力指数
        /// </summary>
        public double Cpu { get; set; }

        /// <summary>
        /// Cpl：下单侧过程能力指数
        /// </summary>
        public double Cpl { get; set; }
    }

    /// <summary>
    /// TOP缺陷分析DTO
    /// </summary>
    public class TopDefectDto
    {
        /// <summary>
        /// 缺陷类型
        /// </summary>
        public string DefectType { get; set; }

        /// <summary>
        /// 缺陷数量
        /// </summary>
        public int DefectCount { get; set; }

        /// <summary>
        /// 缺陷比例
        /// </summary>
        public double DefectPercentage { get; set; }

        /// <summary>
        /// 累计比例
        /// </summary>
        public double CumulativePercentage { get; set; }
    }

    /// <summary>
    /// 完工产品数量统计DTO
    /// </summary>
    public class FinishedProductCountDto
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 总完工数量
        /// </summary>
        public int TotalFinishedCount { get; set; }

        /// <summary>
        /// 合格完工数量
        /// </summary>
        public int PassFinishedCount { get; set; }

        /// <summary>
        /// 不合格完工数量
        /// </summary>
        public int FailFinishedCount { get; set; }
    }
}