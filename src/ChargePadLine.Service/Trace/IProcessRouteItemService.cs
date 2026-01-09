using ChargePadLine.Service.Trace.Dto.ProcessRoute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// 工艺路线子项服务接口
  /// </summary>
  public interface IProcessRouteItemService
  {
    /// <summary>
    /// 分页查询工艺路线子项列表
    /// </summary>
    /// <param name="queryDto">查询条件DTO</param>
    /// <returns>分页查询结果</returns>
    Task<PaginatedList<ProcessRouteItemDto>> GetProcessRouteItemsAsync(ProcessRouteItemQueryDto queryDto);

    /// <summary>
    /// 根据ID获取工艺路线子项详情
    /// </summary>
    /// <param name="routeItemId">工艺路线子项ID</param>
    /// <returns>工艺路线子项详情</returns>
    Task<ProcessRouteItemDto?> GetProcessRouteItemByIdAsync(Guid routeItemId);

    /// <summary>
    /// 根据HeadId获取工艺路线子项列表
    /// </summary>
    /// <param name="headId">工艺路线ID</param>
    /// <returns>工艺路线子项列表</returns>
    Task<IEnumerable<ProcessRouteItemDto>> GetProcessRouteItemsByHeadIdAsync(Guid headId);

    /// <summary>
    /// 创建工艺路线子项
    /// </summary>
    /// <param name="dto">创建工艺路线子项DTO</param>
    /// <returns>创建成功的工艺路线子项信息</returns>
    Task<ProcessRouteItemDto> CreateProcessRouteItemAsync(ProcessRouteItemCreateDto dto);

    /// <summary>
    /// 更新工艺路线子项
    /// </summary>
    /// <param name="dto">更新工艺路线子项DTO</param>
    /// <returns>更新后的工艺路线子项信息</returns>
    Task<ProcessRouteItemDto> UpdateProcessRouteItemAsync(ProcessRouteItemUpdateDto dto);

    /// <summary>
    /// 删除工艺路线子项（支持单一和批量删除）
    /// </summary>
    /// <param name="routeItemIds">工艺路线子项ID数组</param>
    /// <returns>删除成功的数量</returns>
    Task<int> DeleteProcessRouteItemsAsync(Guid[] routeItemIds);
  }
}