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
  /// SN实时状态管理API控制器
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class MesSnListCurrentController : ControllerBase
  {
    private readonly IMesSnListCurrentService _mesSnListCurrentService;
    private readonly ILogger<MesSnListCurrentController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mesSnListCurrentService">SN实时状态服务</param>
    /// <param name="logger">日志记录器</param>
    public MesSnListCurrentController(IMesSnListCurrentService mesSnListCurrentService, ILogger<MesSnListCurrentController> logger)
    {
      _mesSnListCurrentService = mesSnListCurrentService;
      _logger = logger;
    }

    /// <summary>
    /// 分页查询SN实时状态列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页SN实时状态列表</returns>
    [HttpGet("GetMesSnListCurrentList")]
    public async Task<ActionResult<PaginatedList<MesSnListCurrentDto>>> GetMesSnListCurrents([FromQuery] MesSnListCurrentQueryDto queryDto)
    {
      try
      {
        var result = await _mesSnListCurrentService.GetMesSnListCurrentsAsync(queryDto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取SN实时状态列表时发生错误");
        return StatusCode(500, "获取SN实时状态列表失败");
      }
    }

    /// <summary>
    /// 根据ID获取SN实时状态详情
    /// </summary>
    /// <param name="id">SN实时状态ID</param>
    /// <returns>SN实时状态详情</returns>
    [HttpGet("GetMesSnListCurrentById")]
    public async Task<ActionResult<MesSnListCurrentDto>> GetMesSnListCurrentById(Guid id)
    {
      try
      {
        var result = await _mesSnListCurrentService.GetMesSnListCurrentByIdAsync(id);
        if (result == null)
        {
          return NotFound($"未找到ID为 {id} 的SN实时状态");
        }
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取SN实时状态详情时发生错误，ID: {SNListCurrentId}", id);
        return StatusCode(500, "获取SN实时状态详情失败");
      }
    }

    /// <summary>
    /// 根据SN号获取SN实时状态详情
    /// </summary>
    /// <param name="snNumber">SN号</param>
    /// <returns>SN实时状态详情</returns>
    [HttpGet("GetMesSnListCurrentBySnNumber")]
    public async Task<ActionResult<MesSnListCurrentDto>> GetMesSnListCurrentBySnNumber(string snNumber)
    {
      try
      {
        var result = await _mesSnListCurrentService.GetMesSnListCurrentBySnNumberAsync(snNumber);
        if (result == null)
        {
          return NotFound($"未找到SN号为 {snNumber} 的实时状态");
        }
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据SN号获取SN实时状态详情时发生错误，SN号: {SnNumber}", snNumber);
        return StatusCode(500, "根据SN号获取SN实时状态详情失败");
      }
    }
  }
}
