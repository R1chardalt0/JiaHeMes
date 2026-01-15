using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.Station;
using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Service;
using Microsoft.Extensions.Logging;

namespace ChargePadLine.WebApi.Controllers.Trace
{
  /// <summary>
  /// 站点管理API控制器
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class StationListController : ControllerBase
  {
    private readonly IStationListService _stationListService;
    private readonly ILogger<StationListController> _logger;

    public StationListController(IStationListService stationListService, ILogger<StationListController> logger)
    {
      _stationListService = stationListService;
      _logger = logger;
    }

    /// <summary>
    /// 分页查询站点列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页站点列表</returns>
    [HttpGet("GetStationListList")]
    public async Task<ActionResult<PaginatedList<StationListDto>>> GetStationLists([FromQuery] StationListQueryDto queryDto)
    {
      try
      {
        var result = await _stationListService.PaginationAsync(queryDto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取站点列表时发生错误");
        return StatusCode(500, "获取站点列表失败");
      }
    }

    /// <summary>
    /// 根据ID查询站点详情
    /// </summary>

    [HttpGet("GetStationListById")]
    public async Task<ActionResult<StationListDto>> GetStationInfoById(Guid id)
    {
      try
      {
        var workOrder = await _stationListService.GetStationInfoById(id);
        if (workOrder == null)
        {
          return NotFound($"未找到ID为 {id} 的站点");
        }
        return Ok(workOrder);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取站点详情时发生错误，ID: {StationListId}", id);
        return StatusCode(500, "获取站点详情失败");
      }
    }

    /// <summary>
    /// 创建站点信息
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("CreateStationList")]
    public async Task<ActionResult<StationListDto>> CreateStationList([FromBody] StationListDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        var result = await _stationListService.CreateStationInfo(dto);
        return CreatedAtAction(nameof(GetStationInfoById), new { id = result.StationId }, result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建站点时发生错误");
        return StatusCode(500, "创建站点失败");
      }
    }

    /// <summary>
    /// 更新站点
    /// </summary>

    [HttpPost("UpdateStationListById")]
    public async Task<ActionResult<StationListDto>> UpdateStationList(Guid id, [FromBody] StationListDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        if (dto.StationId != id)
        {
          return BadRequest("URL中的ID与请求体中的ID不匹配");
        }

        var result = await _stationListService.UpdateStationInfo(dto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新站点时发生错误，ID: {StationListId}", id);
        return StatusCode(500, "更新站点失败");
      }
    }

    /// <summary>
    /// 删除站点（支持单个删除和批量删除）
    /// </summary>

    [HttpPost("DeleteStationListByIds")]
    public async Task<ActionResult<object>> DeleteStationListById([FromBody] List<Guid> ids)
    {
      try
      {
        if (ids == null || ids.Count == 0)
        {
          return BadRequest("站点ID列表不能为空");
        }
        var result = await _stationListService.DeleteStationInfoById(ids.ToArray());
        return Ok(new { deletedCount = result, message = $"成功删除 {result} 个站点" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除站点时发生错误,IDs:{@StationListIds}", ids);
        return StatusCode(500, "删除站点失败");
      }
    }
  }
}