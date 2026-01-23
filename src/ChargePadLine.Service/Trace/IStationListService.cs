using ChargePadLine.Service.Trace.Dto.Station;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// 站点服务接口
  /// </summary>
  public interface IStationListService
  {
    /// <summary>
    /// 创建站点
    /// </summary>
    /// <param name="dto">站点创建数据</param>
    /// <returns>创建的站点信息</returns>
    Task<StationListDto> CreateAsync(StationListCreateDto dto);

    /// <summary>
    /// 更新站点
    /// </summary>
    /// <param name="dto">站点更新数据</param>
    /// <returns>更新后的站点信息</returns>
    Task<StationListDto> UpdateAsync(StationListUpdateDto dto);

    /// <summary>
    /// 删除站点
    /// </summary>
    /// <param name="stationId">站点ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(Guid stationId);

    /// <summary>
    /// 获取站点详情
    /// </summary>
    /// <param name="stationId">站点ID</param>
    /// <returns>站点详情</returns>
    Task<StationListDto> GetByIdAsync(Guid stationId);

    /// <summary>
    /// 获取站点列表
    /// </summary>
    /// <param name="queryDto">查询条件</param>
    /// <returns>分页结果</returns>
    Task<(int Total, List<StationListDto> Items)> GetListAsync(StationListQueryDto queryDto);

    /// <summary>
    /// 分页查询站点列表
    /// </summary>
    /// <param name="queryDto">查询条件</param>
    /// <returns>分页结果</returns>
    Task<ChargePadLine.Service.PaginatedList<StationListDto>> PaginationAsync(StationListQueryDto queryDto);

    /// <summary>
    /// 根据ID获取站点详情
    /// </summary>
    /// <param name="stationId">站点ID</param>
    /// <returns>站点详情</returns>
    Task<StationListDto> GetStationInfoById(Guid stationId);

    /// <summary>
    /// 创建站点
    /// </summary>
    /// <param name="dto">站点信息</param>
    /// <returns>创建的站点信息</returns>
    Task<StationListDto> CreateStationInfo(StationListDto dto);

    /// <summary>
    /// 更新站点
    /// </summary>
    /// <param name="dto">站点信息</param>
    /// <returns>更新后的站点信息</returns>
    Task<StationListDto> UpdateStationInfo(StationListDto dto);

    /// <summary>
    /// 删除站点
    /// </summary>
    /// <param name="stationIds">站点ID列表</param>
    /// <returns>删除成功的数量</returns>
    Task<int> DeleteStationInfoById(Guid[] stationIds);
  }
}
