using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChargePadLine.WebApi.Controllers.Trace;

/// <summary>
/// 追溯BOM项目控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TraceBomItemController : ControllerBase
{
  private readonly ITraceBomItemService _traceBomItemService;
  private readonly ILogger<TraceBomItemController> _logger;

  public TraceBomItemController(ITraceBomItemService traceBomItemService, ILogger<TraceBomItemController> logger)
  {
    _traceBomItemService = traceBomItemService;
    _logger = logger;
  }

  /// <summary>
  /// 分页查询追溯BOM项目列表
  /// </summary>
  /// <param name="current">当前页码</param>
  /// <param name="pageSize">每页记录数</param>
  /// <param name="id">ID(模糊匹配)</param>
  /// <param name="materialName">物料名称(模糊匹配)</param>
  /// <param name="materialCode">物料代码(模糊匹配)</param>
  /// <returns>分页追溯BOM项目列表</returns>
  [HttpGet("GetTraceBomItemList")]
  public async Task<ActionResult<PaginatedList<TraceBomItem>>> GetTraceBomItemsListAsync(
      [FromQuery] int current = 1,
      [FromQuery] int pageSize = 10,
      [FromQuery] string? id = null,
      [FromQuery] string? materialName = null,
      [FromQuery] string? materialCode = null)
  {
    try
    {
      // 将字符串ID转换为Guid（如果提供的话）
      Guid? guidId = null;
      if (!string.IsNullOrEmpty(id) && Guid.TryParse(id, out Guid parsedId))
      {
        guidId = parsedId;
      }

      var result = await _traceBomItemService.GetListAsync(guidId, materialName, materialCode, current, pageSize);
      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "查询追溯BOM项目时发生错误");
      return StatusCode(500, "查询追溯BOM项目失败");
    }
  }

  /// <summary>
  /// 根据追溯信息ID获取追溯BOM项目列表
  /// </summary>
  /// <param name="traceInfoId">追溯信息ID</param>
  /// <returns>追溯BOM项目实体列表</returns>
  [HttpGet("GetTraceBomItemByTraceInfoId")]
  public async Task<ActionResult<List<TraceBomItem>>> GetTraceBomItemsByTraceInfoIdAsync([FromQuery] Guid traceInfoId)
  {
    try
    {
      var result = await _traceBomItemService.GetByTraceInfoIdAsync(traceInfoId);
      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "根据追溯信息ID查询追溯BOM项目时发生错误, TraceInfoId: {TraceInfoId}", traceInfoId);
      return StatusCode(500, "查询追溯BOM项目失败");
    }
  }

  /// <summary>
  /// 批量删除追溯BOM项目
  /// </summary>
  /// <param name="ids">追溯BOM项目ID列表</param>
  /// <returns>删除结果</returns>
  [HttpPost("DeleteTraceBomItemsById")]
  public async Task<ActionResult<object>> DeleteTraceBomItemsAsync([FromBody] List<Guid> ids)
  {
    try
    {
      if (ids == null || ids.Count == 0)
      {
        return BadRequest("追溯BOM项目ID列表不能为空");
      }

      var result = await _traceBomItemService.DeleteByIdsAsync(ids);
      if (result)
      {
        return Ok(new { deletedCount = ids.Count, message = $"成功删除 {ids.Count} 个追溯BOM项目" });
      }
      else
      {
        return BadRequest("删除失败");
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "删除追溯BOM项目时发生错误, IDs: {@TraceBomItemIds}", ids);
      return StatusCode(500, "删除追溯BOM项目失败");
    }
  }
}