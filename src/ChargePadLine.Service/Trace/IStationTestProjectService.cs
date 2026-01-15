using ChargePadLine.Service.Trace.Dto.Station;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// 站点测试项服务接口
  /// </summary>
  public interface IStationTestProjectService
  {
    /// <summary>
    /// 创建站点测试项
    /// </summary>
    /// <param name="dto">测试项创建数据</param>
    /// <returns>创建的测试项信息</returns>
    Task<StationTestProjectDto> CreateAsync(StationTestProjectCreateDto dto);

    /// <summary>
    /// 更新站点测试项
    /// </summary>
    /// <param name="dto">测试项更新数据</param>
    /// <returns>更新后的测试项信息</returns>
    Task<StationTestProjectDto> UpdateAsync(StationTestProjectUpdateDto dto);



    /// <summary>
    /// 获取站点测试项详情
    /// </summary>
    /// <param name="testProjectId">测试项ID</param>
    /// <returns>测试项详情</returns>
    Task<StationTestProjectDto> GetByIdAsync(Guid testProjectId);

    /// <summary>
    /// 获取站点测试项列表
    /// </summary>
    /// <param name="queryDto">查询条件</param>
    /// <returns>分页结果</returns>
    Task<(int Total, List<StationTestProjectDto> Items)> GetListAsync(StationTestProjectQueryDto queryDto);

    /// <summary>
    /// 分页查询站点测试项列表
    /// </summary>
    /// <param name="queryDto">查询条件</param>
    /// <returns>分页结果</returns>
    Task<ChargePadLine.Service.PaginatedList<StationTestProjectDto>> PaginationAsync(StationTestProjectQueryDto queryDto);

    /// <summary>
    /// 根据站点ID获取测试项列表
    /// </summary>
    /// <param name="stationId">站点ID</param>
    /// <returns>测试项列表</returns>
    Task<List<StationTestProjectDto>> GetByStationIdAsync(Guid stationId);

    /// <summary>
    /// 创建站点测试项
    /// </summary>
    /// <param name="dto">测试项信息</param>
    /// <returns>创建的测试项信息</returns>
    Task<StationTestProjectDto> CreateStationTestProject(StationTestProjectDto dto);

    /// <summary>
    /// 更新站点测试项
    /// </summary>
    /// <param name="dto">测试项信息</param>
    /// <returns>更新后的测试项信息</returns>
    Task<StationTestProjectDto> UpdateStationTestProject(StationTestProjectDto dto);

    /// <summary>
    /// 删除站点测试项（支持单一和批量删除）
    /// </summary>
    /// <param name="testProjectIds">测试项ID数组</param>
    /// <returns>删除成功的数量</returns>
    Task<int> DeleteStationTestProjectsAsync(Guid[] testProjectIds);
  }
}