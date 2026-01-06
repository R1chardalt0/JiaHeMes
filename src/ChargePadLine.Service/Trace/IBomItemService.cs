using ChargePadLine.Service.Trace.Dto.BOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// BOM子项服务接口
  /// </summary>
  public interface IBomItemService
  {
    /// <summary>
    /// 分页查询BOM子项列表
    /// </summary>
    /// <param name="queryDto">查询条件DTO</param>
    /// <returns>分页查询结果</returns>
    Task<PaginatedList<BomItemDto>> GetBomItemsAsync(BomItemQueryDto queryDto);

    /// <summary>
    /// 根据ID获取BOM子项详情
    /// </summary>
    /// <param name="bomItemId">BOM子项ID</param>
    /// <returns>BOM子项详情</returns>
    Task<BomItemDto?> GetBomItemByIdAsync(Guid bomItemId);

    /// <summary>
    /// 根据BomId获取BOM子项列表
    /// </summary>
    /// <param name="bomId">BOM ID</param>
    /// <returns>BOM子项列表</returns>
    Task<IEnumerable<BomItemDto>> GetBomItemsByBomIdAsync(Guid bomId);

    /// <summary>
    /// 创建BOM子项
    /// </summary>
    /// <param name="dto">创建BOM子项DTO</param>
    /// <returns>创建成功的BOM子项信息</returns>
    Task<BomItemDto> CreateBomItemAsync(BomItemCreateDto dto);

    /// <summary>
    /// 更新BOM子项
    /// </summary>
    /// <param name="dto">更新BOM子项DTO</param>
    /// <returns>更新后的BOM子项信息</returns>
    Task<BomItemDto> UpdateBomItemAsync(BomItemUpdateDto dto);

    /// <summary>
    /// 删除BOM子项（支持单一和批量删除）
    /// </summary>
    /// <param name="bomItemIds">BOM子项ID数组</param>
    /// <returns>删除成功的数量</returns>
    Task<int> DeleteBomItemsAsync(Guid[] bomItemIds);
  }
}