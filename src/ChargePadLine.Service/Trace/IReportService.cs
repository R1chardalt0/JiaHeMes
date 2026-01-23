using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChargePadLine.Service.Trace.Dto;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// 生产报告服务接口
  /// 提供生产数据统计、质量分析等报告生成功能
  /// </summary>
  public interface IReportService
  {
    /// <summary>
    /// 获取每小时产出统计
    /// </summary>
    /// <param name="productionLineId">生产线ID（可选）</param>
    /// <param name="workOrderId">工单ID（可选）</param>
    /// <param name="resourceId">设备ID（可选）</param>
    /// <param name="startTime">开始时间（可选）</param>
    /// <param name="endTime">结束时间（可选）</param>
    /// <returns>每小时产出统计数据</returns>
    Task<List<HourlyOutputDto>> GetHourlyOutputAsync(Guid? productionLineId = null, Guid? workOrderId = null, Guid? resourceId = null, DateTime? startTime = null, DateTime? endTime = null);

    /// <summary>
    /// 计算OEE（设备综合效率）
    /// </summary>
    /// <param name="request">OEE计算请求参数</param>
    /// <returns>OEE计算结果</returns>
    Task<OeeCalculationResultDto> CalculateOEEAsync(OeeCalculationRequestDto request);

    /// <summary>
    /// 获取各站NG件统计
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="productionLineId">生产线ID（可选）</param>
    /// <returns>各站NG件统计数据</returns>
    Task<List<StationNGStatisticsDto>> GetStationNGStatisticsAsync(DateTime startTime, DateTime endTime, Guid? productionLineId = null);

    /// <summary>
    /// 计算一次通过率
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="workOrderId">工单ID（可选）</param>
    /// <returns>一次通过率结果</returns>
    Task<FirstPassYieldDto> CalculateFirstPassYieldAsync(DateTime startTime, DateTime endTime, Guid? workOrderId = null);

    /// <summary>
    /// 计算合格率/不良率
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="productionLineId">生产线ID（可选）</param>
    /// <returns>合格率/不良率结果</returns>
    Task<QualityRateDto> CalculateQualityRateAsync(DateTime startTime, DateTime endTime, Guid? productionLineId = null);

    /// <summary>
    /// 计算过程能力指数
    /// </summary>
    /// <param name="request">过程能力指数计算请求参数</param>
    /// <returns>过程能力指数计算结果</returns>
    Task<ProcessCapabilityDto> CalculateProcessCapabilityAsync(ProcessCapabilityRequestDto request);

    /// <summary>
    /// 获取TOP缺陷分析
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="topN">返回前N个缺陷（默认10）</param>
    /// <param name="productionLineId">生产线ID（可选）</param>
    /// <returns>TOP缺陷分析结果</returns>
    Task<List<TopDefectDto>> GetTopDefectsAsync(DateTime startTime, DateTime endTime, int topN = 10, Guid? productionLineId = null);

    /// <summary>
    /// 获取指定时间范围内的完工产品数量统计
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="productionLineId">生产线ID（可选）</param>
    /// <returns>完工产品数量统计结果列表（按产品分组）</returns>
    Task<List<FinishedProductCountDto>> GetFinishedProductCountAsync(DateTime startTime, DateTime endTime, Guid? productionLineId = null);

  }
}