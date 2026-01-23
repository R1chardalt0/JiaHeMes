using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.ProcessRoute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChargePadLine.WebApi.util;
using ChargePadLine.WebApi.Controllers.util;

namespace ChargePadLine.WebApi.Controllers.Trace
{
  /// <summary>
  /// 工艺路线管理API控制器
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class ProcessRouteController : ControllerBase
  {
    private readonly IProcessRouteService _processRouteService;
    private readonly ILogger<ProcessRouteController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="processRouteService">工艺路线服务</param>
    /// <param name="logger">日志记录器</param>
    public ProcessRouteController(IProcessRouteService processRouteService, ILogger<ProcessRouteController> logger)
    {
      _processRouteService = processRouteService;
      _logger = logger;
    }

    /// <summary>
    /// 分页查询工艺路线列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页工艺路线列表</returns>
    [HttpGet("GetProcessRouteList")]
    public async Task<ActionResult<PagedResp<ProcessRouteDto>>> GetProcessRoutes([FromQuery] ProcessRouteQueryDto queryDto)
    {
      try
      {
        var result = await _processRouteService.GetProcessRoutesAsync(queryDto);
        return Ok(RespExtensions.MakePagedSuccess(result));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工艺路线列表时发生错误");
        return StatusCode(500, RespExtensions.MakeFail("500", "获取工艺路线列表失败"));
      }
    }

    /// <summary>
    /// 根据ID获取工艺路线详情
    /// </summary>
    /// <param name="id">工艺路线ID</param>
    /// <returns>工艺路线详情</returns>
    [HttpGet("GetProcessRouteById")]
    public async Task<ActionResult<ProcessRouteDto>> GetProcessRouteById(Guid id)
    {
      try
      {
        var processRoute = await _processRouteService.GetProcessRouteByIdAsync(id);
        if (processRoute == null)
        {
          return NotFound($"未找到ID为 {id} 的工艺路线");
        }
        return Ok(processRoute);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工艺路线详情时发生错误，ID: {RouteId}", id);
        return StatusCode(500, "获取工艺路线详情失败");
      }
    }

    /// <summary>
    /// 创建工艺路线
    /// </summary>
    /// <param name="dto">创建工艺路线参数</param>
    /// <returns>创建的工艺路线信息</returns>
    [HttpPost("CreateProcessRoute")]
    public async Task<ActionResult<ProcessRouteDto>> CreateProcessRoute([FromBody] ProcessRouteCreateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        var result = await _processRouteService.CreateProcessRouteAsync(dto);
        return CreatedAtAction(nameof(GetProcessRouteById), new { id = result.Id }, result);
      }
      catch (InvalidOperationException ex)
      {
        _logger.LogError(ex, "创建工艺路线时发生错误");
        return BadRequest(ex.Message);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建工艺路线时发生错误");
        return StatusCode(500, "创建工艺路线失败");
      }
    }

    /// <summary>
    /// 更新工艺路线
    /// </summary>
    /// <param name="id">工艺路线ID</param>
    /// <param name="dto">更新工艺路线参数</param>
    /// <returns>更新后的工艺路线信息</returns>
    [HttpPost("UpdateProcessRouteById")]
    public async Task<ActionResult<ProcessRouteDto>> UpdateProcessRoute(Guid id, [FromBody] ProcessRouteUpdateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        if (dto.Id != id)
        {
          return BadRequest("URL中的ID与请求体中的ID不匹配");
        }

        var result = await _processRouteService.UpdateProcessRouteAsync(dto);
        return Ok(result);
      }
      catch (InvalidOperationException ex)
      {
        _logger.LogError(ex, "更新工艺路线时发生错误，ID: {RouteId}", id);
        return BadRequest(ex.Message);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新工艺路线时发生错误，ID: {RouteId}", id);
        return StatusCode(500, "更新工艺路线失败");
      }
    }

    /// <summary>
    /// 删除工艺路线（支持单个删除和批量删除）
    /// </summary>
    /// <param name="ids">工艺路线ID列表</param>
    /// <returns>删除结果</returns>
    [HttpPost("DeleteProcessRouteByIds")]
    public async Task<ActionResult<object>> DeleteProcessRouteByIds([FromBody] List<Guid> ids)
    {
      try
      {
        if (ids == null || ids.Count == 0)
        {
          return BadRequest("工艺路线ID列表不能为空");
        }
        var result = await _processRouteService.DeleteProcessRoutesAsync(ids.ToArray());
        return Ok(new { deletedCount = result, message = $"成功删除 {result} 个工艺路线" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除工艺路线时发生错误,IDs:{@RouteIds}", ids);
        return StatusCode(500, "删除工艺路线失败");
      }
    }
  }
}