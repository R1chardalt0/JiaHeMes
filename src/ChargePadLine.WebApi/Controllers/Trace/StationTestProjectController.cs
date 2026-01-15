using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.Station;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargePadLine.WebApi.Controllers.Trace
{
  /// <summary>
  /// 站点测试项管理API控制器
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class StationTestProjectController : ControllerBase
  {
    private readonly IStationTestProjectService _stationTestProjectService;
    private readonly ILogger<StationTestProjectController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="stationTestProjectService">站点测试项服务</param>
    /// <param name="logger">日志记录器</param>
    public StationTestProjectController(IStationTestProjectService stationTestProjectService, ILogger<StationTestProjectController> logger)
    {
      _stationTestProjectService = stationTestProjectService;
      _logger = logger;
    }

    /// <summary>
    /// 分页查询站点测试项列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页站点测试项列表</returns>
    [HttpGet("GetStationTestProjectList")]
    public async Task<ActionResult<(int Total, List<StationTestProjectDto> Items)>> GetStationTestProjectList([FromQuery] StationTestProjectQueryDto queryDto)
    {
      try
      {
        var result = await _stationTestProjectService.GetListAsync(queryDto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取站点测试项列表时发生错误");
        return StatusCode(500, "获取站点测试项列表失败");
      }
    }

    /// <summary>
    /// 根据ID获取站点测试项详情
    /// </summary>
    /// <param name="id">站点测试项ID</param>
    /// <returns>站点测试项详情</returns>
    [HttpGet("GetStationTestProjectById")]
    public async Task<ActionResult<StationTestProjectDto>> GetStationTestProjectById(Guid id)
    {
      try
      {
        var testProject = await _stationTestProjectService.GetByIdAsync(id);
        if (testProject == null)
        {
          return NotFound($"未找到ID为 {id} 的站点测试项");
        }
        return Ok(testProject);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取站点测试项详情时发生错误，ID: {TestProjectId}", id);
        return StatusCode(500, "获取站点测试项详情失败");
      }
    }

    /// <summary>
    /// 根据站点ID获取测试项列表
    /// </summary>
    /// <param name="stationId">站点ID</param>
    /// <returns>测试项列表</returns>
    [HttpGet("GetStationTestProjectByStationId")]
    public async Task<ActionResult<List<StationTestProjectDto>>> GetStationTestProjectByStationId(Guid stationId)
    {
      try
      {
        var testProjects = await _stationTestProjectService.GetByStationIdAsync(stationId);
        return Ok(testProjects);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据站点ID获取测试项列表时发生错误，站点ID: {StationId}", stationId);
        return StatusCode(500, "根据站点ID获取测试项列表失败");
      }
    }

    /// <summary>
    /// 创建站点测试项
    /// </summary>
    /// <param name="dto">创建站点测试项参数</param>
    /// <returns>创建的站点测试项信息</returns>
    [HttpPost("CreateStationTestProject")]
    public async Task<ActionResult<StationTestProjectDto>> CreateStationTestProject([FromBody] StationTestProjectCreateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        var result = await _stationTestProjectService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetStationTestProjectById), new { id = result.StationTestProjectId }, result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建站点测试项时发生错误");
        return StatusCode(500, "创建站点测试项失败");
      }
    }

    /// <summary>
    /// 更新站点测试项
    /// </summary>
    /// <param name="id">站点测试项ID</param>
    /// <param name="dto">更新站点测试项参数</param>
    /// <returns>更新后的站点测试项信息</returns>
    [HttpPost("UpdateStationTestProjectById")]
    public async Task<ActionResult<StationTestProjectDto>> UpdateStationTestProject(Guid id, [FromBody] StationTestProjectUpdateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        if (dto.StationTestProjectId != id)
        {
          return BadRequest("URL中的ID与请求体中的ID不匹配");
        }

        var result = await _stationTestProjectService.UpdateAsync(dto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新站点测试项时发生错误，ID: {TestProjectId}", id);
        return StatusCode(500, "更新站点测试项失败");
      }
    }

    /// <summary>
    /// 删除站点测试项（支持单个删除和批量删除）
    /// </summary>
    /// <param name="ids">站点测试项ID列表</param>
    /// <returns>删除结果</returns>
    [HttpPost("DeleteStationTestProjectByIds")]
    public async Task<ActionResult<object>> DeleteStationTestProjectByIds([FromBody] List<Guid> ids)
    {
      try
      {
        if (ids == null || ids.Count == 0)
        {
          return BadRequest("站点测试项ID列表不能为空");
        }
        var result = await _stationTestProjectService.DeleteStationTestProjectsAsync(ids.ToArray());
        return Ok(new { deletedCount = result, message = $"成功删除 {result} 个站点测试项" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除站点测试项时发生错误,IDs:{@TestProjectIds}", ids);
        return StatusCode(500, new { message = $"{ex.GetType().FullName}: {ex.Message}" });
      }
    }
  }
}