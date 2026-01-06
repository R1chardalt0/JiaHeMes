using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.BOM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargePadLine.WebApi.Controllers.Trace
{
  /// <summary>
  /// BOM子项管理API控制器
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class BomItemController : ControllerBase
  {
    private readonly IBomItemService _bomItemService;
    private readonly ILogger<BomItemController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="bomItemService">BOM子项服务</param>
    /// <param name="logger">日志记录器</param>
    public BomItemController(IBomItemService bomItemService, ILogger<BomItemController> logger)
    {
      _bomItemService = bomItemService;
      _logger = logger;
    }

    /// <summary>
    /// 分页查询BOM子项列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页BOM子项列表</returns>
    [HttpGet("GetBomItemList")]
    public async Task<ActionResult<PaginatedList<BomItemDto>>> GetBomItems([FromQuery] BomItemQueryDto queryDto)
    {
      try
      {
        var result = await _bomItemService.GetBomItemsAsync(queryDto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取BOM子项列表时发生错误");
        return StatusCode(500, "获取BOM子项列表失败");
      }
    }

    /// <summary>
    /// 根据ID获取BOM子项详情
    /// </summary>
    /// <param name="id">BOM子项ID</param>
    /// <returns>BOM子项详情</returns>
    [HttpGet("GetBomItemById")]
    public async Task<ActionResult<BomItemDto>> GetBomItemById(Guid id)
    {
      try
      {
        var bomItem = await _bomItemService.GetBomItemByIdAsync(id);
        if (bomItem == null)
        {
          return NotFound($"未找到ID为 {id} 的BOM子项");
        }
        return Ok(bomItem);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取BOM子项详情时发生错误，ID: {BomItemId}", id);
        return StatusCode(500, "获取BOM子项详情失败");
      }
    }

    /// <summary>
    /// 根据BomId获取BOM子项列表
    /// </summary>
    /// <param name="bomId">BOM ID</param>
    /// <returns>BOM子项列表</returns>
    [HttpGet("GetBomItemsByBomId")]
    public async Task<ActionResult<IEnumerable<BomItemDto>>> GetBomItemsByBomId(Guid bomId)
    {
      try
      {
        var bomItems = await _bomItemService.GetBomItemsByBomIdAsync(bomId);
        return Ok(bomItems);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据BomId获取BOM子项列表时发生错误，BomId: {BomId}", bomId);
        return StatusCode(500, "获取BOM子项列表失败");
      }
    }

    /// <summary>
    /// 创建BOM子项
    /// </summary>
    /// <param name="dto">创建BOM子项参数</param>
    /// <returns>创建的BOM子项信息</returns>
    [HttpPost("CreateBomItem")]
    public async Task<ActionResult<BomItemDto>> CreateBomItem([FromBody] BomItemCreateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        var result = await _bomItemService.CreateBomItemAsync(dto);
        return CreatedAtAction(nameof(GetBomItemById), new { id = result.BomItemId }, result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建BOM子项时发生错误");
        return StatusCode(500, "创建BOM子项失败");
      }
    }

    /// <summary>
    /// 更新BOM子项
    /// </summary>
    /// <param name="id">BOM子项ID</param>
    /// <param name="dto">更新BOM子项参数</param>
    /// <returns>更新后的BOM子项信息</returns>
    [HttpPost("UpdateBomItemById")]
    public async Task<ActionResult<BomItemDto>> UpdateBomItem(Guid id, [FromBody] BomItemUpdateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        if (dto.BomItemId != id)
        {
          return BadRequest("URL中的ID与请求体中的ID不匹配");
        }

        var result = await _bomItemService.UpdateBomItemAsync(dto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新BOM子项时发生错误，ID: {BomItemId}", id);
        return StatusCode(500, "更新BOM子项失败");
      }
    }

    /// <summary>
    /// 删除BOM子项（支持单个删除和批量删除）
    /// </summary>
    /// <param name="ids">BOM子项ID列表</param>
    /// <returns>删除结果</returns>
    [HttpPost("DeleteBomItemByIds")]
    public async Task<ActionResult<object>> DeleteBomItemByIds([FromBody] List<Guid> ids)
    {
      try
      {
        if (ids == null || ids.Count == 0)
        {
          return BadRequest("BOM子项ID列表不能为空");
        }
        var result = await _bomItemService.DeleteBomItemsAsync(ids.ToArray());
        return Ok(new { deletedCount = result, message = $"成功删除 {result} 个BOM子项" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除BOM子项时发生错误,IDs:{@BomItemIds}", ids);
        return StatusCode(500, "删除BOM子项失败");
      }
    }
  }
}