using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.Order;
using ChargePadLine.WebApi.Controllers.util;
using ChargePadLine.WebApi.util;    

namespace ChargePadLine.WebApi.Controllers.Trace
{
  /// <summary>
  /// MES工单BOM批次控制器
  /// </summary>
  [Route("api/trace/[controller]")]
  [ApiController]
  public class MESOrderBomBatchController : ControllerBase
  {
    private readonly IMESOrderBomBatchService _service;
    private readonly ILogger<MESOrderBomBatchController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="service">MES工单BOM批次服务</param>
    /// <param name="logger">日志记录器</param>
    public MESOrderBomBatchController(IMESOrderBomBatchService service, ILogger<MESOrderBomBatchController> logger)
    {
      _service = service;
      _logger = logger;
    }

    /// <summary>
    /// 根据ID查询工单BOM批次
    /// </summary>
    /// <param name="id">工单BOM批次ID</param>
    /// <returns>工单BOM批次数据传输对象</returns>
    [HttpGet("GetMESOrderBomBatchById")]
    public async Task<ActionResult<MESOrderBomBatchDto>> GetMESOrderBomBatchById(Guid id)
    {
      try
      {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
        {
          return NotFound($"未找到ID为 {id} 的工单BOM批次");
        }
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工单BOM批次详情时发生错误，ID: {OrderBomBatchId}", id);
        return StatusCode(500, "获取工单BOM批次详情失败");
      }
    }

    /// <summary>
    /// 查询工单BOM批次列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>工单BOM批次数据传输对象列表</returns>
    [HttpGet("GetMESOrderBomBatchList")]
    public async Task<ActionResult<List<MESOrderBomBatchDto>>> GetMESOrderBomBatchList([FromQuery] MESOrderBomBatchQueryDto queryDto)
    {
      try
      {
        var result = await _service.GetListAsync(queryDto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工单BOM批次列表时发生错误");
        return StatusCode(500, "获取工单BOM批次列表失败");
      }
    }

    /// <summary>
    /// 分页查询工单BOM批次
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页结果，包含工单BOM批次数据传输对象列表和总记录数</returns>
    [HttpGet("GetMESOrderBomBatchPagedList")]
    public async Task<ActionResult<PagedResp<MESOrderBomBatchDto>>> GetMESOrderBomBatchPagedList([FromQuery] MESOrderBomBatchQueryDto queryDto)
    {
      try
      {
        var result = await _service.GetPagedListAsync(queryDto);
        return Ok(RespExtensions.MakePagedSuccess(result));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "分页获取工单BOM批次列表时发生错误");
        return StatusCode(500, RespExtensions.MakeFail("500", "分页获取工单BOM批次列表失败"));
      }
    }

    /// <summary>
    /// 根据工单ID查询工单BOM批次列表
    /// </summary>
    /// <param name="orderListId">工单ID</param>
    /// <returns>工单BOM批次数据传输对象列表</returns>
    [HttpGet("GetMESOrderBomBatchByOrderListId")]
    public async Task<ActionResult<List<MESOrderBomBatchDto>>> GetMESOrderBomBatchByOrderId(Guid orderListId)
    {
      try
      {
        var result = await _service.GetByOrderListIdAsync(orderListId);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据工单ID获取工单BOM批次列表时发生错误，工单ID: {OrderListId}", orderListId);
        return StatusCode(500, "根据工单ID获取工单BOM批次列表失败");
      }
    }

    /// <summary>
    /// 根据物料ID查询工单BOM批次列表
    /// </summary>
    /// <param name="productListId">物料ID</param>
    /// <returns>工单BOM批次数据传输对象列表</returns>
    [HttpGet("GetMESOrderBomBatchByProductListId")]
    public async Task<ActionResult<List<MESOrderBomBatchDto>>> GetMESOrderBomBatchByProductId(Guid productListId)
    {
      try
      {
        var result = await _service.GetByProductListIdAsync(productListId);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据物料ID获取工单BOM批次列表时发生错误，物料ID: {ProductListId}", productListId);
        return StatusCode(500, "根据物料ID获取工单BOM批次列表失败");
      }
    }
  }
}
