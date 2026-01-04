using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.DbContexts.Repository;
using Microsoft.EntityFrameworkCore;
using ChargePadLine.Service;

namespace ChargePadLine.Service.Trace.Impl;

/// <summary>
/// 追溯工艺项目服务实现
/// </summary>
public class TraceProcItemService : ITraceProcItemService
{
  private readonly IRepository<TraceProcItem> _repository;

  public TraceProcItemService(IRepository<TraceProcItem> repository)
  {
    _repository = repository;
  }

  /// <summary>
  /// 分页查询追溯工艺项目
  /// </summary>
  /// <param name="current">当前页码</param>
  /// <param name="pageSize">每页记录数</param>
  /// <param name="id">ID(模糊匹配)</param>
  /// <param name="station">工位位置(模糊匹配)</param>
  /// <param name="key">键(模糊匹配)</param>
  /// <returns>分页追溯工艺项目列表</returns>
  public async Task<PaginatedList<TraceProcItem>> PaginationAsync(int current, int pageSize, string? id, string? station, string? key)
  {
    var query = _repository.GetQueryable();

    // 根据ID进行模糊匹配
    if (!string.IsNullOrEmpty(id))
    {
      query = query.Where(x => x.Id.ToString().Contains(id));
    }

    // 根据工位位置进行模糊匹配
    if (!string.IsNullOrEmpty(station))
    {
      query = query.Where(x => x.Station.Contains(station));
    }

    // 根据键进行模糊匹配
    if (!string.IsNullOrEmpty(key))
    {
      query = query.Where(x => x.Key.Contains(key));
    }

    return await query.RetrievePagedListAsync(current, pageSize);
  }

  /// <summary>
  /// 根据条件查询追溯工艺项目列表（支持分页）
  /// </summary>
  /// <param name="id">ID</param>
  /// <param name="station">工位位置</param>
  /// <param name="key">键</param>
  /// <param name="page">页码</param>
  /// <param name="size">每页大小</param>
  /// <returns>分页追溯工艺项目列表</returns>
  public async Task<PaginatedList<TraceProcItem>> GetListAsync(Guid? id, string? station, string? key, int page, int size)
  {
    var queryable = _repository.GetQueryable();
    
    if (id.HasValue)
        queryable = queryable.Where(x => x.Id == id.Value);
        
    if (!string.IsNullOrEmpty(station))
        queryable = queryable.Where(x => x.Station.Contains(station));
        
    if (!string.IsNullOrEmpty(key))
        queryable = queryable.Where(x => x.Key.Contains(key));
        
    var total = await queryable.CountAsync();
    var items = await queryable.OrderByDescending(x => x.CreatedAt)
        .Skip((page - 1) * size)
        .Take(size)
        .ToListAsync();
        
    return new PaginatedList<TraceProcItem>(items, total, page, size);
  }

  /// <summary>
  /// 根据ID获取追溯工艺项目
  /// </summary>
  /// <param name="id">追溯工艺项目ID</param>
  /// <returns>追溯工艺项目实体</returns>
  public async Task<TraceProcItem?> GetByIdAsync(Guid id)
  {
    var query = _repository.GetQueryable();
    return await query.FirstOrDefaultAsync(x => x.Id == id);
  }

  /// <summary>
  /// 根据追溯信息ID获取追溯工艺项目列表
  /// </summary>
  /// <param name="traceInfoId">追溯信息ID</param>
  /// <returns>追溯工艺项目实体列表</returns>
  public async Task<List<TraceProcItem>> GetByTraceInfoIdAsync(Guid traceInfoId)
  {
    var query = _repository.GetQueryable();
    return await query.Where(x => x.TraceInfoId == traceInfoId).ToListAsync();
  }

  /// <summary>
  /// 批量删除追溯工艺项目
  /// </summary>
  /// <param name="ids">追溯工艺项目ID列表</param>
  /// <returns>删除结果</returns>
  public async Task<bool> DeleteByIdsAsync(List<Guid> ids)
  {
    var entities = _repository.GetQueryable().Where(x => ids.Contains(x.Id)).ToList();
    if (entities.Any())
    {
      var now = DateTimeOffset.UtcNow;
      foreach (var entity in entities)
      {
        entity.IsDeleted = true;
        entity.DeletedAt = now;
        await _repository.UpdateAsync(entity);
      }
      return true;
    }
    return false;
  }
}