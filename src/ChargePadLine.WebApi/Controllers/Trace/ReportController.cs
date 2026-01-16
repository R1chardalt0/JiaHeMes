using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto;
using ChargePadLine.WebApi.Controllers.Systems;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargePadLine.WebApi.Controllers.Trace
{
  /// <summary>
  /// 生产报告控制器
  /// 提供生产数据统计、质量分析等报告生成的API接口
  /// 用于支持生产管理系统的数据分析和决策功能
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class ReportController : BaseController
  {
    private readonly IReportService _reportService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="reportService">生产报告服务实例，用于处理报告数据的业务逻辑</param>
    public ReportController(IReportService reportService)
    {
      _reportService = reportService;
    }

    /// <summary>
    /// 获取每小时产出统计
    /// 按小时统计指定时间范围内的生产数据，包括产出数量、合格数量和不合格数量
    /// </summary>
    /// <param name="productionLineId">生产线ID（可选），用于过滤特定生产线的数据</param>
    /// <param name="workOrderId">工单ID（可选），用于过滤特定工单的数据</param>
    /// <param name="resourceId">设备ID（可选），用于过滤特定设备的数据</param>
    /// <param name="startTime">开始时间（可选），用于指定统计的开始时间</param>
    /// <param name="endTime">结束时间（可选），用于指定统计的结束时间</param>
    /// <returns>返回每小时产出统计数据的列表，包含小时、产出数量、合格数量、不合格数量等信息</returns>
    /// <response code="200">获取成功，返回统计数据</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("HourlyOutput")]
    public async Task<IActionResult> GetHourlyOutput(Guid? productionLineId = null, Guid? workOrderId = null, Guid? resourceId = null, DateTime? startTime = null, DateTime? endTime = null)
    {
      try
      {
        var result = await _reportService.GetHourlyOutputAsync(productionLineId, workOrderId, resourceId, startTime, endTime);
        return Ok(new { code = 200, data = result, message = "获取成功" });
      }
      catch (Exception ex)
      {
        // 记录异常日志
        return StatusCode(500, new { code = 500, message = "服务器内部错误: " + ex.Message });
      }
    }

    /// <summary>
    /// 计算OEE（设备综合效率）
    /// 根据用户提供的参数计算设备的综合效率，包括可用性、性能效率和质量率
    /// </summary>
    /// <param name="request">OEE计算请求参数，包含计划生产时间、理论节拍时间、实际运行时间等参数</param>
    /// <returns>返回OEE计算结果，包含可用性、性能效率、质量率和最终的OEE值</returns>
    /// <response code="200">计算成功，返回OEE计算结果</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("CalculateOEE")]
    public async Task<IActionResult> CalculateOEE([FromBody] OeeCalculationRequestDto request)
    {
      try
      {
        var result = await _reportService.CalculateOEEAsync(request);
        return Ok(new { code = 200, data = result, message = "计算成功" });
      }
      catch (Exception ex)
      {
        // 记录异常日志
        return StatusCode(500, new { code = 500, message = "服务器内部错误: " + ex.Message });
      }
    }

    /// <summary>
    /// 获取各站NG件统计
    /// 统计指定时间范围内各站点的不合格数量和NG率
    /// </summary>
    /// <param name="startTime">统计开始时间，格式为yyyy-MM-dd HH:mm:ss</param>
    /// <param name="endTime">统计结束时间，格式为yyyy-MM-dd HH:mm:ss</param>
    /// <param name="productionLineId">生产线ID（可选），用于过滤特定生产线的数据</param>
    /// <returns>返回各站点NG统计数据的列表，包含站点名称、NG数量、总数量、NG率等信息</returns>
    /// <response code="200">获取成功，返回统计数据</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("StationNGStatistics")]
    public async Task<IActionResult> GetStationNGStatistics(DateTime startTime, DateTime endTime, Guid? productionLineId = null)
    {
      try
      {
        var result = await _reportService.GetStationNGStatisticsAsync(startTime, endTime, productionLineId);
        return Ok(new { code = 200, data = result, message = "获取成功" });
      }
      catch (Exception ex)
      {
        // 记录异常日志
        return StatusCode(500, new { code = 500, message = "服务器内部错误: " + ex.Message });
      }
    }

    /// <summary>
    /// 计算一次通过率
    /// 计算产品从开始到完成不经过任何返工的比例
    /// </summary>
    /// <param name="startTime">统计开始时间，格式为yyyy-MM-dd HH:mm:ss</param>
    /// <param name="endTime">统计结束时间，格式为yyyy-MM-dd HH:mm:ss</param>
    /// <param name="workOrderId">工单ID（可选），用于过滤特定工单的数据</param>
    /// <returns>返回一次通过率计算结果，包含总数量、一次通过数量和一次通过率</returns>
    /// <response code="200">计算成功，返回一次通过率结果</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("FirstPassYield")]
    public async Task<IActionResult> CalculateFirstPassYield(DateTime startTime, DateTime endTime, Guid? workOrderId = null)
    {
      try
      {
        var result = await _reportService.CalculateFirstPassYieldAsync(startTime, endTime, workOrderId);
        return Ok(new { code = 200, data = result, message = "计算成功" });
      }
      catch (Exception ex)
      {
        // 记录异常日志
        return StatusCode(500, new { code = 500, message = "服务器内部错误: " + ex.Message });
      }
    }

    /// <summary>
    /// 计算合格率/不良率
    /// 统计指定时间范围内的产品合格和不合格比例
    /// </summary>
    /// <param name="startTime">统计开始时间，格式为yyyy-MM-dd HH:mm:ss</param>
    /// <param name="endTime">统计结束时间，格式为yyyy-MM-dd HH:mm:ss</param>
    /// <param name="productionLineId">生产线ID（可选），用于过滤特定生产线的数据</param>
    /// <returns>返回合格率/不良率计算结果，包含总数量、合格数量、不合格数量、合格率和不良率</returns>
    /// <response code="200">计算成功，返回合格率/不良率结果</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("QualityRate")]
    public async Task<IActionResult> CalculateQualityRate(DateTime startTime, DateTime endTime, Guid? productionLineId = null)
    {
      try
      {
        var result = await _reportService.CalculateQualityRateAsync(startTime, endTime, productionLineId);
        return Ok(new { code = 200, data = result, message = "计算成功" });
      }
      catch (Exception ex)
      {
        // 记录异常日志
        return StatusCode(500, new { code = 500, message = "服务器内部错误: " + ex.Message });
      }
    }

    /// <summary>
    /// 计算过程能力指数
    /// 计算CP、Cpk、Ca等过程能力指标，用于评估生产过程的稳定性和能力
    /// </summary>
    /// <param name="request">过程能力指数计算请求参数，包含规格上限、规格下限、目标值、测量数据等</param>
    /// <returns>返回过程能力指数计算结果，包含CP、Cpk、Ca、标准差、平均值等指标</returns>
    /// <response code="200">计算成功，返回过程能力指数结果</response>
    /// <response code="400">参数错误，返回错误信息</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("ProcessCapability")]
    public async Task<IActionResult> CalculateProcessCapability([FromBody] ProcessCapabilityRequestDto request)
    {
      try
      {
        var result = await _reportService.CalculateProcessCapabilityAsync(request);
        return Ok(new { code = 200, data = result, message = "计算成功" });
      }
      catch (ArgumentException ex)
      {
        // 参数验证错误，返回400状态码
        return BadRequest(new { code = 400, message = ex.Message });
      }
      catch (Exception ex)
      {
        // 记录异常日志
        return StatusCode(500, new { code = 500, message = "服务器内部错误: " + ex.Message });
      }
    }

    /// <summary>
    /// 获取TOP缺陷分析
    /// 统计并排序出现最多的缺陷类型，用于质量改进分析
    /// </summary>
    /// <param name="startTime">统计开始时间，格式为yyyy-MM-dd HH:mm:ss</param>
    /// <param name="endTime">统计结束时间，格式为yyyy-MM-dd HH:mm:ss</param>
    /// <param name="topN">返回前N个缺陷类型（默认10），用于控制结果数量</param>
    /// <param name="productionLineId">生产线ID（可选），用于过滤特定生产线的数据</param>
    /// <returns>返回TOP缺陷分析结果，包含缺陷类型、出现次数和占比等信息</returns>
    /// <response code="200">获取成功，返回TOP缺陷分析数据</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("TopDefects")]
    public async Task<IActionResult> GetTopDefects(DateTime startTime, DateTime endTime, int topN = 10, Guid? productionLineId = null)
    {
      try
      {
        var result = await _reportService.GetTopDefectsAsync(startTime, endTime, topN, productionLineId);
        return Ok(new { code = 200, data = result, message = "获取成功" });
      }
      catch (Exception ex)
      {
        // 记录异常日志
        return StatusCode(500, new { code = 500, message = "服务器内部错误: " + ex.Message });
      }
    }
  }
}