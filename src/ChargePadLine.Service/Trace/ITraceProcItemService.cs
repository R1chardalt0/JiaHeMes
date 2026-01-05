using ChargePadLine.Entitys.Trace.TraceInformation;

namespace ChargePadLine.Service.Trace;

/// <summary>
/// 追溯工艺项目服务接口
/// </summary>
public interface ITraceProcItemService
{
  /// <summary>
  /// 分页查询追溯工艺项目
  /// </summary>
  /// <param name="current">当前页码</param>
  /// <param name="pageSize">每页记录数</param>
  /// <param name="id">ID(模糊匹配)</param>
  /// <param name="station">工位位置(模糊匹配)</param>
  /// <param name="key">键(模糊匹配)</param>
  /// <returns>分页追溯工艺项目列表</returns>
  Task<PaginatedList<TraceProcItem>> PaginationAsync(int current, int pageSize, string? id, string? station, string? key);

  /// <summary>
  /// 根据条件查询追溯工艺项目列表（支持分页）
  /// </summary>
  /// <param name="id">ID</param>
  /// <param name="station">工位位置</param>
  /// <param name="key">键</param>
  /// <param name="page">页码</param>
  /// <param name="size">每页大小</param>
  /// <returns>分页追溯工艺项目列表</returns>
  Task<PaginatedList<TraceProcItem>> GetListAsync(Guid? id, string? station, string? key, int page, int size);

  /// <summary>
    /// 根据ID获取追溯工艺项目
    /// </summary>
    /// <param name="id">追溯工艺项目ID</param>
    /// <returns>追溯工艺项目实体</returns>
    Task<TraceProcItem?> GetByIdAsync(Guid id);

    /// <summary>
    /// 根据追溯信息ID获取追溯工艺项目列表
    /// </summary>
    /// <param name="traceInfoId">追溯信息ID</param>
    /// <returns>追溯工艺项目实体列表</returns>
    Task<List<TraceProcItem>> GetByTraceInfoIdAsync(Guid traceInfoId);

    /// <summary>
    /// 批量删除追溯工艺项目
    /// </summary>
    /// <param name="ids">追溯工艺项目ID列表</param>
    /// <returns>删除结果</returns>
    Task<bool> DeleteByIdsAsync(List<Guid> ids);
}