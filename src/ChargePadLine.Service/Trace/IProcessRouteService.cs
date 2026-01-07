using ChargePadLine.Service.Trace.Dto.ProcessRoute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// 工艺路线服务接口
  /// </summary>
  public interface IProcessRouteService
  {
    /// <summary>
    /// 分页查询工艺路线列表
    /// </summary>
    /// <param name="queryDto">查询条件DTO</param>
    /// <returns>分页查询结果</returns>
    Task<PaginatedList<ProcessRouteDto>> GetProcessRoutesAsync(ProcessRouteQueryDto queryDto);

    /// <summary>
    /// 根据ID获取工艺路线详情
    /// </summary>
    /// <param name="routeId">工艺路线ID</param>
    /// <returns>工艺路线详情</returns>
    Task<ProcessRouteDto?> GetProcessRouteByIdAsync(Guid routeId);

    /// <summary>
    /// 创建工艺路线
    /// </summary>
    /// <param name="dto">创建工艺路线DTO</param>
    /// <returns>创建成功的工艺路线信息</returns>
    Task<ProcessRouteDto> CreateProcessRouteAsync(ProcessRouteCreateDto dto);

    /// <summary>
    /// 更新工艺路线
    /// </summary>
    /// <param name="dto">更新工艺路线DTO</param>
    /// <returns>更新后的工艺路线信息</returns>
    Task<ProcessRouteDto> UpdateProcessRouteAsync(ProcessRouteUpdateDto dto);

    /// <summary>
    /// 删除工艺路线（支持单一和批量删除）
    /// </summary>
    /// <param name="routeIds">工艺路线ID数组</param>
    /// <returns>删除成功的数量</returns>
    Task<int> DeleteProcessRoutesAsync(Guid[] routeIds);
  }
}
