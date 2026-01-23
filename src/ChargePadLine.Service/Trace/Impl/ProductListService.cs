using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.Product;
using ChargePadLine.Service;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Impl
{
  /// <summary>
  /// 产品列表服务实现
  /// </summary>
  public class ProductListService : IProductListService
  {
    private readonly IRepository<ProductList> _productListRepo;
    private readonly ILogger<ProductListService> _logger;
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="productListRepo">产品列表仓储</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="dbContext">数据库上下文</param>
    public ProductListService(
        IRepository<ProductList> productListRepo,
        ILogger<ProductListService> logger,
        AppDbContext dbContext)
    {
      _productListRepo = productListRepo;
      _logger = logger;
      _dbContext = dbContext;
    }

    /// <summary>
    /// 分页查询产品列表
    /// </summary>
    public async Task<PaginatedList<ProductListDto>> GetProductListsAsync(ProductListQueryDto queryDto)
    {
      try
      {
        // 验证分页参数
        if (queryDto.PageIndex < 1) queryDto.PageIndex = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        // 构建查询条件
        var query = _productListRepo.GetQueryable();

        // 产品编码模糊匹配
        if (!string.IsNullOrEmpty(queryDto.ProductCode))
        {
          query = query.Where(p => p.ProductCode.Contains(queryDto.ProductCode));
        }

        // 产品名称模糊匹配
        if (!string.IsNullOrEmpty(queryDto.ProductName))
        {
          query = query.Where(p => p.ProductName.Contains(queryDto.ProductName));
        }

        // 产品类别模糊匹配
        if (!string.IsNullOrEmpty(queryDto.ProductType))
        {
          query = query.Where(p => p.ProductType.Contains(queryDto.ProductType));
        }

        // 备注信息模糊匹配
        if (!string.IsNullOrEmpty(queryDto.Remark))
        {
          query = query.Where(p => p.Remark != null && p.Remark.Contains(queryDto.Remark));
        }

        // 获取总数
        var totalCount = await query.CountAsync();

        // 分页
        var productLists = await query
          .Skip((queryDto.PageIndex - 1) * queryDto.PageSize)
          .Take(queryDto.PageSize)
          .ToListAsync();

        // 转换为DTO
        var productListDtos = productLists.Select(p => p.ToDto()).ToList();

        // 返回分页结果
        return new PaginatedList<ProductListDto>(productListDtos, totalCount, queryDto.PageIndex, queryDto.PageSize);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "查询产品列表时发生错误");
        throw;
      }
    }

    /// <summary>
    /// 根据ID获取产品详情
    /// </summary>
    public async Task<ProductListDto?> GetProductListByIdAsync(Guid productListId)
    {
      var productList = await _productListRepo.GetQueryable()
          .FirstOrDefaultAsync(p => p.ProductListId == productListId);

      if (productList == null)
        return null;

      return productList.ToDto();
    }

    /// <summary>
    /// 创建产品
    /// </summary>
    public async Task<ProductListDto> CreateProductListAsync(ProductListCreateDto dto)
    {
      try
      {
        // 验证产品编码唯一性
        var existingProduct = await _productListRepo.GetAsync(p => p.ProductCode == dto.ProductCode);
        if (existingProduct != null)
        {
          _logger.LogError("产品编码已存在: {ProductCode}", dto.ProductCode);
          throw new InvalidOperationException($"产品编码已存在: {dto.ProductCode}");
        }

        // 创建产品实体
        var productList = new ProductList
        {
          ProductListId = Guid.NewGuid(),
          ProductCode = dto.ProductCode,
          ProductName = dto.ProductName,
          BomId = dto.BomId,
          ProcessRouteId = dto.ProcessRouteId,
          ProductType = dto.ProductType,
          Remark = dto.Remark,
          CreateTime = DateTimeOffset.Now
          ,
          MaxReworkCount = dto.MaxReworkCount
        };

        // 保存产品
        var result = await _productListRepo.InsertAsync(productList);

        if (result != null)
        {
          _logger.LogInformation($"成功创建产品，ID: {result.ProductListId}");
          return result.ToDto();
        }
        else
        {
          _logger.LogError("创建产品失败");
          throw new InvalidOperationException("创建产品失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建产品时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 更新产品
    /// </summary>
    public async Task<ProductListDto> UpdateProductListAsync(ProductListUpdateDto dto)
    {
      try
      {
        // 查找现有产品
        var existingProduct = await _productListRepo.GetAsync(p => p.ProductListId == dto.ProductListId);
        if (existingProduct == null)
          throw new InvalidOperationException($"产品不存在，ID: {dto.ProductListId}");

        // 检查产品编码唯一性（排除当前产品）
        if (dto.ProductCode != existingProduct.ProductCode)
        {
          var existingProductWithSameCode = await _productListRepo.GetAsync(p => p.ProductCode == dto.ProductCode && p.ProductListId != dto.ProductListId);
          if (existingProductWithSameCode != null)
          {
            _logger.LogError("产品编码已存在: {ProductCode}", dto.ProductCode);
            throw new InvalidOperationException($"产品编码已存在: {dto.ProductCode}");
          }
        }

        // 更新产品信息
        existingProduct.ProductCode = dto.ProductCode;
        existingProduct.ProductName = dto.ProductName;
        existingProduct.BomId = dto.BomId;
        existingProduct.ProcessRouteId = dto.ProcessRouteId;
        existingProduct.ProductType = dto.ProductType;
        existingProduct.Remark = dto.Remark;
        existingProduct.UpdateTime = DateTimeOffset.Now;
        existingProduct.MaxReworkCount = dto.MaxReworkCount;

        // 保存更新
        var result = await _productListRepo.UpdateAsync(existingProduct);

        if (result != null)
        {
          _logger.LogInformation($"成功更新产品，ID: {result.ProductListId}");
          return result.ToDto();
        }
        else
        {
          _logger.LogError("更新产品失败");
          throw new InvalidOperationException("更新产品失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新产品时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 删除产品（支持单一和批量删除）
    /// </summary>
    public async Task<int> DeleteProductListsAsync(Guid[] productListIds)
    {
      if (productListIds == null || productListIds.Length == 0)
        return 0; // 没有要删除的产品

      try
      {
        var products = await _dbContext.ProductList.Where(p => productListIds.Contains(p.ProductListId)).ToListAsync();
        if (products.Count == 0)
        {
          return 0;
        }

        _dbContext.ProductList.RemoveRange(products);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"成功删除产品，共删除 {products.Count} 个");
        return products.Count;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除产品时发生异常");
        throw;
      }
    }
  }
}
