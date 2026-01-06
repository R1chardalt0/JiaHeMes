using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChargePadLine.WebApi.Controllers.Trace;

/// <summary>
/// 追溯工艺项目控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TraceProcItemController : ControllerBase
{
  private readonly ITraceProcItemService _traceProcItemService;
  private readonly ILogger<TraceProcItemController> _logger;

  public TraceProcItemController(ITraceProcItemService traceProcItemService, ILogger<TraceProcItemController> logger)
  {
    _traceProcItemService = traceProcItemService;
    _logger = logger;
  }

  /// <summary>
  /// 分页查询追溯工艺项目列表
  /// </summary>
  /// <param name="current">当前页码</param>
  /// <param name="pageSize">每页记录数</param>
  /// <param name="id">ID(模糊匹配)</param>
  /// <param name="station">工位位置(模糊匹配)</param>
  /// <param name="key">键(模糊匹配)</param>
  /// <returns>分页追溯工艺项目列表</returns>
  [HttpGet("GetTraceProcItemList")]
  public async Task<ActionResult<PaginatedList<TraceProcItem>>> GetTraceProcItemsListAsync(
      [FromQuery] int current = 1,
      [FromQuery] int pageSize = 10,
      [FromQuery] string? id = null,
      [FromQuery] string? station = null,
      [FromQuery] string? key = null)
  {
    try
    {
      // 将字符串ID转换为Guid（如果提供的话）
      Guid? guidId = null;
      if (!string.IsNullOrEmpty(id) && Guid.TryParse(id, out Guid parsedId))
      {
        guidId = parsedId;
      }

      var result = await _traceProcItemService.GetListAsync(guidId, station, key, current, pageSize);
      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "查询追溯工艺项目时发生错误");
      return StatusCode(500, "查询追溯工艺项目失败");
    }
  }

  /// <summary>
  /// 根据追溯信息ID获取追溯工艺项目列表
  /// </summary>
  /// <param name="traceInfoId">追溯信息ID</param>
  /// <returns>追溯工艺项目实体列表</returns>
  [HttpGet("GetTraceProcItemByTraceInfoId")]
  public async Task<ActionResult<List<TraceProcItem>>> GetTraceProcItemsByTraceInfoIdAsync([FromQuery] Guid traceInfoId)
  {
    try
    {
      var result = await _traceProcItemService.GetByTraceInfoIdAsync(traceInfoId);
      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "根据追溯信息ID查询追溯工艺项目时发生错误, TraceInfoId: {TraceInfoId}", traceInfoId);
      return StatusCode(500, "查询追溯工艺项目失败");
    }
  }

  /// <summary>
  /// 批量删除追溯工艺项目
  /// </summary>
  /// <param name="ids">追溯工艺项目ID列表</param>
  /// <returns>删除结果</returns>
  [HttpPost("DeleteTraceProcItemsById")]
  public async Task<ActionResult<object>> DeleteTraceProcItemsAsync([FromBody] List<Guid> ids)
  {
    try
    {
      if (ids == null || ids.Count == 0)
      {
        return BadRequest("追溯工艺项目ID列表不能为空");
      }

      var result = await _traceProcItemService.DeleteByIdsAsync(ids);
      if (result)
      {
        return Ok(new { deletedCount = ids.Count, message = $"成功删除 {ids.Count} 个追溯工艺项目" });
      }
      else
      {
        return BadRequest("删除失败");
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "删除追溯工艺项目时发生错误, IDs: {@TraceProcItemIds}", ids);
      return StatusCode(500, "删除追溯工艺项目失败");
    }
  }
}