using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Service;

namespace ChargePadLine.WebApi.Controllers.Trace
{
  /// <summary>
  /// 工单管理API控制器
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class WorkOrderController : ControllerBase
  {
    private readonly IWorkOrderService _workOrderService;
    private readonly ILogger<WorkOrderController> _logger;

    public WorkOrderController(IWorkOrderService workOrderService, ILogger<WorkOrderController> logger)
    {
      _workOrderService = workOrderService;
      _logger = logger;
    }

    /// <summary>
    /// 分页查询工单列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页工单列表</returns>
    [HttpGet]
    public async Task<ActionResult<PaginatedList<WorkOrderDto>>> GetWorkOrders([FromQuery] WorkOrderQueryDto queryDto)
    {
      try
      {
        var result = await _workOrderService.GetWorkOrdersAsync(queryDto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工单列表时发生错误");
        return StatusCode(500, "获取工单列表失败");
      }
    }

    /// <summary>
    /// 根据ID获取工单详情
    /// </summary>
    /// <param name="id">工单ID</param>
    /// <returns>工单详情</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkOrderDto>> GetWorkOrder(int id)
    {
      try
      {
        var workOrder = await _workOrderService.GetWorkOrderByIdAsync(id);
        if (workOrder == null)
        {
          return NotFound($"未找到ID为 {id} 的工单");
        }
        return Ok(workOrder);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工单详情时发生错误，ID: {WorkOrderId}", id);
        return StatusCode(500, "获取工单详情失败");
      }
    }



    /// <summary>
    /// 创建工单
    /// </summary>
    /// <param name="dto">创建工单DTO</param>
    /// <returns>创建成功的工单信息</returns>
    [HttpPost]
    public async Task<ActionResult<WorkOrderDto>> CreateWorkOrder([FromBody] CreateWorkOrderDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        var result = await _workOrderService.CreateWorkOrderAsync(dto);
        return CreatedAtAction(nameof(GetWorkOrder), new { id = result.Id }, result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建工单时发生错误");
        return StatusCode(500, "创建工单失败");
      }
    }

    /// <summary>
    /// 更新工单
    /// </summary>
    /// <param name="id">工单ID</param>
    /// <param name="dto">更新工单DTO</param>
    /// <returns>更新后的工单信息</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<WorkOrderDto>> UpdateWorkOrder(int id, [FromBody] UpdateWorkOrderDto dto)
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

        var result = await _workOrderService.UpdateWorkOrderAsync(dto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新工单时发生错误，ID: {WorkOrderId}", id);
        return StatusCode(500, "更新工单失败");
      }
    }

    /// <summary>
    /// 删除工单
    /// </summary>
    /// <param name="id">工单ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteWorkOrder(int id)
    {
      try
      {
        var result = await _workOrderService.DeleteWorkOrderAsync(id);
        if (result)
        {
          return Ok(new { success = true, message = "工单删除成功" });
        }
        else
        {
          return NotFound(new { success = false, message = $"未找到ID为 {id} 的工单" });
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除工单时发生错误，ID: {WorkOrderId}", id);
        return StatusCode(500, "删除工单失败");
      }
    }

    /// <summary>
    /// 批量删除工单
    /// </summary>
    /// <param name="ids">工单ID数组</param>
    /// <returns>实际删除的工单数量</returns>
    [HttpDelete]
    public async Task<ActionResult<int>> DeleteWorkOrders([FromBody] int[] ids)
    {
      try
      {
        if (ids == null || ids.Length == 0)
        {
          return BadRequest("工单ID数组不能为空");
        }

        var result = await _workOrderService.DeleteWorkOrdersAsync(ids);
        return Ok(new { deletedCount = result, message = $"成功删除 {result} 个工单" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "批量删除工单时发生错误");
        return StatusCode(500, "批量删除工单失败");
      }
    }
  }
}