using ChargePadLine.Service.Trace.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// 产品列表服务接口
  /// </summary>
  public interface IProductListService
  {
    /// <summary>
    /// 分页查询产品列表
    /// </summary>
    /// <param name="queryDto">查询条件DTO</param>
    /// <returns>分页查询结果</returns>
    Task<PaginatedList<ProductListDto>> GetProductListsAsync(ProductListQueryDto queryDto);

    /// <summary>
    /// 根据ID获取产品详情
    /// </summary>
    /// <param name="productListId">产品ID</param>
    /// <returns>产品详情</returns>
    Task<ProductListDto?> GetProductListByIdAsync(Guid productListId);

    /// <summary>
    /// 创建产品
    /// </summary>
    /// <param name="dto">创建产品DTO</param>
    /// <returns>创建成功的产品信息</returns>
    Task<ProductListDto> CreateProductListAsync(ProductListCreateDto dto);

    /// <summary>
    /// 更新产品
    /// </summary>
    /// <param name="dto">更新产品DTO</param>
    /// <returns>更新后的产品信息</returns>
    Task<ProductListDto> UpdateProductListAsync(ProductListUpdateDto dto);

    /// <summary>
    /// 删除产品（支持单一和批量删除）
    /// </summary>
    /// <param name="productListIds">产品ID数组</param>
    /// <returns>删除成功的数量</returns>
    Task<int> DeleteProductListsAsync(Guid[] productListIds);
  }
}
