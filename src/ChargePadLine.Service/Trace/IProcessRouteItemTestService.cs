using ChargePadLine.Service.Trace.Dto.ProcessRoute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// 工艺路线工位测试服务接口
  /// </summary>
  public interface IProcessRouteItemTestService
  {
    /// <summary>
    /// 创建工艺路线工位测试
    /// </summary>
    /// <param name="dto">测试创建数据</param>
    /// <returns>创建的测试信息</returns>
    Task<ProcessRouteItemTestDto> CreateAsync(ProcessRouteItemTestCreateDto dto);

    /// <summary>
    /// 更新工艺路线工位测试
    /// </summary>
    /// <param name="dto">测试更新数据</param>
    /// <returns>更新后的测试信息</returns>
    Task<ProcessRouteItemTestDto> UpdateAsync(ProcessRouteItemTestUpdateDto dto);

    /// <summary>
    /// 获取工艺路线工位测试详情
    /// </summary>
    /// <param name="testId">测试ID</param>
    /// <returns>测试详情</returns>
    Task<ProcessRouteItemTestDto> GetByIdAsync(Guid testId);

    /// <summary>
    /// 获取工艺路线工位测试列表
    /// </summary>
    /// <param name="queryDto">查询条件</param>
    /// <returns>分页结果</returns>
    Task<(int Total, List<ProcessRouteItemTestDto> Items)> GetListAsync(ProcessRouteItemTestQueryDto queryDto);

    /// <summary>
    /// 分页查询工艺路线工位测试列表
    /// </summary>
    /// <param name="queryDto">查询条件</param>
    /// <returns>分页结果</returns>
    Task<ChargePadLine.Service.PaginatedList<ProcessRouteItemTestDto>> PaginationAsync(ProcessRouteItemTestQueryDto queryDto);

    /// <summary>
    /// 根据工艺路线明细ID获取测试列表
    /// </summary>
    /// <param name="processRouteItemId">工艺路线明细ID</param>
    /// <returns>测试列表</returns>
    Task<List<ProcessRouteItemTestDto>> GetByProcessRouteItemIdAsync(Guid processRouteItemId);

    /// <summary>
    /// 删除工艺路线工位测试（支持单一和批量删除）
    /// </summary>
    /// <param name="testIds">测试ID数组</param>
    /// <returns>删除成功的数量</returns>
    Task<int> DeleteAsync(Guid[] testIds);
  }
}