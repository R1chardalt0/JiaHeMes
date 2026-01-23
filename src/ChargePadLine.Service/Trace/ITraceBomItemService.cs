using ChargePadLine.Entitys.Trace.TraceInformation;

namespace ChargePadLine.Service.Trace;

/// <summary>
/// 追溯BOM项目服务接口
/// </summary>
public interface ITraceBomItemService
{
    /// <summary>
    /// 分页查询追溯BOM项目
    /// </summary>
    /// <param name="current">当前页码</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="id">ID(模糊匹配)</param>
    /// <param name="materialName">物料名称(模糊匹配)</param>
    /// <param name="materialCode">物料代码(模糊匹配)</param>
    /// <returns>分页追溯BOM项目列表</returns>
    Task<PaginatedList<TraceBomItem>> PaginationAsync(int current, int pageSize, string? id, string? materialName, string? materialCode);

    /// <summary>
    /// 根据条件查询追溯BOM项目列表（支持分页）
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="materialName">物料名称</param>
    /// <param name="materialCode">物料代码</param>
    /// <param name="page">页码</param>
    /// <param name="size">每页大小</param>
    /// <returns>分页追溯BOM项目列表</returns>
    Task<PaginatedList<TraceBomItem>> GetListAsync(Guid? id, string? materialName, string? materialCode, int page, int size);

    /// <summary>
    /// 根据ID获取追溯BOM项目
    /// </summary>
    /// <param name="id">追溯BOM项目ID</param>
    /// <returns>追溯BOM项目实体</returns>
    Task<TraceBomItem?> GetByIdAsync(Guid id);

    /// <summary>
    /// 根据追溯信息ID获取追溯BOM项目列表
    /// </summary>
    /// <param name="traceInfoId">追溯信息ID</param>
    /// <returns>追溯BOM项目实体列表</returns>
    Task<List<TraceBomItem>> GetByTraceInfoIdAsync(Guid traceInfoId);

    /// <summary>
    /// 批量删除追溯BOM项目
    /// </summary>
    /// <param name="ids">追溯BOM项目ID列表</param>
    /// <returns>删除结果</returns>
    Task<bool> DeleteByIdsAsync(List<Guid> ids);
}