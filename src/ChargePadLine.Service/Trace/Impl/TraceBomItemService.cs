using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.DbContexts.Repository;
using Microsoft.EntityFrameworkCore;
using ChargePadLine.Service;

namespace ChargePadLine.Service.Trace.Impl;

/// <summary>
/// 追溯BOM项目服务实现
/// </summary>
public class TraceBomItemService : ITraceBomItemService
{
  private readonly IRepository<TraceBomItem> _repository;

  public TraceBomItemService(IRepository<TraceBomItem> repository)
  {
    _repository = repository;
  }

  /// <summary>
  /// 分页查询追溯BOM项目
  /// </summary>
  /// <param name="current">当前页码</param>
  /// <param name="pageSize">每页记录数</param>
  /// <param name="id">ID(模糊匹配)</param>
  /// <param name="materialName">物料名称(模糊匹配)</param>
  /// <param name="materialCode">物料代码(模糊匹配)</param>
  /// <returns>分页追溯BOM项目列表</returns>
  public async Task<PaginatedList<TraceBomItem>> PaginationAsync(int current, int pageSize, string? id, string? materialName, string? materialCode)
  {
    var query = _repository.GetQueryable();

    // 根据ID进行模糊匹配
    if (!string.IsNullOrEmpty(id))
    {
      query = query.Where(x => x.Id.ToString().Contains(id));
    }

    // 根据物料名称进行模糊匹配
    if (!string.IsNullOrEmpty(materialName))
    {
      query = query.Where(x => x.MaterialName.Contains(materialName));
    }

    // 根据物料代码进行模糊匹配
    if (!string.IsNullOrEmpty(materialCode))
    {
      query = query.Where(x => x.MaterialCode != null && x.MaterialCode.Value.Contains(materialCode));
    }

    return await query.RetrievePagedListAsync(current, pageSize);
  }

  /// <summary>
  /// 根据条件查询追溯BOM项目列表（支持分页）
  /// </summary>
  /// <param name="id">ID</param>
  /// <param name="materialName">物料名称</param>
  /// <param name="materialCode">物料代码</param>
  /// <param name="page">页码</param>
  /// <param name="size">每页大小</param>
  /// <returns>分页追溯BOM项目列表</returns>
  public async Task<PaginatedList<TraceBomItem>> GetListAsync(Guid? id, string? materialName, string? materialCode, int page, int size)
  {
    var queryable = _repository.GetQueryable();

    if (id.HasValue)
      queryable = queryable.Where(x => x.Id == id.Value);

    if (!string.IsNullOrEmpty(materialName))
      queryable = queryable.Where(x => x.MaterialName.Contains(materialName));

    if (!string.IsNullOrEmpty(materialCode))
      queryable = queryable.Where(x => x.MaterialCode != null && x.MaterialCode.Value.Contains(materialCode));

    var total = await queryable.CountAsync();
    var items = await queryable.OrderByDescending(x => x.CreatedAt)
        .Skip((page - 1) * size)
        .Take(size)
        .ToListAsync();

    return new PaginatedList<TraceBomItem>(items, total, page, size);
  }

  /// <summary>
  /// 根据ID获取追溯BOM项目
  /// </summary>
  /// <param name="id">追溯BOM项目ID</param>
  /// <returns>追溯BOM项目实体</returns>
  public async Task<TraceBomItem?> GetByIdAsync(Guid id)
  {
    var query = _repository.GetQueryable();
    return await query.FirstOrDefaultAsync(x => x.Id == id);
  }

  /// <summary>
  /// 根据追溯信息ID获取追溯BOM项目列表
  /// </summary>
  /// <param name="traceInfoId">追溯信息ID</param>
  /// <returns>追溯BOM项目实体列表</returns>
  public async Task<List<TraceBomItem>> GetByTraceInfoIdAsync(Guid traceInfoId)
  {
    var query = _repository.GetQueryable();
    return await query.Where(x => x.TraceInfoId == traceInfoId).ToListAsync();
  }

  /// <summary>
  /// 批量删除追溯BOM项目
  /// </summary>
  /// <param name="ids">追溯BOM项目ID列表</param>
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