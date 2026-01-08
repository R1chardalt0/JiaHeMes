// using ChargePadLine.Service.Trace;
// using ChargePadLine.Service.Trace.Dto;
// using Microsoft.AspNetCore.Mvc;
// using ChargePadLine.Service;

// namespace ChargePadLine.WebApi.Controllers.Trace
// {
//   /// <summary>
//   /// 工单管理API控制器
//   /// </summary>
//   [ApiController]
//   [Route("api/[controller]")]
//   public class WorkOrderController : ControllerBase
//   {
//     private readonly IWorkOrderService _workOrderService;
//     private readonly ILogger<WorkOrderController> _logger;

//     public WorkOrderController(IWorkOrderService workOrderService, ILogger<WorkOrderController> logger)
//     {
//       _workOrderService = workOrderService;
//       _logger = logger;
//     }

//     /// <summary>
//     /// 分页查询工单列表
//     /// </summary>
//     /// <param name="queryDto">查询参数</param>
//     /// <returns>分页工单列表</returns>
//     [HttpGet("GetWorkOrderList")]
//     public async Task<ActionResult<PaginatedList<WorkOrderDto>>> GetWorkOrders([FromQuery] WorkOrderQueryDto queryDto)
//     {
//       try
//       {
//         var result = await _workOrderService.GetWorkOrdersAsync(queryDto);
//         return Ok(result);
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "获取工单列表时发生错误");
//         return StatusCode(500, "获取工单列表失败");
//       }
//     }

//     /// <summary>
//     /// 根据ID获取工单详情
//     /// </summary>

//     [HttpGet("GetWorkOrderById")]
//     public async Task<ActionResult<WorkOrderDto>> GetWorkOrder(int id)
//     {
//       try
//       {
//         var workOrder = await _workOrderService.GetWorkOrderByIdAsync(id);
//         if (workOrder == null)
//         {
//           return NotFound($"未找到ID为 {id} 的工单");
//         }
//         return Ok(workOrder);
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "获取工单详情时发生错误，ID: {WorkOrderId}", id);
//         return StatusCode(500, "获取工单详情失败");
//       }
//     }

//     /// <summary>
//     /// 创建工单
//     /// </summary>

//     [HttpPost("CreateWorkOrder")]
//     public async Task<ActionResult<WorkOrderDto>> CreateWorkOrder([FromBody] CreateWorkOrderDto dto)
//     {
//       try
//       {
//         // 验证输入参数
//         if (dto == null)
//         {
//           return BadRequest("请求数据不能为空");
//         }

//         var result = await _workOrderService.CreateWorkOrderAsync(dto);
//         return CreatedAtAction(nameof(GetWorkOrder), new { id = result.Id }, result);
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "创建工单时发生错误");
//         return StatusCode(500, "创建工单失败");
//       }
//     }

//     /// <summary>
//     /// 更新工单
//     /// </summary>

//     [HttpPost("UpdateWorkOrderById")]
//     public async Task<ActionResult<WorkOrderDto>> UpdateWorkOrder(int id, [FromBody] UpdateWorkOrderDto dto)
//     {
//       try
//       {
//         // 验证输入参数
//         if (dto == null)
//         {
//           return BadRequest("请求数据不能为空");
//         }

//         if (dto.Id != id)
//         {
//           return BadRequest("URL中的ID与请求体中的ID不匹配");
//         }

//         var result = await _workOrderService.UpdateWorkOrderAsync(dto);
//         return Ok(result);
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "更新工单时发生错误，ID: {WorkOrderId}", id);
//         return StatusCode(500, "更新工单失败");
//       }
//     }

//     /// <summary>
//     /// 删除工单（支持单个删除和批量删除）
//     /// </summary>

//     [HttpPost("DeleteWorkOrderByIds")]
//     public async Task<ActionResult<object>> DeleteWorkOrderByIds([FromBody] List<int> ids)
//     {
//       try
//       {
//         if (ids == null || ids.Count == 0)
//         {
//           return BadRequest("工单ID列表不能为空");
//         }
//         var result = await _workOrderService.DeleteWorkOrdersAsync(ids.ToArray());
//         return Ok(new { deletedCount = result, message = $"成功删除 {result} 个工单" });
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "删除工单时发生错误,IDs:{@WorkOrderIds}", ids);
//         return StatusCode(500, "删除工单失败");
//       }
//     }
//   }
// }