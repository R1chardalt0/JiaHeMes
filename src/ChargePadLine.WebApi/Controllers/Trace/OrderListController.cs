using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargePadLine.WebApi.Controllers.Trace
{
  /// <summary>
  /// 工单列表管理API控制器
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class OrderListController : ControllerBase
  {
    private readonly IOrderListService _orderListService;
    private readonly ILogger<OrderListController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="orderListService">工单列表服务</param>
    /// <param name="logger">日志记录器</param>
    public OrderListController(IOrderListService orderListService, ILogger<OrderListController> logger)
    {
      _orderListService = orderListService;
      _logger = logger;
    }

    /// <summary>
    /// 分页查询工单列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页工单列表</returns>
    [HttpGet("GetOrderListList")]
    public async Task<ActionResult<PaginatedList<OrderListDto>>> GetOrderLists([FromQuery] OrderListQueryDto queryDto)
    {
      try
      {
        var result = await _orderListService.GetOrderListsAsync(queryDto);
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
    /// <param name="orderListId">工单ID</param>
    /// <returns>工单详情</returns>
    [HttpGet("GetOrderListById")]
    public async Task<ActionResult<OrderListDto>> GetOrderList(Guid orderListId)
    {
      try
      {
        var orderList = await _orderListService.GetOrderListByIdAsync(orderListId);
        if (orderList == null)
        {
          return NotFound($"未找到ID为 {orderListId} 的工单");
        }
        return Ok(orderList);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工单详情时发生错误，ID: {OrderListId}", orderListId);
        return StatusCode(500, "获取工单详情失败");
      }
    }

    /// <summary>
    /// 根据工单编码获取工单详情
    /// </summary>
    /// <param name="orderCode">工单编码</param>
    /// <returns>工单详情</returns>
    [HttpGet("GetOrderListByCode")]
    public async Task<ActionResult<OrderListDto>> GetOrderListByCode(string orderCode)
    {
      try
      {
        if (string.IsNullOrEmpty(orderCode))
        {
          return BadRequest("工单编码不能为空");
        }
        var orderList = await _orderListService.GetOrderListByCodeAsync(orderCode);
        if (orderList == null)
        {
          return NotFound($"未找到编码为 {orderCode} 的工单");
        }
        return Ok(orderList);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据工单编码获取工单详情时发生错误，编码: {OrderCode}", orderCode);
        return StatusCode(500, "获取工单详情失败");
      }
    }

    /// <summary>
    /// 创建工单
    /// </summary>
    /// <param name="dto">创建工单DTO</param>
    /// <returns>创建成功的工单信息</returns>
    [HttpPost("CreateOrderList")]
    public async Task<ActionResult<OrderListDto>> CreateOrderList([FromBody] OrderListCreateDto dto)
    {
      try
      {
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }
        var result = await _orderListService.CreateOrderListAsync(dto);
        return CreatedAtAction(nameof(GetOrderList), new { orderListId = result.OrderListId }, result);
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
    /// <param name="orderListId">工单ID</param>
    /// <param name="dto">更新工单DTO</param>
    /// <returns>更新后的工单信息</returns>
    [HttpPost("UpdateOrderListById")]
    public async Task<ActionResult<OrderListDto>> UpdateOrderList(Guid orderListId, [FromBody] OrderListUpdateDto dto)
    {
      try
      {
        // 添加调试日志以查看ID值
        _logger.LogInformation("后端调试 - URL查询参数中的ID: {UrlOrderId}, 请求体中的ID: {BodyOrderId}", orderListId, dto?.OrderListId);

        // 验证输入参数
        if (dto == null)
        {
          _logger.LogWarning("请求数据为空");
          return BadRequest("请求数据不能为空");
        }

        if (dto.OrderListId != orderListId)
        {
          _logger.LogWarning("ID不匹配 - URL查询参数ID: {UrlOrderId}, 请求体ID: {BodyOrderId}", orderListId, dto.OrderListId);
          return BadRequest("URL中的ID与请求体中的ID不匹配");
        }

        var result = await _orderListService.UpdateOrderListAsync(dto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新工单时发生错误，ID: {OrderListId}", orderListId);
        return StatusCode(500, "更新工单失败");
      }
    }

    /// <summary>
    /// 删除工单（支持单个删除和批量删除）
    /// </summary>
    /// <param name="ids">工单ID列表</param>
    /// <returns>删除结果</returns>
    [HttpPost("DeleteOrderListByIds")]
    public async Task<ActionResult<object>> DeleteOrderListByIds([FromBody] List<Guid> ids)
    {
      try
      {
        if (ids == null || ids.Count == 0)
        {
          return BadRequest("工单ID列表不能为空");
        }
        var result = await _orderListService.DeleteOrderListsAsync(ids.ToArray());
        return Ok(new { deletedCount = result, message = $"成功删除 {result} 个工单" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除工单时发生错误,IDs:{@OrderListIds}", ids);
        return StatusCode(500, "删除工单失败");
      }
    }

    /// <summary>
    /// 更新工单状态
    /// </summary>
    /// <param name="orderListId">工单ID</param>
    /// <param name="orderStatus">新状态</param>
    /// <returns>更新后的工单信息</returns>
    [HttpPost("UpdateOrderStatus")]
    public async Task<ActionResult<OrderListDto>> UpdateOrderStatus(Guid orderListId, int orderStatus)
    {
      try
      {
        var result = await _orderListService.UpdateOrderStatusAsync(orderListId, orderStatus);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新工单状态时发生错误，ID: {OrderListId}, 状态: {OrderStatus}", orderListId, orderStatus);
        return StatusCode(500, "更新工单状态失败");
      }
    }

    /// <summary>
    /// 获取工单统计信息
    /// </summary>
    /// <returns>工单状态统计</returns>
    [HttpGet("GetOrderStatistics")]
    public async Task<ActionResult<Dictionary<string, int>>> GetOrderStatistics()
    {
      try
      {
        var result = await _orderListService.GetOrderStatisticsAsync();
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工单统计信息时发生错误");
        return StatusCode(500, "获取工单统计信息失败");
      }
    }
  }
}