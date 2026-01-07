using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.ProcessRoute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargePadLine.WebApi.Controllers.Trace
{
  /// <summary>
  /// 工艺路线子项管理API控制器
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class ProcessRouteItemController : ControllerBase
  {
    private readonly IProcessRouteItemService _processRouteItemService;
    private readonly ILogger<ProcessRouteItemController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="processRouteItemService">工艺路线子项服务</param>
    /// <param name="logger">日志记录器</param>
    public ProcessRouteItemController(IProcessRouteItemService processRouteItemService, ILogger<ProcessRouteItemController> logger)
    {
      _processRouteItemService = processRouteItemService;
      _logger = logger;
    }

    /// <summary>
    /// 分页查询工艺路线子项列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页工艺路线子项列表</returns>
    [HttpGet("GetProcessRouteItemList")]
    public async Task<ActionResult<PaginatedList<ProcessRouteItemDto>>> GetProcessRouteItems([FromQuery] ProcessRouteItemQueryDto queryDto)
    {
      try
      {
        var result = await _processRouteItemService.GetProcessRouteItemsAsync(queryDto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工艺路线子项列表时发生错误");
        return StatusCode(500, "获取工艺路线子项列表失败");
      }
    }

    /// <summary>
    /// 根据ID获取工艺路线子项详情
    /// </summary>
    /// <param name="id">工艺路线子项ID</param>
    /// <returns>工艺路线子项详情</returns>
    [HttpGet("GetProcessRouteItemById")]
    public async Task<ActionResult<ProcessRouteItemDto>> GetProcessRouteItemById(Guid id)
    {
      try
      {
        var processRouteItem = await _processRouteItemService.GetProcessRouteItemByIdAsync(id);
        if (processRouteItem == null)
        {
          return NotFound($"未找到ID为 {id} 的工艺路线子项");
        }
        return Ok(processRouteItem);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工艺路线子项详情时发生错误，ID: {RouteItemId}", id);
        return StatusCode(500, "获取工艺路线子项详情失败");
      }
    }

    /// <summary>
    /// 根据HeadId获取工艺路线子项列表
    /// </summary>
    /// <param name="headId">工艺路线Head ID</param>
    /// <returns>工艺路线子项列表</returns>
    [HttpGet("GetProcessRouteItemsByHeadId")]
    public async Task<ActionResult<IEnumerable<ProcessRouteItemDto>>> GetProcessRouteItemsByHeadId(Guid headId)
    {
      try
      {
        var processRouteItems = await _processRouteItemService.GetProcessRouteItemsByHeadIdAsync(headId);
        return Ok(processRouteItems);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据HeadId获取工艺路线子项列表时发生错误，HeadId: {HeadId}", headId);
        return StatusCode(500, "获取工艺路线子项列表失败");
      }
    }

    /// <summary>
    /// 创建工艺路线子项
    /// </summary>
    /// <param name="dto">创建工艺路线子项参数</param>
    /// <returns>创建的工艺路线子项信息</returns>
    [HttpPost("CreateProcessRouteItem")]
    public async Task<ActionResult<ProcessRouteItemDto>> CreateProcessRouteItem([FromBody] ProcessRouteItemCreateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        var result = await _processRouteItemService.CreateProcessRouteItemAsync(dto);
        return CreatedAtAction(nameof(GetProcessRouteItemById), new { id = result.Id }, result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建工艺路线子项时发生错误");
        return StatusCode(500, "创建工艺路线子项失败");
      }
    }

    /// <summary>
    /// 更新工艺路线子项
    /// </summary>
    /// <param name="id">工艺路线子项ID</param>
    /// <param name="dto">更新工艺路线子项参数</param>
    /// <returns>更新后的工艺路线子项信息</returns>
    [HttpPost("UpdateProcessRouteItemById")]
    public async Task<ActionResult<ProcessRouteItemDto>> UpdateProcessRouteItem(Guid id, [FromBody] ProcessRouteItemUpdateDto dto)
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

        var result = await _processRouteItemService.UpdateProcessRouteItemAsync(dto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新工艺路线子项时发生错误，ID: {RouteItemId}", id);
        return StatusCode(500, "更新工艺路线子项失败");
      }
    }

    /// <summary>
    /// 删除工艺路线子项（支持单个删除和批量删除）
    /// </summary>
    /// <param name="ids">工艺路线子项ID列表</param>
    /// <returns>删除结果</returns>
    [HttpPost("DeleteProcessRouteItemByIds")]
    public async Task<ActionResult<object>> DeleteProcessRouteItemByIds([FromBody] List<Guid> ids)
    {
      try
      {
        if (ids == null || ids.Count == 0)
        {
          return BadRequest("工艺路线子项ID列表不能为空");
        }
        var result = await _processRouteItemService.DeleteProcessRouteItemsAsync(ids.ToArray());
        return Ok(new { deletedCount = result, message = $"成功删除 {result} 个工艺路线子项" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除工艺路线子项时发生错误,IDs:{@RouteItemIds}", ids);
        return StatusCode(500, "删除工艺路线子项失败");
      }
    }
  }
}