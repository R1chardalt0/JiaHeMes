using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.DbContexts.Repository;
using Microsoft.EntityFrameworkCore;
using ChargePadLine.Service;

namespace ChargePadLine.Service.Trace.Impl;

/// <summary>
/// 追溯信息服务实现
/// </summary>
public class TraceInfoService : ITraceInfoService
{
  private readonly IRepository<TraceInfo> _repository;

  public TraceInfoService(IRepository<TraceInfo> repository)
  {
    _repository = repository;
  }

  /// <summary>
  /// 分页查询追溯信息
  /// </summary>
  /// <param name="current">当前页码</param>
  /// <param name="pageSize">每页记录数</param>
  /// <param name="id">ID(模糊匹配)</param>
  /// <param name="pin">产品识别码(模糊匹配)</param>
  /// <param name="vsn">VSN(模糊匹配)</param>
  /// <returns>分页追溯信息列表</returns>
  public async Task<PaginatedList<TraceInfo>> PaginationAsync(int current, int pageSize, string? id, string? pin, string? vsn)
  {
    var query = _repository.GetQueryable();

    // 根据ID进行模糊匹配
    if (!string.IsNullOrEmpty(id))
    {
      query = query.Where(x => x.Id.ToString().Contains(id));
    }

    // 根据产品识别码进行模糊匹配
    if (!string.IsNullOrEmpty(pin))
    {
      query = query.Where(x => x.PIN.Value.Contains(pin));
    }

    // 根据VSN进行模糊匹配
    if (!string.IsNullOrEmpty(vsn))
    {
      if (uint.TryParse(vsn, out var vsnValue))
      {
        query = query.Where(x => x.Vsn == vsnValue);
      }
      else
      {
        // 如果无法解析为数字，则按字符串包含匹配
        query = query.Where(x => x.Vsn.ToString().Contains(vsn));
      }
    }

    return await query.RetrievePagedListAsync(current, pageSize);
  }

  /// <summary>
  /// 根据条件查询追溯信息列表（支持分页）
  /// </summary>
  /// <param name="id">ID</param>
  /// <param name="pin">产品识别码</param>
  /// <param name="vsn">VSN</param>
  /// <param name="productCode">产品编码</param>
  /// <param name="page">页码</param>
  /// <param name="size">每页大小</param>
  /// <returns>分页追溯信息列表</returns>
  public async Task<PaginatedList<TraceInfo>> GetListAsync(Guid? id, string? pin, string? vsn, string? productCode, int page, int size)
  {
    var queryable = _repository.GetQueryable();

    if (id.HasValue)
      queryable = queryable.Where(x => x.Id == id.Value);

    if (!string.IsNullOrEmpty(pin))
      queryable = queryable.Where(x => x.PIN.Value.Contains(pin));

    if (!string.IsNullOrEmpty(vsn))
    {
      if (uint.TryParse(vsn, out var vsnValue))
      {
        queryable = queryable.Where(x => x.Vsn == vsnValue);
      }
      else
      {
        // 如果无法解析为数字，则按字符串包含匹配
        queryable = queryable.Where(x => x.Vsn.ToString().Contains(vsn));
      }
    }

    if (!string.IsNullOrEmpty(productCode))
      queryable = queryable.Where(x => x.ProductCode.Value.Contains(productCode));

    var total = await queryable.CountAsync();
    var items = await queryable.OrderByDescending(x => x.CreatedAt)
        .Skip((page - 1) * size)
        .Take(size)
        .ToListAsync();

    return new PaginatedList<TraceInfo>(items, total, page, size);
  }

  /// <summary>
  /// 根据ID获取追溯信息
  /// </summary>
  /// <param name="id">追溯信息ID</param>
  /// <returns>追溯信息实体</returns>
  public async Task<TraceInfo?> GetByIdAsync(Guid id)
  {
    var query = _repository.GetQueryable();
    return await query.FirstOrDefaultAsync(x => x.Id == id);
  }

  /// <summary>
  /// 批量删除追溯信息
  /// </summary>
  /// <param name="ids">追溯信息ID列表</param>
  /// <returns>删除结果</returns>
  public async Task<bool> DeleteByIdsAsync(List<Guid> ids)
  {
    var entities = _repository.GetQueryable().Where(x => ids.Contains(x.Id)).ToList();
    if (entities.Any())
    {
      foreach (var entity in entities)
      {
        await _repository.DeleteAsync(entity);
      }
      return true;
    }
    return false;
  }
}