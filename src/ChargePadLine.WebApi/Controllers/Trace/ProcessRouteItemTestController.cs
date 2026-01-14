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
  /// 工艺路线工位测试管理API控制器
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class ProcessRouteItemTestController : ControllerBase
  {
    private readonly IProcessRouteItemTestService _processRouteItemTestService;
    private readonly ILogger<ProcessRouteItemTestController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="processRouteItemTestService">工艺路线工位测试服务</param>
    /// <param name="logger">日志记录器</param>
    public ProcessRouteItemTestController(IProcessRouteItemTestService processRouteItemTestService, ILogger<ProcessRouteItemTestController> logger)
    {
      _processRouteItemTestService = processRouteItemTestService;
      _logger = logger;
    }

    /// <summary>
    /// 分页查询工艺路线工位测试列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页工艺路线工位测试列表</returns>
    [HttpGet("GetProcessRouteItemTestList")]
    public async Task<ActionResult<(int Total, List<ProcessRouteItemTestDto> Items)>> GetProcessRouteItemTestList([FromQuery] ProcessRouteItemTestQueryDto queryDto)
    {
      try
      {
        var result = await _processRouteItemTestService.GetListAsync(queryDto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工艺路线工位测试列表时发生错误");
        return StatusCode(500, "获取工艺路线工位测试列表失败");
      }
    }

    /// <summary>
    /// 根据ID获取工艺路线工位测试详情
    /// </summary>
    /// <param name="id">工艺路线工位测试ID</param>
    /// <returns>工艺路线工位测试详情</returns>
    [HttpGet("GetProcessRouteItemTestById")]
    public async Task<ActionResult<ProcessRouteItemTestDto>> GetProcessRouteItemTestById(Guid id)
    {
      try
      {
        var test = await _processRouteItemTestService.GetByIdAsync(id);
        if (test == null)
        {
          return NotFound($"未找到ID为 {id} 的工艺路线工位测试");
        }
        return Ok(test);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工艺路线工位测试详情时发生错误，ID: {TestId}", id);
        return StatusCode(500, "获取工艺路线工位测试详情失败");
      }
    }

    /// <summary>
    /// 根据工艺路线明细ID获取测试列表
    /// </summary>
    /// <param name="processRouteItemId">工艺路线明细ID</param>
    /// <returns>测试列表</returns>
    [HttpGet("GetProcessRouteItemTestByProcessRouteItemId")]
    public async Task<ActionResult<List<ProcessRouteItemTestDto>>> GetProcessRouteItemTestByProcessRouteItemId(Guid processRouteItemId)
    {
      try
      {
        var tests = await _processRouteItemTestService.GetByProcessRouteItemIdAsync(processRouteItemId);
        return Ok(tests);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据工艺路线明细ID获取测试列表时发生错误，工艺路线明细ID: {ProcessRouteItemId}", processRouteItemId);
        return StatusCode(500, "根据工艺路线明细ID获取测试列表失败");
      }
    }

    /// <summary>
    /// 创建工艺路线工位测试
    /// </summary>
    /// <param name="dto">创建工艺路线工位测试参数</param>
    /// <returns>创建的工艺路线工位测试信息</returns>
    [HttpPost("CreateProcessRouteItemTest")]
    public async Task<ActionResult<ProcessRouteItemTestDto>> CreateProcessRouteItemTest([FromBody] ProcessRouteItemTestCreateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        var result = await _processRouteItemTestService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetProcessRouteItemTestById), new { id = result.ProRouteItemStationTestId }, result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建工艺路线工位测试时发生错误");
        return StatusCode(500, "创建工艺路线工位测试失败");
      }
    }

    /// <summary>
    /// 更新工艺路线工位测试
    /// </summary>
    /// <param name="id">工艺路线工位测试ID</param>
    /// <param name="dto">更新工艺路线工位测试参数</param>
    /// <returns>更新后的工艺路线工位测试信息</returns>
    [HttpPost("UpdateProcessRouteItemTestById")]
    public async Task<ActionResult<ProcessRouteItemTestDto>> UpdateProcessRouteItemTest(Guid id, [FromBody] ProcessRouteItemTestUpdateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        if (dto.ProRouteItemStationTestId != id)
        {
          return BadRequest("URL中的ID与请求体中的ID不匹配");
        }

        var result = await _processRouteItemTestService.UpdateAsync(dto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新工艺路线工位测试时发生错误，ID: {TestId}", id);
        return StatusCode(500, "更新工艺路线工位测试失败");
      }
    }

    /// <summary>
    /// 删除工艺路线工位测试（支持单个删除和批量删除）
    /// </summary>
    /// <param name="ids">工艺路线工位测试ID列表</param>
    /// <returns>删除结果</returns>
    [HttpPost("DeleteProcessRouteItemTestByIds")]
    public async Task<ActionResult<object>> DeleteProcessRouteItemTestByIds([FromBody] List<Guid> ids)
    {
      try
      {
        if (ids == null || ids.Count == 0)
        {
          return BadRequest("工艺路线工位测试ID列表不能为空");
        }
        var result = await _processRouteItemTestService.DeleteAsync(ids.ToArray());
        return Ok(new { deletedCount = result, message = $"成功删除 {result} 个工艺路线工位测试" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除工艺路线工位测试时发生错误,IDs:{@TestIds}", ids);
        return StatusCode(500, new { message = $"{ex.GetType().FullName}: {ex.Message}" });
      }
    }
  }
}