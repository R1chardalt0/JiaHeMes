using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChargePadLine.WebApi.Controllers.Trace;

/// <summary>
/// 追溯信息控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TraceInfoController : ControllerBase
{
  private readonly ITraceInfoService _traceInfoService;
  private readonly ILogger<TraceInfoController> _logger;

  public TraceInfoController(ITraceInfoService traceInfoService, ILogger<TraceInfoController> logger)
  {
    _traceInfoService = traceInfoService;
    _logger = logger;
  }

  /// <summary>
  /// 分页查询追溯信息列表
  /// </summary>
  /// <param name="current">当前页码</param>
  /// <param name="pageSize">每页记录数</param>
  /// <param name="id">ID(模糊匹配)</param>
  /// <param name="pin">产品识别码(模糊匹配)</param>
  /// <param name="vsn">VSN(模糊匹配)</param>
  /// <param name="productCode">产品编码(模糊匹配)</param>
  /// <returns>分页追溯信息列表</returns>
  [HttpGet("GetTraceInfoList")]
  public async Task<ActionResult<PaginatedList<TraceInfo>>> GetTraceInfosListAsync(
      [FromQuery] int current = 1,
      [FromQuery] int pageSize = 10,
      [FromQuery] string? id = null,
      [FromQuery] string? pin = null,
      [FromQuery] string? vsn = null,
      [FromQuery] string? productCode = null)
  {
    try
    {
      // 将字符串ID转换为Guid（如果提供的话）
      Guid? guidId = null;
      if (!string.IsNullOrEmpty(id) && Guid.TryParse(id, out Guid parsedId))
      {
        guidId = parsedId;
      }

      var result = await _traceInfoService.GetListAsync(guidId, pin, vsn, productCode, current, pageSize);
      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "查询追溯信息时发生错误");
      return StatusCode(500, "查询追溯信息失败");
    }
  }

  /// <summary>
  /// 根据ID获取追溯信息
  /// </summary>
  /// <param name="id">追溯信息ID</param>
  /// <returns>追溯信息实体</returns>
  [HttpGet("GetTraceInfoById")]
  public async Task<ActionResult<TraceInfo>> GetTraceInfoByIdAsync([FromQuery] Guid id)
  {
    try
    {
      var result = await _traceInfoService.GetByIdAsync(id);
      if (result == null)
      {
        return NotFound($"未找到ID为 {id} 的追溯信息");
      }
      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "根据ID查询追溯信息时发生错误, ID: {Id}", id);
      return StatusCode(500, "查询追溯信息失败");
    }
  }

  /// <summary>
  /// 批量删除追溯信息
  /// </summary>
  /// <param name="ids">追溯信息ID列表</param>
  /// <returns>删除结果</returns>
  [HttpPost("DeleteTraceInfosById")]
  public async Task<ActionResult<object>> DeleteTraceInfosAsync([FromBody] List<Guid> ids)
  {
    try
    {
      if (ids == null || ids.Count == 0)
      {
        return BadRequest("追溯信息ID列表不能为空");
      }

      var result = await _traceInfoService.DeleteByIdsAsync(ids);
      if (result)
      {
        return Ok(new { deletedCount = ids.Count, message = $"成功删除 {ids.Count} 个追溯信息" });
      }
      else
      {
        return BadRequest("删除失败");
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "删除追溯信息时发生错误, IDs: {@TraceInfoIds}", ids);
      return StatusCode(500, "删除追溯信息失败");
    }
  }
}