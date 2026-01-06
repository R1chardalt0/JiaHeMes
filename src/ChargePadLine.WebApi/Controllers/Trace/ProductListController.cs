using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ChargePadLine.WebApi.Controllers.Trace
{
  /// <summary>
  /// 产品列表管理API控制器
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class ProductListController : ControllerBase
  {
    private readonly IProductListService _productListService;
    private readonly ILogger<ProductListController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="productListService">产品列表服务</param>
    /// <param name="logger">日志记录器</param>
    public ProductListController(IProductListService productListService, ILogger<ProductListController> logger)
    {
      _productListService = productListService;
      _logger = logger;
    }

    /// <summary>
    /// 分页查询产品列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页产品列表</returns>
    [HttpGet("GetProductListList")]
    public async Task<ActionResult<PaginatedList<ProductListDto>>> GetProductLists([FromQuery] ProductListQueryDto queryDto)
    {
      try
      {
        var result = await _productListService.GetProductListsAsync(queryDto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取产品列表时发生错误");
        return StatusCode(500, "获取产品列表失败");
      }
    }

    /// <summary>
    /// 根据ID获取产品详情
    /// </summary>
    /// <param name="id">产品ID</param>
    /// <returns>产品详情</returns>
    [HttpGet("GetProductListById")]
    public async Task<ActionResult<ProductListDto>> GetProductListById(Guid id)
    {
      try
      {
        var productList = await _productListService.GetProductListByIdAsync(id);
        if (productList == null)
        {
          return NotFound($"未找到ID为 {id} 的产品");
        }
        return Ok(productList);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取产品详情时发生错误，ID: {ProductListId}", id);
        return StatusCode(500, "获取产品详情失败");
      }
    }

    /// <summary>
    /// 创建产品
    /// </summary>
    /// <param name="dto">创建产品参数</param>
    /// <returns>创建的产品信息</returns>
    [HttpPost("CreateProductList")]
    public async Task<ActionResult<ProductListDto>> CreateProductList([FromBody] ProductListCreateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        var result = await _productListService.CreateProductListAsync(dto);
        return CreatedAtAction(nameof(GetProductListById), new { id = result.ProductListId }, result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建产品时发生错误");
        return StatusCode(500, "创建产品失败");
      }
    }

    /// <summary>
    /// 更新产品
    /// </summary>
    /// <param name="id">产品ID</param>
    /// <param name="dto">更新产品参数</param>
    /// <returns>更新后的产品信息</returns>
    [HttpPost("UpdateProductListById")]
    public async Task<ActionResult<ProductListDto>> UpdateProductList(Guid id, [FromBody] ProductListUpdateDto dto)
    {
      try
      {
        // 验证输入参数
        if (dto == null)
        {
          return BadRequest("请求数据不能为空");
        }

        if (dto.ProductListId != id)
        {
          return BadRequest("URL中的ID与请求体中的ID不匹配");
        }

        var result = await _productListService.UpdateProductListAsync(dto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新产品时发生错误，ID: {ProductListId}", id);
        return StatusCode(500, "更新产品失败");
      }
    }

    /// <summary>
    /// 删除产品（支持单个删除和批量删除）
    /// </summary>
    /// <param name="ids">产品ID列表</param>
    /// <returns>删除结果</returns>
    [HttpPost("DeleteProductListByIds")]
    public async Task<ActionResult<object>> DeleteProductListByIds([FromBody] List<Guid> ids)
    {
      try
      {
        if (ids == null || ids.Count == 0)
        {
          return BadRequest("产品ID列表不能为空");
        }
        var result = await _productListService.DeleteProductListsAsync(ids.ToArray());
        return Ok(new { deletedCount = result, message = $"成功删除 {result} 个产品" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除产品时发生错误,IDs:{@ProductListIds}", ids);
        return StatusCode(500, "删除产品失败");
      }
    }
  }
}