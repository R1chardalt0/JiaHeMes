using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Service;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// 追溯信息服务接口
  /// 提供查询和删除功能，禁止新增和编辑
  /// </summary>
  public interface ITraceInfoService
  {
    /// <summary>
    /// 分页查询追溯信息
    /// </summary>
    /// <param name="current">当前页码</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="id">ID(模糊匹配)</param>
    /// <param name="pin">产品识别码(模糊匹配)</param>
    /// <param name="vsn">VSN(模糊匹配)</param>
    /// <returns>分页追溯信息列表</returns>
    Task<PaginatedList<TraceInfo>> PaginationAsync(int current, int pageSize, string? id, string? pin, string? vsn);

    /// <summary>
    /// 根据条件查询追溯信息列表（支持分页）
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="pin">产品识别码</param>
    /// <param name="vsn">VSN</param>
    /// <param name="page">页码</param>
    /// <param name="size">每页大小</param>
    /// <returns>分页追溯信息列表</returns>
    Task<PaginatedList<TraceInfo>> GetListAsync(Guid? id, string? pin, string? vsn, int page, int size);

    /// <summary>
    /// 根据ID获取追溯信息
    /// </summary>
    /// <param name="id">追溯信息ID</param>
    /// <returns>追溯信息实体</returns>
    Task<TraceInfo?> GetByIdAsync(Guid id);

    /// <summary>
    /// 批量删除追溯信息
    /// </summary>
    /// <param name="ids">追溯信息ID列表</param>
    /// <returns>删除结果</returns>
    Task<bool> DeleteByIdsAsync(List<Guid> ids);
  }
}