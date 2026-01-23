using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.MesSnList;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargePadLine.WebApi.Controllers.Trace
{
  /// <summary>
  /// SN历史记录管理API控制器
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class MesSnListHistoryController : ControllerBase
  {
    private readonly IMesSnListHistoryService _mesSnListHistoryService;
    private readonly ILogger<MesSnListHistoryController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mesSnListHistoryService">SN历史记录服务</param>
    /// <param name="logger">日志记录器</param>
    public MesSnListHistoryController(IMesSnListHistoryService mesSnListHistoryService, ILogger<MesSnListHistoryController> logger)
    {
      _mesSnListHistoryService = mesSnListHistoryService;
      _logger = logger;
    }

    /// <summary>
    /// 分页查询SN历史记录列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页SN历史记录列表</returns>
    [HttpGet("GetMesSnListHistoryList")]
    public async Task<ActionResult<PaginatedList<MesSnListHistoryDto>>> GetMesSnListHistories([FromQuery] MesSnListHistoryQueryDto queryDto)
    {
      try
      {
        var result = await _mesSnListHistoryService.GetMesSnListHistoriesAsync(queryDto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取SN历史记录列表时发生错误");
        return StatusCode(500, "获取SN历史记录列表失败");
      }
    }

    /// <summary>
    /// 根据ID获取SN历史记录详情
    /// </summary>
    /// <param name="id">SN历史记录ID</param>
    /// <returns>SN历史记录详情</returns>
    [HttpGet("GetMesSnListHistoryById")]
    public async Task<ActionResult<MesSnListHistoryDto>> GetMesSnListHistoryById(Guid id)
    {
      try
      {
        var result = await _mesSnListHistoryService.GetMesSnListHistoryByIdAsync(id);
        if (result == null)
        {
          return NotFound($"未找到ID为 {id} 的SN历史记录");
        }
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取SN历史记录详情时发生错误，ID: {SNListHistoryId}", id);
        return StatusCode(500, "获取SN历史记录详情失败");
      }
    }

    /// <summary>
    /// 根据SN号获取SN历史记录详情
    /// </summary>
    /// <param name="snNumber">SN号</param>
    /// <returns>SN历史记录详情</returns>
    [HttpGet("GetMesSnListHistoryBySnNumber")]
    public async Task<ActionResult<MesSnListHistoryDto>> GetMesSnListHistoryBySnNumber(string snNumber)
    {
      try
      {
        var result = await _mesSnListHistoryService.GetMesSnListHistoryBySnNumberAsync(snNumber);
        if (result == null)
        {
          return NotFound($"未找到SN号为 {snNumber} 的历史记录");
        }
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据SN号获取SN历史记录详情时发生错误，SN号: {SnNumber}", snNumber);
        return StatusCode(500, "根据SN号获取SN历史记录详情失败");
      }
    }
  }
}
