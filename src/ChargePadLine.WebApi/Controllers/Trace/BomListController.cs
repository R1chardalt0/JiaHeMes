using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.BOM;
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
  /// BOM列表管理API控制器
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class BomListController : ControllerBase
  {
    private readonly IBomListService _bomListService;
    private readonly ILogger<BomListController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="bomListService">BOM列表服务</param>
    /// <param name="logger">日志记录器</param>
    public BomListController(IBomListService bomListService, ILogger<BomListController> logger)
    {
      _bomListService = bomListService;
      _logger = logger;
    }

    /// <summary>
    /// 分页查询BOM列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页BOM列表</returns>
    [HttpGet("GetBomListList")]
    public async Task<ActionResult<PagedResp<BomListDto>>> GetBomLists([FromQuery] BomListQueryDto queryDto)
    {
      try
      {
        var result = await _bomListService.GetBomListsAsync(queryDto);
        return Ok(RespExtensions.MakePagedSuccess(result));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取BOM列表时发生错误");
        return StatusCode(500, RespExtensions.MakeFail("500", "获取BOM列表失败"));
      }
    }

    /// <summary>
    /// 根据ID获取BOM详情
    /// </summary>
    /// <param name="id">BOM ID</param>
    /// <returns>BOM详情</returns>
    [HttpGet("GetBomListById")]
    public async Task<ActionResult<BomListDto>> GetBomListById(Guid id)
    {
      try
      {
        var bomList = await _bomListService.GetBomListByIdAsync(id);
        if (bomList == null)
        {
          return NotFound($"未找到ID为 {id} 的BOM");
        }
        return Ok(bomList);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取BOM详情时发生错误，ID: {BomId}", id);
        return StatusCode(500, "获取BOM详情失败");
      }
    }

    /// <summary>
    /// 创建BOM
    /// </summary>
    /// <param name="dto">创建BOM参数</param>
    /// <returns>创建的BOM信息</returns>
    [HttpPost("CreateBomList")]
    public async Task<ActionResult<BomListDto>> CreateBomList([FromBody] BomListCreateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        var result = await _bomListService.CreateBomListAsync(dto);
        return CreatedAtAction(nameof(GetBomListById), new { id = result.BomId }, result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建BOM时发生错误");
        return StatusCode(500, "创建BOM失败");
      }
    }

    /// <summary>
    /// 更新BOM
    /// </summary>
    /// <param name="id">BOM ID</param>
    /// <param name="dto">更新BOM参数</param>
    /// <returns>更新后的BOM信息</returns>
    [HttpPost("UpdateBomListById")]
    public async Task<ActionResult<BomListDto>> UpdateBomList(Guid id, [FromBody] BomListUpdateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        if (dto.BomId != id)
        {
          return BadRequest("URL中的ID与请求体中的ID不匹配");
        }

        var result = await _bomListService.UpdateBomListAsync(dto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新BOM时发生错误，ID: {BomId}", id);
        return StatusCode(500, "更新BOM失败");
      }
    }

    /// <summary>
    /// 删除BOM（支持单个删除和批量删除）
    /// </summary>
    /// <param name="ids">BOM ID列表</param>
    /// <returns>删除结果</returns>
    [HttpPost("DeleteBomListByIds")]
    public async Task<ActionResult<object>> DeleteBomListByIds([FromBody] List<Guid> ids)
    {
      try
      {
        if (ids == null || ids.Count == 0)
        {
          return BadRequest("BOM ID列表不能为空");
        }
        var result = await _bomListService.DeleteBomListsAsync(ids.ToArray());
        return Ok(new { deletedCount = result, message = $"成功删除 {result} 个BOM" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除BOM时发生错误,IDs:{@BomIds}", ids);
        return StatusCode(500, new { message = $"{ex.GetType().FullName}: {ex.Message}" });
      }
    }
  }
}