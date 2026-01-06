using ChargePadLine.Service.Trace.Dto.BOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// BOM列表服务接口
  /// </summary>
  public interface IBomListService
  {
    /// <summary>
    /// 分页查询BOM列表
    /// </summary>
    /// <param name="queryDto">查询条件DTO</param>
    /// <returns>分页查询结果</returns>
    Task<PaginatedList<BomListDto>> GetBomListsAsync(BomListQueryDto queryDto);

    /// <summary>
    /// 根据ID获取BOM详情
    /// </summary>
    /// <param name="bomId">BOM ID</param>
    /// <returns>BOM详情</returns>
    Task<BomListDto?> GetBomListByIdAsync(Guid bomId);

    /// <summary>
    /// 创建BOM
    /// </summary>
    /// <param name="dto">创建BOM DTO</param>
    /// <returns>创建成功的BOM信息</returns>
    Task<BomListDto> CreateBomListAsync(BomListCreateDto dto);

    /// <summary>
    /// 更新BOM
    /// </summary>
    /// <param name="dto">更新BOM DTO</param>
    /// <returns>更新后的BOM信息</returns>
    Task<BomListDto> UpdateBomListAsync(BomListUpdateDto dto);

    /// <summary>
    /// 删除BOM（支持单一和批量删除）
    /// </summary>
    /// <param name="bomIds">BOM ID数组</param>
    /// <returns>删除成功的数量</returns>
    Task<int> DeleteBomListsAsync(Guid[] bomIds);
  }
}
